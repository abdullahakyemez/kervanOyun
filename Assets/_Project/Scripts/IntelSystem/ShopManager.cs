using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;
using Kervan.Core.Save;
using Kervan.TradeAndTravel;

namespace Kervan.IntelSystem
{
    /// <summary>
    /// Bir şehirdeki han dükkânı satın alındığında yayınlanır.
    /// </summary>
    public readonly struct ShopPurchasedEvent : IGameEvent
    {
        public readonly string NodeId;
        public readonly string CityName;
        public readonly int CostAkce;

        public ShopPurchasedEvent(string nodeId, string cityName, int costAkce)
        {
            NodeId = nodeId;
            CityName = cityName;
            CostAkce = costAkce;
        }
    }

    /// <summary>
    /// Bir han dükkânının kasasında biriken kâr tahsil edildiğinde yayınlanır.
    /// </summary>
    public readonly struct ShopProfitCollectedEvent : IGameEvent
    {
        public readonly string NodeId;
        public readonly string CityName;
        public readonly int CollectedAmount;

        public ShopProfitCollectedEvent(string nodeId, string cityName, int amount)
        {
            NodeId = nodeId;
            CityName = cityName;
            CollectedAmount = amount;
        }
    }

    [Serializable]
    public class HanShop
    {
        public string nodeId;
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
    /// Güvenli şehirlerde sabit han dükkânı / pazar tezgâhı açma ve gün geçtikçe pasif akçe geliri toplama sistemi (saf C# servisi).
    /// </summary>
    public class ShopManager
    {
        /// <summary>
        /// Servis bulucu (<see cref="Services"/>) üzerinden aktif <see cref="ShopManager"/> örneğine erişim sağlar.
        /// </summary>
        public static ShopManager Instance =>
            Services.TryGet<ShopManager>(out var manager) ? manager : null;

        private CaravanData _caravanData;
        private readonly List<HanShop> _shops = new List<HanShop>();

        public IReadOnlyList<HanShop> Shops => _shops;

        public ShopManager()
        {
            ResetToDefaults();
        }

        /// <summary>
        /// Şehir dükkânlarını yeni oyun başlangıç değerlerine sıfırlar.
        /// </summary>
        public void ResetToDefaults()
        {
            _caravanData = null;
            _shops.Clear();
            _shops.Add(new HanShop
            {
                nodeId = "node_sogut",
                cityName = "Söğüt",
                faction = FactionType.Ottoman,
                purchaseCost = 200,
                minReputationRequired = 20,
                dailyBaseIncome = 25,
                dailyApprenticeWage = 5,
                isOwned = false,
                storedGoldInTill = 0
            });
            _shops.Add(new HanShop
            {
                nodeId = "node_bilecik",
                cityName = "Bilecik",
                faction = FactionType.Ottoman,
                purchaseCost = 350,
                minReputationRequired = 35,
                dailyBaseIncome = 35,
                dailyApprenticeWage = 5,
                isOwned = false,
                storedGoldInTill = 0
            });
            _shops.Add(new HanShop
            {
                nodeId = "node_bursa",
                cityName = "Bursa",
                faction = FactionType.Ottoman,
                purchaseCost = 600,
                minReputationRequired = 50,
                dailyBaseIncome = 55,
                dailyApprenticeWage = 10,
                isOwned = false,
                storedGoldInTill = 0
            });
        }

        private CaravanData ResolveCaravanData()
        {
            if (Services.TryGet<GameSession>(out var session) && session.Caravan != null)
            {
                _caravanData = session.Caravan;
                return _caravanData;
            }

            if (_caravanData == null && CaravanManager.Instance != null)
            {
                _caravanData = CaravanManager.Instance.Caravan;
            }

            return _caravanData;
        }

        public void SetCaravanData(CaravanData caravanData)
        {
            _caravanData = caravanData;
        }

        /// <summary>
        /// ASCII düğüm kimliği (<c>node_sogut</c>) veya görünen şehir adı ile ilgili han dükkânını bulur.
        /// </summary>
        public HanShop GetShop(string nodeIdOrCityName)
        {
            if (string.IsNullOrWhiteSpace(nodeIdOrCityName)) return null;

            for (int i = 0; i < _shops.Count; i++)
            {
                var shop = _shops[i];
                if (shop == null) continue;

                if (!string.IsNullOrWhiteSpace(shop.nodeId) &&
                    string.Equals(shop.nodeId, nodeIdOrCityName, StringComparison.Ordinal))
                {
                    return shop;
                }

                if (!string.IsNullOrWhiteSpace(shop.cityName) &&
                    string.Equals(shop.cityName, nodeIdOrCityName, StringComparison.Ordinal))
                {
                    return shop;
                }
            }

            return null;
        }

        /// <summary>
        /// Şehirde yeni bir han dükkânı / tezgâh satın alır.
        /// </summary>
        public bool TryPurchaseShop(string nodeIdOrCityName, out string message)
        {
            var caravan = ResolveCaravanData();

            var shop = GetShop(nodeIdOrCityName);
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

            if (Services.TryGet<ReputationManager>(out var repManager))
            {
                int currentRep = repManager.GetReputation(shop.faction);
                if (currentRep < shop.minReputationRequired)
                {
                    string factionLabel = repManager.GetFactionDisplayName(shop.faction);
                    message = $"Bu şehirde dükkân açmak için {factionLabel} nezdinde en az {shop.minReputationRequired} itibar gerekir (Mevcut: {currentRep}).";
                    return false;
                }
            }

            if (caravan != null && !caravan.ModifyAkce(-shop.purchaseCost))
            {
                message = $"Yeterli akçeniz yok! Gerekli bedel: {shop.purchaseCost} Akçe.";
                return false;
            }

            shop.isOwned = true;
            message = $"{shop.cityName} hanında dükkân mülkiyeti alındı! Kalfa tezgâh başına geçti.";
            EventBus.Publish(new ShopPurchasedEvent(shop.nodeId, shop.cityName, shop.purchaseCost));
            Debug.Log($"[ShopManager] {message}");
            return true;
        }

        /// <summary>
        /// Her gün geçişinde (<see cref="GameSession.AdvanceDays"/>) sahip olunan dükkânların kasasına net kâr ekler.
        /// </summary>
        public void SimulateDailyProfits(int daysPassed)
        {
            if (daysPassed <= 0) return;

            for (int i = 0; i < _shops.Count; i++)
            {
                var shop = _shops[i];
                if (shop == null || !shop.isOwned) continue;

                int profit = shop.NetDailyProfit * daysPassed;
                shop.storedGoldInTill += profit;
                Debug.Log($"[ShopManager] {shop.cityName} ({shop.nodeId}) dükkânı {daysPassed} günde {profit} Akçe kâr üretti. Kasada biriken: {shop.storedGoldInTill} Akçe.");
            }
        }

        /// <summary>
        /// Şehirdeki dükkân kasasında birikmiş kârı kervan hazinesine aktarır.
        /// </summary>
        public int CollectShopProfit(string nodeIdOrCityName)
        {
            var caravan = ResolveCaravanData();

            var shop = GetShop(nodeIdOrCityName);
            if (shop == null || !shop.isOwned || shop.storedGoldInTill <= 0) return 0;

            int collected = shop.storedGoldInTill;
            shop.storedGoldInTill = 0;

            if (caravan != null)
            {
                caravan.ModifyAkce(collected);
            }

            EventBus.Publish(new ShopProfitCollectedEvent(shop.nodeId, shop.cityName, collected));
            Debug.Log($"[ShopManager] {shop.cityName} ({shop.nodeId}) kasasından {collected} Akçe kervan hazinesine aktarıldı.");
            return collected;
        }

        /// <summary>
        /// Sahip olunan veya kasasında para bulunan dükkânları JSON kaydı için serileştirir.
        /// </summary>
        public List<ShopSaveData> ToSaveData()
        {
            var list = new List<ShopSaveData>(_shops.Count);
            for (int i = 0; i < _shops.Count; i++)
            {
                var shop = _shops[i];
                if (shop == null || string.IsNullOrWhiteSpace(shop.nodeId)) continue;

                list.Add(new ShopSaveData
                {
                    nodeId = shop.nodeId,
                    isOwned = shop.isOwned,
                    storedGoldInTill = Mathf.Max(0, shop.storedGoldInTill)
                });
            }
            return list;
        }

        /// <summary>
        /// Diskten okunan dükkân mülkiyet ve kasa kayıtlarını çalışma anı listesine uygular.
        /// </summary>
        public void ApplySaveData(List<ShopSaveData> savedShops)
        {
            ResetToDefaults();
            if (savedShops == null || savedShops.Count == 0) return;

            for (int i = 0; i < savedShops.Count; i++)
            {
                var entry = savedShops[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.nodeId)) continue;

                var shop = GetShop(entry.nodeId);
                if (shop != null)
                {
                    shop.isOwned = entry.isOwned;
                    shop.storedGoldInTill = Mathf.Max(0, entry.storedGoldInTill);
                }
            }
        }
    }
}
