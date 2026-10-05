using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;
using Kervan.TradeAndTravel;

namespace Kervan.IntelSystem
{
    public struct ShopProfitCollectedEvent : IGameEvent
    {
        public string CityName;
        public int CollectedAmount;

        public ShopProfitCollectedEvent(string cityName, int amount)
        {
            CityName = cityName;
            CollectedAmount = amount;
        }
    }

    [System.Serializable]
    public class HanShop
    {
        public string cityName;
        public FactionType faction;
        public bool isOwned;
        public int purchaseCost = 250;         // Dükkân mülk bedeli
        public int minReputationRequired = 25; // En az Dost seviyesi
        public int storedGoldInTill = 0;       // Kasada biriken kâr
        public int dailyBaseIncome = 25;       // Günlük temel pasif gelir (Akçe)
        public int dailyApprenticeWage = 5;    // Kalfa günlük yevmiyesi

        public List<CaravanItemSlot> storedGoods = new List<CaravanItemSlot>();

        public int NetDailyProfit => Mathf.Max(0, dailyBaseIncome - dailyApprenticeWage);
    }

    /// <summary>
    /// Güvenli şehirlerde sabit han dükkânı / pazar tezgâhı açma ve seyahat esnasında pasif akçe geliri toplama sistemi.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        private CaravanData _caravanData;

        [Header("Şehir Dükkânları")]
        [SerializeField] private List<HanShop> _shops = new List<HanShop>
        {
            new HanShop { cityName = "Söğüt", faction = FactionType.Ottoman, purchaseCost = 200, minReputationRequired = 20, isOwned = false },
            new HanShop { cityName = "Bilecik", faction = FactionType.Ottoman, purchaseCost = 350, minReputationRequired = 35, isOwned = false },
            new HanShop { cityName = "Bursa", faction = FactionType.Ottoman, purchaseCost = 600, minReputationRequired = 50, isOwned = false }
        };

        public IReadOnlyList<HanShop> Shops => _shops;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ResolveCaravanData();
            EventBus.Subscribe<TravelCompletedEvent>(OnTravelCompleted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TravelCompletedEvent>(OnTravelCompleted);
        }

        private void ResolveCaravanData()
        {
            if (_caravanData == null && CaravanManager.Instance != null)
            {
                _caravanData = CaravanManager.Instance.Caravan;
            }
        }

        public void SetCaravanData(CaravanData caravanData)
        {
            _caravanData = caravanData;
        }

        public HanShop GetShop(string cityName)
        {
            return _shops.Find(s => s.cityName.Equals(cityName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Şehirde yeni bir han dükkânı / tezgâh satın alır veya kiralar.
        /// </summary>
        public bool TryPurchaseShop(string cityName, out string message)
        {
            ResolveCaravanData();

            var shop = GetShop(cityName);
            if (shop == null)
            {
                message = "Bu şehirde satılık dükkân bulunmuyor.";
                return false;
            }

            if (shop.isOwned)
            {
                message = "Bu dükkâna zaten sahipsiniz!";
                return false;
            }

            if (ReputationManager.Instance != null)
            {
                int currentRep = ReputationManager.Instance.GetReputation(shop.faction);
                if (currentRep < shop.minReputationRequired)
                {
                    message = $"Bu şehirde dükkân açmak için {shop.faction} nezdinde en az {shop.minReputationRequired} itibar gerekir (Mevcut: {currentRep}).";
                    return false;
                }
            }

            if (_caravanData != null && !_caravanData.ModifyAkce(-shop.purchaseCost))
            {
                message = $"Yeterli akçeniz yok! Gerekli bedel: {shop.purchaseCost} Akçe.";
                return false;
            }

            shop.isOwned = true;
            message = $"{cityName} hanında dükkân mülkiyeti alındı! Kalfa tezgâh başına geçti.";
            Debug.Log($"[Kervan-Dükkân] {message}");
            return true;
        }

        /// <summary>
        /// Her seyahat veya gün geçişinde sahip olunan dükkânların kasasına kâr ekler.
        /// </summary>
        public void SimulateDailyProfits(int daysPassed)
        {
            foreach (var shop in _shops)
            {
                if (!shop.isOwned) continue;

                int profit = shop.NetDailyProfit * daysPassed;
                shop.storedGoldInTill += profit;
                Debug.Log($"[Kervan-Dükkân] {shop.cityName} dükkânı {daysPassed} günde {profit} Akçe kâr üretti. Kasada biriken: {shop.storedGoldInTill} Akçe.");
            }
        }

        /// <summary>
        /// Şehre varıldığında dükkân kasasındaki birikmiş kârı kervan hazinesine aktarır.
        /// </summary>
        public int CollectShopProfit(string cityName)
        {
            ResolveCaravanData();

            var shop = GetShop(cityName);
            if (shop == null || !shop.isOwned || shop.storedGoldInTill <= 0) return 0;

            int collected = shop.storedGoldInTill;
            shop.storedGoldInTill = 0;

            if (_caravanData != null)
            {
                _caravanData.ModifyAkce(collected);
            }

            EventBus.Publish(new ShopProfitCollectedEvent(cityName, collected));
            Debug.Log($"[Kervan-Dükkân] {cityName} kasasından {collected} Akçe kervan hazinesine aktarıldı.");
            return collected;
        }

        private void OnTravelCompleted(TravelCompletedEvent evt)
        {
            int days = evt.Route != null ? Mathf.Max(1, evt.Route.DistanceDays) : 1;
            SimulateDailyProfits(days);
        }
    }
}
