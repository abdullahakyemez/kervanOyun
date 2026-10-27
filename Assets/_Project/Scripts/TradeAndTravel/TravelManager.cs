using System;
using UnityEngine;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    public struct TravelCompletedEvent : IGameEvent
    {
        public LocationNodeSO Destination;
        public TravelRouteSO Route;

        public TravelCompletedEvent(LocationNodeSO dest, TravelRouteSO route)
        {
            Destination = dest;
            Route = route;
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
        private const float AmbushChanceFactor = 0.40f;
        private const float IncidentThreshold = 0.85f;
        private const int BrokenAxleCost = 25;
        private const int FoundFoodAmount = 3;

        [Header("Rota Veri Tabanı & Test Fallback")]
        [Tooltip("Aktif rotayı RouteId veya From/To NodeId üzerinden çözümlemek için kullanılan rota veri tabanı")]
        [SerializeField] private RouteDatabaseSO _routeDatabase;

        [Tooltip("Doğrudan TravelScene'den Play'e basıldığında veya ActiveTravel boş olduğunda kullanılacak varsayılan rota")]
        [SerializeField] private TravelRouteSO _activeRoute;

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

        // Olaylar
        public event Action<float, int> OnTravelProgressUpdated; // progress (0..1), routeDay
        public event Action<LocationNodeSO> OnDestinationReached;
        public event Action<TravelRouteSO> OnAmbushTriggered;
        public event Action<string, string> OnRandomEventTriggered;

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
                OnRandomEventTriggered?.Invoke("Açlık Tehlikesi!", "Kervanın erzakları tükendi. Muhafızlar yorgun ve hoşnutsuz.");
            }

            CheckRandomEncounters();
        }

        private void CheckRandomEncounters()
        {
            if (_activeRoute == null)
            {
                return;
            }

            float roll = UnityEngine.Random.value;

            if (roll <= _activeRoute.DangerLevel * AmbushChanceFactor)
            {
                _isTraveling = false;
                Debug.Log("[TravelManager] PUSU! Haydutlar yolu kesti.");
                OnAmbushTriggered?.Invoke(_activeRoute);
                return;
            }

            if (roll > IncidentThreshold)
            {
                TriggerRoadIncident();
            }
        }

        private void TriggerRoadIncident()
        {
            int eventType = UnityEngine.Random.Range(0, 3);
            switch (eventType)
            {
                case 0:
                    Caravan.ModifyAkce(-BrokenAxleCost);
                    OnRandomEventTriggered?.Invoke("Yük Bağları Çözüldü", $"Engebeli yolda yük semeri hasar gördü. Yerel ustaya {BrokenAxleCost} Akçe ödendi.");
                    break;
                case 1:
                    OnRandomEventTriggered?.Invoke("Gezgin Derviş", "Yol kenarında karşılaşılan bir Horasan dervişi kervana hayır duası etti. Muhafızların morali yükseldi.");
                    break;
                case 2:
                    Caravan.AddFood(FoundFoodAmount);
                    OnRandomEventTriggered?.Invoke("Yol Azığı Bulundu", $"Konaklama yerinde {FoundFoodAmount} günlük taze erzak temin edildi.");
                    break;
            }
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
