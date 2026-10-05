using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Kervanin seyahatini, yol sartlarini, erzak tuketimini ve
    /// pusu/olay zarlarini yoneten merkezi seyahat motoru.
    /// </summary>
    public class TravelManager : MonoBehaviour
    {
        [Header("Veri Referanslari")]
        [SerializeField] private CaravanData _caravanData = new CaravanData();
        [SerializeField] private LocationNodeSO _currentLocation;
        [SerializeField] private TravelRouteSO _activeRoute;

        [Header("Seyahat Durumu")]
        [SerializeField] private bool _isTraveling;
        [SerializeField] private float _daysProgress; // 0.0f - TotalDays
        [SerializeField] private float _dayDurationSeconds = 3.0f; // 1 oyun gunu kac gercek saniye surer
        [SerializeField] private int _globalDayCount = 1;

        private float _timeAccumulator;
        private int _lastDayRecorded;

        // Olaylar
        public event Action<float, int> OnTravelProgressUpdated; // progress (0..1), day
        public event Action<LocationNodeSO> OnDestinationReached;
        public event Action<TravelRouteSO> OnAmbushTriggered;
        public event Action<string, string> OnRandomEventTriggered;

        // Getters
        public CaravanData Caravan => _caravanData;
        public LocationNodeSO CurrentLocation => _currentLocation;
        public TravelRouteSO ActiveRoute => _activeRoute;
        public bool IsTraveling => _isTraveling;
        public int GlobalDayCount => _globalDayCount;

        private void Start()
        {
            _lastDayRecorded = _globalDayCount;
        }

        private void Update()
        {
            if (!_isTraveling || _activeRoute == null) return;

            _timeAccumulator += Time.deltaTime;
            float dayDelta = Time.deltaTime / _dayDurationSeconds;
            _daysProgress += dayDelta;

            int currentRouteDay = Mathf.FloorToInt(_daysProgress);
            if (currentRouteDay > _lastDayRecorded)
            {
                _lastDayRecorded = currentRouteDay;
                _globalDayCount++;
                ProcessDailyLogistics();
            }

            float totalDays = Mathf.Max(1, _activeRoute.DistanceDays);
            float normalizedProgress = Mathf.Clamp01(_daysProgress / totalDays);

            OnTravelProgressUpdated?.Invoke(normalizedProgress, _globalDayCount);

            if (_daysProgress >= totalDays)
            {
                ArriveAtDestination();
            }
        }

        public void StartTravel(TravelRouteSO route)
        {
            if (route == null || _isTraveling) return;

            _activeRoute = route;
            _isTraveling = true;
            _daysProgress = 0f;
            _lastDayRecorded = 0;
            _currentLocation = null;

            Debug.Log($"[Travel] Seyahat basladi: {route.FromNode?.NodeName} -> {route.ToNode?.NodeName} ({route.DistanceDays} Gun)");
        }

        public void PauseTravel()
        {
            _isTraveling = false;
        }

        public void ResumeTravel()
        {
            if (_activeRoute != null && _daysProgress < _activeRoute.DistanceDays)
            {
                _isTraveling = true;
            }
        }

        private void ProcessDailyLogistics()
        {
            if (_activeRoute == null) return;

            // 1. Erzak Tuketimi
            int foodToConsume = Mathf.RoundToInt(1 * _activeRoute.FoodConsumptionMultiplier);
            bool hasFood = _caravanData.ConsumeFood(foodToConsume);

            if (!hasFood)
            {
                Debug.LogWarning("[Travel] Erzak tukendi! Kervan muhafizlari ac ve moralleri dusuyor.");
                OnRandomEventTriggered?.Invoke("Açlık Tehlikesi!", "Kervanın erzakları bitti. Muhafızlar yorgun ve hoşnutsuz.");
            }

            // 2. Yol Tehlike ve Pusu Zari
            CheckRandomEncounters();
        }

        private void CheckRandomEncounters()
        {
            if (_activeRoute == null) return;

            float roll = UnityEngine.Random.value;

            // Pusu Zari
            if (roll <= _activeRoute.DangerLevel * 0.40f)
            {
                _isTraveling = false;
                Debug.Log("[Travel] PUSU! Haydutlar yolu kesti.");
                OnAmbushTriggered?.Invoke(_activeRoute);
                return;
            }

            // Diger Yol Hadiseleri
            if (roll > 0.85f)
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
                    // Bozuk yol aks kirilmasi
                    _caravanData.ModifyAkce(-25);
                    OnRandomEventTriggered?.Invoke("Araba Aksı Kırıldı", "Engebeli yolda arabanın aksı çatladı. Yerel köylülerden 25 Akçeye yedek parça temin edildi.");
                    break;
                case 1:
                    // Dervis / Dua
                    OnRandomEventTriggered?.Invoke("Gezgin Derviş", "Yol kenarında karşılaşılan bir derviş kervana hayır duası etti. Muhafızların morali yükseldi.");
                    break;
                case 2:
                    // Sahipsiz tahil cuvali
                    _caravanData.AddFood(3);
                    OnRandomEventTriggered?.Invoke("Yol Azığı Bulundu", "Terk edilmiş bir göçebe kampında 3 günlük bozulmamış kuru erzak bulundu.");
                    break;
            }
        }

        private void ArriveAtDestination()
        {
            _isTraveling = false;
            _currentLocation = _activeRoute.ToNode;
            var arrived = _currentLocation;
            _activeRoute = null;

            Debug.Log($"[Travel] Hedefe varildi: {arrived?.NodeName}");
            OnDestinationReached?.Invoke(arrived);
        }

        public bool RestAtCaravanserai(int days)
        {
            if (_currentLocation == null || !_currentLocation.HasInn) return false;

            int totalCost = _currentLocation.DailyRestCost * days;
            if (!_caravanData.ModifyAkce(-totalCost)) return false;

            _globalDayCount += days;
            _caravanData.HealAllGuards(_currentLocation.DailyHealAmount * days);
            _caravanData.ConsumeFood(days);

            Debug.Log($"[Travel] {days} gun han/kervansarayda dinlenildi. Masraf: {totalCost} Akce.");
            return true;
        }
    }
}
