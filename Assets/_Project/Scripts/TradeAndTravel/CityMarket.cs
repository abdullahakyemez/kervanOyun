using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    [Serializable]
    public class LiveMarketItem
    {
        public ItemDataSO Item;
        public int CurrentStock;
        public float BaseMultiplier;
        public float Saturation; // 0.0 = Doygunluk yok, 1.0 = Asiri doymus (fiyat dipte)

        public LiveMarketItem(ItemDataSO item, int initialStock, float baseMultiplier)
        {
            Item = item;
            CurrentStock = initialStock;
            BaseMultiplier = baseMultiplier;
            Saturation = 0f;
        }
    }

    /// <summary>
    /// GDD.md standardinda sehir pazarinin dinamik arz-talep, doygunluk ve
    /// fiyatlandirma motorunu calistiran sinif.
    /// </summary>
    public class CityMarket : MonoBehaviour
    {
        [Header("Veri Referansi")]
        [SerializeField] private CityMarketDataSO _marketData;

        [Header("Canli Stok & Fiyatlar")]
        [SerializeField] private List<LiveMarketItem> _marketItems = new List<LiveMarketItem>();

        public event Action OnMarketUpdated;

        public CityMarketDataSO Data => _marketData;
        public IReadOnlyList<LiveMarketItem> MarketItems => _marketItems;

        private void Awake()
        {
            if (_marketItems.Count == 0 && _marketData != null)
            {
                InitializeFromData(_marketData);
            }
        }

        public void InitializeFromData(CityMarketDataSO data)
        {
            _marketData = data;
            _marketItems.Clear();

            if (_marketData == null) return;

            // Yerel mallar (Bol uretim, ucuz)
            foreach (var entry in _marketData.LocalGoods)
            {
                if (entry.Item != null)
                    _marketItems.Add(new LiveMarketItem(entry.Item, entry.BaseStock, entry.PriceMultiplier));
            }

            // Talep edilen mallar (Kitlik, pahali)
            foreach (var entry in _marketData.DemandedGoods)
            {
                if (entry.Item != null && !_marketItems.Exists(m => m.Item.ItemId == entry.Item.ItemId))
                    _marketItems.Add(new LiveMarketItem(entry.Item, entry.BaseStock, entry.PriceMultiplier));
            }

            // Standart mallar
            foreach (var entry in _marketData.StandardGoods)
            {
                if (entry.Item != null && !_marketItems.Exists(m => m.Item.ItemId == entry.Item.ItemId))
                    _marketItems.Add(new LiveMarketItem(entry.Item, entry.BaseStock, entry.PriceMultiplier));
            }

            OnMarketUpdated?.Invoke();
        }

        /// <summary>
        /// Oyuncunun pazardan satin alirken odeyecegi birim fiyat (Vergi dahil).
        /// </summary>
        public int GetBuyPrice(string itemId)
        {
            var liveItem = _marketItems.Find(m => m.Item != null && m.Item.ItemId == itemId);
            if (liveItem == null) return 0;

            float taxRate = _marketData != null ? _marketData.BaseTaxRate : 0.10f;
            float saturationDiscount = Mathf.Clamp01(liveItem.Saturation * 0.40f);
            float effectiveMultiplier = liveItem.BaseMultiplier * (1.0f - saturationDiscount);

            float priceBeforeTax = liveItem.Item.BasePrice * effectiveMultiplier;
            int finalPrice = Mathf.RoundToInt(priceBeforeTax * (1.0f + taxRate));

            return Mathf.Max(1, finalPrice);
        }

        /// <summary>
        /// Sehrin oyuncudan mali satin alirken odeyecegi birim fiyat (Vergi kesintili).
        /// </summary>
        public int GetSellPrice(string itemId)
        {
            var liveItem = _marketItems.Find(m => m.Item != null && m.Item.ItemId == itemId);
            if (liveItem == null) return 0;

            float taxRate = _marketData != null ? _marketData.BaseTaxRate : 0.10f;
            float saturationDiscount = Mathf.Clamp01(liveItem.Saturation * 0.50f);
            float effectiveMultiplier = liveItem.BaseMultiplier * (1.0f - saturationDiscount);

            float rawPrice = liveItem.Item.BasePrice * effectiveMultiplier;
            int finalPrice = Mathf.RoundToInt(rawPrice * (1.0f - taxRate));

            return Mathf.Max(1, finalPrice);
        }

        public bool BuyItem(CaravanData caravan, string itemId, int quantity)
        {
            if (caravan == null || quantity <= 0) return false;

            var liveItem = _marketItems.Find(m => m.Item != null && m.Item.ItemId == itemId);
            if (liveItem == null || liveItem.CurrentStock < quantity) return false;

            int unitPrice = GetBuyPrice(itemId);
            int totalPrice = unitPrice * quantity;

            if (caravan.CurrentAkce < totalPrice) return false;

            float totalWeight = liveItem.Item.UnitWeightKg * quantity;
            if (!caravan.CanCarry(totalWeight)) return false;

            // Satin alma gerceklesti
            caravan.ModifyAkce(-totalPrice);
            caravan.AddGoods(liveItem.Item, quantity, unitPrice);
            liveItem.CurrentStock -= quantity;

            // Stok azaldikca fiyatin hafif artmasi (kitlasma)
            liveItem.Saturation = Mathf.Max(0f, liveItem.Saturation - (quantity * 0.01f));

            EventBus.Publish(new MarketTransactionEvent(itemId, quantity, unitPrice, true));
            OnMarketUpdated?.Invoke();
            return true;
        }

        public bool SellItem(CaravanData caravan, string itemId, int quantity)
        {
            if (caravan == null || quantity <= 0) return false;

            var liveItem = _marketItems.Find(m => m.Item != null && m.Item.ItemId == itemId);
            if (liveItem == null) return false;

            int unitPrice = GetSellPrice(itemId);
            int totalEarnings = unitPrice * quantity;

            bool removed = caravan.RemoveGoods(itemId, quantity);
            if (!removed) return false;

            caravan.ModifyAkce(totalEarnings);
            liveItem.CurrentStock += quantity;

            // GDD.md kurali: Doygunluk Carpani (Market Saturation)
            // Ayni sehirlere arka arkaya cok mal satilirsa sehir o mala doyar ve fiyat duser!
            liveItem.Saturation = Mathf.Clamp01(liveItem.Saturation + (quantity * 0.03f));

            EventBus.Publish(new MarketTransactionEvent(itemId, quantity, unitPrice, false));
            OnMarketUpdated?.Invoke();
            return true;
        }

        /// <summary>
        /// Sehirde zaman gectikce pazarin doygunlugunu temizler ve stoklari tazeler.
        /// </summary>
        public void DailyMarketRecovery()
        {
            foreach (var item in _marketItems)
            {
                // Doygunluk her gun %20 azalir
                item.Saturation = Mathf.Max(0f, item.Saturation - 0.20f);
            }
            OnMarketUpdated?.Invoke();
        }
    }
}
