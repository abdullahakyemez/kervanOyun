using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    [Serializable]
    public class MarketGoodEntry
    {
        public ItemDataSO Item;
        [Tooltip("Yerel stok miktari")]
        public int BaseStock = 50;
        [Tooltip("Fiyat carpani (1.0 = standart, 0.6 = yerel uretim/ucuz, 1.8 = kitlik/pahali)")]
        [Range(0.4f, 3.0f)]
        public float PriceMultiplier = 1.0f;
    }

    /// <summary>
    /// Bir sehrin pazarindaki yerel uretim (bol/ucuz) ve talep edilen (kit/pahali)
    /// emtialari tanimlayan ScriptableObject veri modeli.
    /// </summary>
    [CreateAssetMenu(fileName = "Market_New", menuName = "Kervan/Trade/City Market Data", order = 2)]
    public class CityMarketDataSO : ScriptableObject
    {
        [Header("Bagli Sehir Dugumu")]
        [SerializeField] private LocationNodeSO _cityNode;

        [Header("Pazar Vergisi & Ticaret Sartlari")]
        [Range(0f, 0.30f)]
        [Tooltip("Pazar giris/islem vergisi (%5 - %20)")]
        [SerializeField] private float _baseTaxRate = 0.10f;

        [Header("Emtia Listeleri")]
        [Tooltip("Sehrin bolca urettigi, ucuz yerel mallar (Arbitraj icin alinacak yer)")]
        [SerializeField] private List<MarketGoodEntry> _localGoods = new List<MarketGoodEntry>();

        [Tooltip("Sehrin kitlik cektigi ve yuksek fiyat odedigi mallar (Arbitraj icin satilacak yer)")]
        [SerializeField] private List<MarketGoodEntry> _demandedGoods = new List<MarketGoodEntry>();

        [Tooltip("Pazarda standart fiyattan bulunan diger genel mallar")]
        [SerializeField] private List<MarketGoodEntry> _standardGoods = new List<MarketGoodEntry>();

        // Getters
        public LocationNodeSO CityNode => _cityNode;
        public float BaseTaxRate => _baseTaxRate;
        public IReadOnlyList<MarketGoodEntry> LocalGoods => _localGoods;
        public IReadOnlyList<MarketGoodEntry> DemandedGoods => _demandedGoods;
        public IReadOnlyList<MarketGoodEntry> StandardGoods => _standardGoods;

        public MarketGoodEntry FindEntry(string itemId)
        {
            var entry = _localGoods.Find(e => e.Item != null && e.Item.ItemId == itemId);
            if (entry != null) return entry;

            entry = _demandedGoods.Find(e => e.Item != null && e.Item.ItemId == itemId);
            if (entry != null) return entry;

            return _standardGoods.Find(e => e.Item != null && e.Item.ItemId == itemId);
        }
    }
}
