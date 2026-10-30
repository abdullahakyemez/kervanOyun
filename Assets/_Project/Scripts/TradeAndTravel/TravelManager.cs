using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    public readonly struct TravelCompletedEvent : IGameEvent
    {
        public readonly LocationNodeSO Destination;
        public readonly TravelRouteSO Route;

        public TravelCompletedEvent(LocationNodeSO dest, TravelRouteSO route)
        {
            Destination = dest;
            Route = route;
        }
    }

    /// <summary>
    /// Seyahat sırasında bir yol hadisesi (<see cref="RoadEventSO"/>) tetiklendiğinde yayınlanır.
    /// </summary>
    public readonly struct RoadEventTriggeredEvent : IGameEvent
    {
        public readonly RoadEventSO RoadEvent;
        public readonly string OutcomeSummary;

        public RoadEventTriggeredEvent(RoadEventSO roadEvent, string outcomeSummary)
        {
            RoadEvent = roadEvent;
            OutcomeSummary = outcomeSummary;
        }
    }

    /// <summary>
    /// Pusu veya tehlikeli yol olayında oyuncunun seçimi (Kaç / Haraç Ver / Savaş) sonucunu taşıyan veri yapısı.
    /// </summary>
    public readonly struct AmbushResolutionResult
    {
        public readonly bool Success;
        public readonly bool ShouldEnterCombat;
        public readonly string Message;
        public readonly string OutcomeSummary;

        public AmbushResolutionResult(bool success, bool shouldEnterCombat, string message, string outcomeSummary)
        {
            Success = success;
            ShouldEnterCombat = shouldEnterCombat;
            Message = message;
            OutcomeSummary = outcomeSummary;
        }
    }

    /// <summary>
    /// Kervanın seyahatini, yol şartlarını, erzak tüketimini ve
    /// pusu/olay zarlarını yöneten merkezi seyahat motoru.
    /// Tüm kervan ve takvim durumunu doğrudan <see cref="GameSession"/> üzerinden okur ve günceller (BUG-05).
    /// </summary>
    public class TravelManager : MonoBehaviour
    {
        private const float DefaultDayDurationSeconds = 1.2f;
        private const float AmbushChanceFactor = 0.35f;
        private const float DefaultDailyEventChance = 0.35f;

        [Header("Rota Veri Tabanı & Test Fallback")]
        [Tooltip("Aktif rotayı RouteId veya From/To NodeId üzerinden çözümlemek için kullanılan rota veri tabanı")]
        [SerializeField] private RouteDatabaseSO _routeDatabase;

        [Tooltip("Doğrudan TravelScene'den Play'e basıldığında veya ActiveTravel boş olduğunda kullanılacak varsayılan rota")]
        [SerializeField] private TravelRouteSO _activeRoute;

        [Header("Yol Olayları Havuzu (RoadEventSO)")]
        [Tooltip("Rotada özel olay listesi tanımlanmadığında kullanılacak genel yol olayları havuzu")]
        [SerializeField] private List<RoadEventSO> _defaultRoadEvents = new List<RoadEventSO>();

        [Tooltip("Her gün sonunda bir yol hadisesi tetiklenme olasılığı (0.0 = Hiç, 1.0 = Her gün)")]
        [Range(0f, 1f)]
        [SerializeField] private float _dailyEventChance = DefaultDailyEventChance;

        [Header("Seyahat Durumu & Tempo")]
        [Tooltip("Seyahat şu anda akıyor mu?")]
        [SerializeField] private bool _isTraveling;

        [Tooltip("Rota üzerinde geride bırakılan gün ilerlemesi (0.0 .. TotalDays)")]
        [SerializeField] private float _daysProgress;

        [Tooltip("1x tempoda 1 oyun gününün kaç gerçek saniye süreceği")]
        [Range(0.5f, 15f)]
        [SerializeField] private float _dayDurationSeconds = DefaultDayDurationSeconds;

        [Tooltip("Yürüyüş temposu çarpanı (1 = 1x normal, 2 = 2x hızlı)")]
        [Range(0.5f, 4f)]
        [SerializeField] private float _speedMultiplier = 1f;

        private GameSession _gameSession;
        private CaravanData _fallbackCaravan;
        private LocationNodeSO _originNode;
        private LocationNodeSO _destinationNode;
        private int _totalRouteDays = 1;
        private int _lastDayRecorded;
        private List<RoadEventSO> _runtimeFallbackEvents;
        private RoadEventSO _activeRoadEvent;

        // Olaylar
        public event Action<float, int> OnTravelProgressUpdated; // progress (0..1), globalDay
        public event Action<LocationNodeSO> OnDestinationReached;
        public event Action<TravelRouteSO> OnAmbushTriggered;
        public event Action<RoadEventSO, string> OnRoadEventTriggered; // roadEvent, outcomeSummary
        public event Action<string, string> OnRandomEventTriggered; // başlık, açıklama (geriye dönük uyumluluk)

        /// <summary>
        /// O an tetiklenmiş olan ve çözüm bekleyen yol hadisesi veya pusu referansı.
        /// </summary>
        public RoadEventSO ActiveRoadEvent => _activeRoadEvent;

        /// <summary>
        /// Tek doğruluk kaynağı olan <see cref="GameSession.Caravan"/> referansı (BUG-05 çözümü).
        /// </summary>
        public CaravanData Caravan
        {
            get
            {
                if (_gameSession != null && _gameSession.Caravan != null)
                {
                    return _gameSession.Caravan;
                }

                if (Services.TryGet<GameSession>(out _gameSession) && _gameSession.Caravan != null)
                {
                    return _gameSession.Caravan;
                }

                if (CaravanManager.Instance != null && CaravanManager.Instance.Caravan != null)
                {
                    return CaravanManager.Instance.Caravan;
                }

                if (_fallbackCaravan == null)
                {
                    _fallbackCaravan = new CaravanData();
                }

                return _fallbackCaravan;
            }
        }

        public LocationNodeSO OriginNode => _originNode != null ? _originNode : (_activeRoute != null ? _activeRoute.FromNode : null);
        public LocationNodeSO DestinationNode => _destinationNode != null ? _destinationNode : (_activeRoute != null ? _activeRoute.ToNode : null);
        public LocationNodeSO CurrentLocation => _destinationNode;
        public TravelRouteSO ActiveRoute => _activeRoute;
        public bool IsTraveling => _isTraveling;
        public float SpeedMultiplier => _speedMultiplier;
        public int TotalRouteDays => _totalRouteDays;
        public int CurrentRouteDay => Mathf.Clamp(Mathf.FloorToInt(_daysProgress) + 1, 1, _totalRouteDays);
        public int GlobalDayCount => _gameSession != null ? _gameSession.DayCount : 1;

        private void Start()
        {
            Services.TryGet<GameSession>(out _gameSession);
            InitializeTravelFromSessionOrFallback();
        }

        private void Update()
        {
            if (!_isTraveling || _activeRoute == null)
            {
                return;
            }

            float safeDuration = Mathf.Max(0.1f, _dayDurationSeconds);
            float dayDelta = (Time.deltaTime * _speedMultiplier) / safeDuration;
            _daysProgress += dayDelta;

            int completedDays = Mathf.FloorToInt(_daysProgress);
            while (completedDays > _lastDayRecorded && _isTraveling)
            {
                _lastDayRecorded++;

                if (_gameSession != null)
                {
                    if (_gameSession.ActiveTravel != null)
                    {
                        _gameSession.ActiveTravel.DaysElapsed = _lastDayRecorded;
                    }

                    _gameSession.AdvanceDays(1);
                }

                // Son güne ulaşıldıysa varış gerçekleşir, pusu zarı atılmaz
                if (_lastDayRecorded < _totalRouteDays)
                {
                    ProcessDailyLogistics();
                }
                else
                {
                    ConsumeDailyFoodOnly();
                }
            }

            float normalizedProgress = Mathf.Clamp01(_daysProgress / _totalRouteDays);
            OnTravelProgressUpdated?.Invoke(normalizedProgress, GlobalDayCount);

            if (_daysProgress >= _totalRouteDays && _isTraveling)
            {
                ArriveAtDestination();
            }
        }

        /// <summary>
        /// Belirtilen rota üzerinde yeni bir seyahat başlatır.
        /// </summary>
        public void StartTravel(TravelRouteSO route, LocationNodeSO customOrigin = null, LocationNodeSO customDestination = null, int initialDaysElapsed = 0, int overrideTotalDays = 0)
        {
            if (route == null)
            {
                Debug.LogWarning("[TravelManager] Başlatılacak rota (TravelRouteSO) atanmamış!", this);
                return;
            }

            _activeRoute = route;
            _originNode = customOrigin != null ? customOrigin : route.FromNode;
            _destinationNode = customDestination != null ? customDestination : route.ToNode;
            _totalRouteDays = overrideTotalDays > 0 ? overrideTotalDays : Mathf.Max(1, route.DistanceDays);

            int clampedInitialDays = Mathf.Clamp(initialDaysElapsed, 0, _totalRouteDays - 1);
            _daysProgress = clampedInitialDays;
            _lastDayRecorded = clampedInitialDays;
            _isTraveling = true;

            float normalizedProgress = Mathf.Clamp01(_daysProgress / _totalRouteDays);
            OnTravelProgressUpdated?.Invoke(normalizedProgress, GlobalDayCount);

            Debug.Log($"[TravelManager] Seyahat başladı: {OriginNode?.NodeName} -> {DestinationNode?.NodeName} ({_totalRouteDays} Gün)");
        }

        /// <summary>
        /// Seyahati geçici olarak duraklatır (Mola / Kamp durumu).
        /// </summary>
        public void PauseTravel()
        {
            _isTraveling = false;
        }

        /// <summary>
        /// Mola durumundaki seyahati kaldığı yerden devam ettirir.
        /// </summary>
        public void ResumeTravel()
        {
            if (_activeRoute != null && _daysProgress < _totalRouteDays)
            {
                _isTraveling = true;
            }
        }

        /// <summary>
        /// Seyahat hız çarpanını günceller (1f = 1x, 2f = 2x tempo).
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Clamp(multiplier, 0.5f, 4f);
        }

        private void InitializeTravelFromSessionOrFallback()
        {
            if (_gameSession != null && _gameSession.ActiveTravel != null)
            {
                ActiveTravelState active = _gameSession.ActiveTravel;
                TravelRouteSO resolvedRoute = ResolveRouteFromDatabase(active.RouteId, active.FromNodeId, active.ToNodeId);

                if (resolvedRoute != null)
                {
                    DetermineDirectionNodes(resolvedRoute, active.FromNodeId, active.ToNodeId, out LocationNodeSO origin, out LocationNodeSO dest);
                    StartTravel(resolvedRoute, origin, dest, active.DaysElapsed, active.TotalDays);
                    return;
                }
            }

            // Doğrudan TravelScene içinden Play'e basıldıysa Inspector'daki veya veri tabanındaki ilk rotayı kullan
            if (_activeRoute == null && _routeDatabase != null && _routeDatabase.Routes != null && _routeDatabase.Routes.Count > 0)
            {
                _activeRoute = _routeDatabase.Routes[0];
            }

            if (_activeRoute != null)
            {
                StartTravel(_activeRoute);
            }
            else
            {
                Debug.LogWarning("[TravelManager] Aktif seyahat veya varsayılan rota bulunamadı. TravelManager Inspector'ından RouteDatabase veya ActiveRoute atayın.", this);
            }
        }

        private TravelRouteSO ResolveRouteFromDatabase(string routeId, string fromNodeId, string toNodeId)
        {
            if (_routeDatabase != null && _routeDatabase.Routes != null)
            {
                var routes = _routeDatabase.Routes;

                // 1. Önce RouteId ile eşleştir
                if (!string.IsNullOrEmpty(routeId))
                {
                    for (int i = 0; i < routes.Count; i++)
                    {
                        TravelRouteSO r = routes[i];
                        if (r != null && string.Equals(r.RouteId, routeId, StringComparison.Ordinal))
                        {
                            return r;
                        }
                    }
                }

                // 2. FromNodeId ve ToNodeId (çift yönlü) ile eşleştir
                for (int i = 0; i < routes.Count; i++)
                {
                    TravelRouteSO r = routes[i];
                    if (r == null || r.FromNode == null || r.ToNode == null)
                    {
                        continue;
                    }

                    string a = r.FromNode.NodeId;
                    string b = r.ToNode.NodeId;

                    bool forward = string.Equals(a, fromNodeId, StringComparison.Ordinal) && string.Equals(b, toNodeId, StringComparison.Ordinal);
                    bool reverse = string.Equals(b, fromNodeId, StringComparison.Ordinal) && string.Equals(a, toNodeId, StringComparison.Ordinal);

                    if (forward || reverse)
                    {
                        return r;
                    }
                }
            }

            return _activeRoute;
        }

        private static void DetermineDirectionNodes(TravelRouteSO route, string fromNodeId, string toNodeId, out LocationNodeSO origin, out LocationNodeSO dest)
        {
            origin = route.FromNode;
            dest = route.ToNode;

            if (route.FromNode != null && route.ToNode != null)
            {
                if (string.Equals(route.ToNode.NodeId, fromNodeId, StringComparison.Ordinal) &&
                    string.Equals(route.FromNode.NodeId, toNodeId, StringComparison.Ordinal))
                {
                    origin = route.ToNode;
                    dest = route.FromNode;
                }
            }
        }

        private void ConsumeDailyFoodOnly()
        {
            if (_activeRoute == null)
            {
                return;
            }

            int foodToConsume = Mathf.Max(1, Mathf.RoundToInt(1f * _activeRoute.FoodConsumptionMultiplier));
            Caravan.ConsumeFood(foodToConsume);
        }

        private void ProcessDailyLogistics()
        {
            if (_activeRoute == null)
            {
                return;
            }

            int foodToConsume = Mathf.Max(1, Mathf.RoundToInt(1f * _activeRoute.FoodConsumptionMultiplier));
            bool hasFood = Caravan.ConsumeFood(foodToConsume);

            if (!hasFood)
            {
                Debug.LogWarning("[TravelManager] Erzak tükendi! Kervan muhafızları aç ve moralleri düşüyor.");
                RoadEventSO starvationEvent = RoadEventSO.CreateRuntimeInstance(
                    "event_starvation_warning",
                    "Açlık Tehlikesi!",
                    "Kervanın erzak çuvalları boşaldı. Muhafızlar yorgun ve hoşnutsuz bir halde yola devam etmeye çalışıyor.",
                    RoadEventCategory.Incident,
                    0,
                    0,
                    -5);
                TriggerSelectedRoadEvent(starvationEvent);
                return;
            }

            CheckRandomEncounters();
        }

        private void CheckRandomEncounters()
        {
            if (_activeRoute == null)
            {
                return;
            }

            float ambushRoll = UnityEngine.Random.value;

            if (ambushRoll <= _activeRoute.DangerLevel * AmbushChanceFactor)
            {
                _isTraveling = false;
                RoadEventSO ambushEvent = SelectWeightedRoadEvent(ambushOnly: true);
                Debug.Log($"[TravelManager] PUSU / SALDIRI! ({ambushEvent?.EventTitle})");
                OnAmbushTriggered?.Invoke(_activeRoute);

                if (ambushEvent != null)
                {
                    TriggerSelectedRoadEvent(ambushEvent);
                }

                return;
            }

            float incidentRoll = UnityEngine.Random.value;
            if (incidentRoll <= _dailyEventChance)
            {
                RoadEventSO selectedEvent = SelectWeightedRoadEvent(ambushOnly: false);
                if (selectedEvent != null)
                {
                    TriggerSelectedRoadEvent(selectedEvent);
                }
            }
        }

        private void TriggerSelectedRoadEvent(RoadEventSO roadEvent)
        {
            if (roadEvent == null)
            {
                return;
            }

            _activeRoadEvent = roadEvent;

            string outcomeSummary;
            if (roadEvent.RequiresCombatChoice)
            {
                // Karar gerektiren pusularda anlık cezalar hemen uygulanmaz, oyuncunun seçimi beklenir.
                outcomeSummary = string.Empty;
            }
            else
            {
                ApplyRoadEventEffects(roadEvent);
                outcomeSummary = roadEvent.BuildOutcomeSummary();
            }

            OnRoadEventTriggered?.Invoke(roadEvent, outcomeSummary);
            OnRandomEventTriggered?.Invoke(roadEvent.EventTitle, roadEvent.EventDescription);
            EventBus.Publish(new RoadEventTriggeredEvent(roadEvent, outcomeSummary));
        }

        /// <summary>
        /// Pusu olayında oyuncu 'Kaç' seçeneğini seçtiğinde kaçış denemesini çözümler.
        /// Başarılı olursa kervan hasarsız kurtulur; başarısız olursa yıpranma ile savaşa yönlendirilir.
        /// </summary>
        public AmbushResolutionResult ResolveAmbushFlee()
        {
            RoadEventSO roadEvent = _activeRoadEvent;
            float fleeChance = roadEvent != null ? roadEvent.FleeChance : 0.65f;
            float roll = UnityEngine.Random.value;

            if (roll <= fleeChance)
            {
                _activeRoadEvent = null;
                Debug.Log($"[TravelManager] PUSUDAN KAÇIŞ BAŞARILI! (Zar: {roll:F2} <= Şans: {fleeChance:F2})");
                return new AmbushResolutionResult(
                    success: true,
                    shouldEnterCombat: false,
                    message: "Kervan çevik bir manevrayla dağ geçidinde izini kaybettirdi. Pusudan sıyrılıp güvenle yola devam ediyorsunuz.",
                    outcomeSummary: "Pusu Savuşturuldu · Hasar Alınmadı");
            }
            else
            {
                int penalty = -5;
                Caravan.HealAllGuards(penalty);
                Debug.Log($"[TravelManager] PUSUDAN KAÇIŞ BAŞARISIZ! (Zar: {roll:F2} > Şans: {fleeChance:F2})");
                return new AmbushResolutionResult(
                    success: false,
                    shouldEnterCombat: true,
                    message: "Kaçış başarısız oldu! Düşmanlar kervanın arkasını sardı. Muhafızlar panikle savunmaya çekildi, çatışma kaçınılmaz!",
                    outcomeSummary: "Kaçış Başarısız · Muhafızlarda -5 Yıpranma");
            }
        }

        /// <summary>
        /// Pusu olayında oyuncu 'Haraç Ver' seçeneğini seçtiğinde akçe ödemesini gerçekleştirir.
        /// </summary>
        public AmbushResolutionResult ResolveAmbushTribute()
        {
            RoadEventSO roadEvent = _activeRoadEvent;
            if (roadEvent != null && roadEvent.Category == RoadEventCategory.WildBeastAttack)
            {
                return new AmbushResolutionResult(
                    success: false,
                    shouldEnterCombat: false,
                    message: "Yabani hayvanlar akçe kabul etmez!",
                    outcomeSummary: "Geçersiz Seçim");
            }

            int cost = roadEvent != null && roadEvent.TributeAkceCost > 0 ? roadEvent.TributeAkceCost : 30;

            if (Caravan.CurrentAkce < cost)
            {
                return new AmbushResolutionResult(
                    success: false,
                    shouldEnterCombat: false,
                    message: $"Eşkıyaların istediği {cost} Akçe kervan kesesinde bulunmuyor!",
                    outcomeSummary: "Yetersiz Akçe");
            }

            Caravan.ModifyAkce(-cost);
            _activeRoadEvent = null;
            Debug.Log($"[TravelManager] HARAÇ ÖDENDİ: -{cost} Akçe");

            return new AmbushResolutionResult(
                success: true,
                shouldEnterCombat: false,
                message: $"Eşkıyalar {cost} Akçeyi sayıp memnuniyetle yolu açtı. Kervan kan dökülmeden seyahate devam ediyor.",
                outcomeSummary: $"-{cost} Akçe Ödendi · Yol Açıldı");
        }

        /// <summary>
        /// Pusu olayında oyuncu 'Savaş' seçeneğini seçtiğinde savaşa geçiş kararını onaylar.
        /// </summary>
        public AmbushResolutionResult ResolveAmbushFight()
        {
            Debug.Log("[TravelManager] SAVAŞ SEÇİLDİ!");
            return new AmbushResolutionResult(
                success: true,
                shouldEnterCombat: true,
                message: "Kervan muhafızları kılıçlarını sıyırdı, kalkanlar birleşti. Meydan savaşı başlıyor!",
                outcomeSummary: "Savaşa Giriliyor");
        }

        private void ApplyRoadEventEffects(RoadEventSO roadEvent)
        {
            if (roadEvent.AkceDelta != 0)
            {
                int delta = roadEvent.AkceDelta;
                if (delta < 0 && Caravan.CurrentAkce + delta < 0)
                {
                    delta = -Caravan.CurrentAkce;
                }

                if (delta != 0)
                {
                    Caravan.ModifyAkce(delta);
                }
            }

            if (roadEvent.FoodDelta > 0)
            {
                Caravan.AddFood(roadEvent.FoodDelta);
            }
            else if (roadEvent.FoodDelta < 0)
            {
                Caravan.ConsumeFood(-roadEvent.FoodDelta);
            }

            if (roadEvent.HealDelta != 0)
            {
                Caravan.HealAllGuards(roadEvent.HealDelta);
            }
        }

        private RoadEventSO SelectWeightedRoadEvent(bool ambushOnly)
        {
            IReadOnlyList<RoadEventSO> candidatePool = GetActiveEventPool();
            float totalWeight = 0f;

            for (int i = 0; i < candidatePool.Count; i++)
            {
                RoadEventSO candidate = candidatePool[i];
                if (candidate == null || !IsCategoryMatch(candidate.Category, ambushOnly))
                {
                    continue;
                }

                totalWeight += candidate.SelectionWeight;
            }

            // Eğer rotada/havuzda istenen kategoride olay yoksa tüm havuzdan veya çalışma anı yedeklerinden seç
            if (totalWeight <= 0f)
            {
                candidatePool = GetOrCreateRuntimeFallbackPool();
                for (int i = 0; i < candidatePool.Count; i++)
                {
                    RoadEventSO candidate = candidatePool[i];
                    if (candidate == null || !IsCategoryMatch(candidate.Category, ambushOnly))
                    {
                        continue;
                    }

                    totalWeight += candidate.SelectionWeight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float pick = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < candidatePool.Count; i++)
            {
                RoadEventSO candidate = candidatePool[i];
                if (candidate == null || !IsCategoryMatch(candidate.Category, ambushOnly))
                {
                    continue;
                }

                cumulative += candidate.SelectionWeight;
                if (pick <= cumulative)
                {
                    return candidate;
                }
            }

            return candidatePool[candidatePool.Count - 1];
        }

        private static bool IsCategoryMatch(RoadEventCategory category, bool ambushOnly)
        {
            bool isAmbushType = category == RoadEventCategory.BanditAmbush || category == RoadEventCategory.WildBeastAttack;
            return ambushOnly ? isAmbushType : true;
        }

        private IReadOnlyList<RoadEventSO> GetActiveEventPool()
        {
            if (_activeRoute != null && _activeRoute.RoadEvents != null && _activeRoute.RoadEvents.Count > 0)
            {
                return _activeRoute.RoadEvents;
            }

            if (_defaultRoadEvents != null && _defaultRoadEvents.Count > 0)
            {
                return _defaultRoadEvents;
            }

            return GetOrCreateRuntimeFallbackPool();
        }

        private List<RoadEventSO> GetOrCreateRuntimeFallbackPool()
        {
            if (_runtimeFallbackEvents != null && _runtimeFallbackEvents.Count > 0)
            {
                return _runtimeFallbackEvents;
            }

            _runtimeFallbackEvents = new List<RoadEventSO>
            {
                RoadEventSO.CreateRuntimeInstance(
                    "event_broken_axle",
                    "Yük Bağları Çözüldü",
                    "Engebeli geçitte kervan katırlarından birinin yük semeri hasar gördü. Yakındaki han ustasına onarım bedeli ödendi.",
                    RoadEventCategory.Incident,
                    akceDelta: -25,
                    foodDelta: 0,
                    healDelta: 0,
                    weight: 1.0f),

                RoadEventSO.CreateRuntimeInstance(
                    "event_wandering_dervish",
                    "Gezgin Horasan Dervişi",
                    "Yol kenarındaki çeşme başında karşılaşılan bir Horasan ereni kervana hayır duası etti ve şifalı otlar ikram etti.",
                    RoadEventCategory.Positive,
                    akceDelta: 0,
                    foodDelta: 0,
                    healDelta: 10,
                    weight: 1.0f),

                RoadEventSO.CreateRuntimeInstance(
                    "event_road_provisions",
                    "Yol Azığı Bulundu",
                    "Konaklama menzilinde dost bir Yörük obası kervana taze kurutulmuş et ve peksimet ikram etti.",
                    RoadEventCategory.Positive,
                    akceDelta: 0,
                    foodDelta: 3,
                    healDelta: 0,
                    weight: 1.0f),

                RoadEventSO.CreateRuntimeInstance(
                    "event_bandit_ambush",
                    "Dağ Eşkıyaları Yolu Kesti!",
                    "Dar boğazda pusuya yatan başıbozuk eşkıyalar kervanın önünü kesti. Muhafızlar kılıç sıyırıp kervan yükünü savundu!",
                    RoadEventCategory.BanditAmbush,
                    akceDelta: -15,
                    foodDelta: 0,
                    healDelta: -5,
                    weight: 0.7f,
                    tributeCost: 30,
                    fleeChance: 0.65f),

                RoadEventSO.CreateRuntimeInstance(
                    "event_wolf_pack",
                    "Bozkır Kurt Sürüsü!",
                    "Alacakaranlıkta aç bir kurt sürüsü kervan hayvanlarının kokusunu alarak yaklaştı. Okçular meşale ve ok yağmuruyla sürüyü püskürttü.",
                    RoadEventCategory.WildBeastAttack,
                    akceDelta: 0,
                    foodDelta: -1,
                    healDelta: -5,
                    weight: 0.7f,
                    tributeCost: 0,
                    fleeChance: 0.60f)
            };

            return _runtimeFallbackEvents;
        }

        private void ArriveAtDestination()
        {
            _isTraveling = false;
            LocationNodeSO arrivedNode = DestinationNode;
            TravelRouteSO completedRoute = _activeRoute;

            if (_gameSession != null)
            {
                if (_gameSession.ActiveTravel != null)
                {
                    _gameSession.CompleteTravel();
                }
                else if (arrivedNode != null && !string.IsNullOrEmpty(arrivedNode.NodeId))
                {
                    _gameSession.SetCurrentNode(arrivedNode.NodeId);
                }
            }

            Debug.Log($"[TravelManager] Hedefe varıldı: {arrivedNode?.NodeName}");
            OnDestinationReached?.Invoke(arrivedNode);
            EventBus.Publish(new TravelCompletedEvent(arrivedNode, completedRoute));
        }

        public bool RestAtCaravanserai(int days)
        {
            LocationNodeSO loc = DestinationNode;
            if (loc == null || !loc.HasInn)
            {
                return false;
            }

            int totalCost = loc.DailyRestCost * days;
            if (!Caravan.ModifyAkce(-totalCost))
            {
                return false;
            }

            if (_gameSession != null)
            {
                _gameSession.AdvanceDays(days);
            }

            Caravan.HealAllGuards(loc.DailyHealAmount * days);
            Caravan.ConsumeFood(days);

            Debug.Log($"[TravelManager] {days} gün kervansarayda dinlenildi. Masraf: {totalCost} Akçe.");
            return true;
        }
    }
}
