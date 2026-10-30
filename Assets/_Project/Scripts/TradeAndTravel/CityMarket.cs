using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Bir emtianın şehir pazarındaki arz-talep kademesini belirtir.
    /// </summary>
    public enum MarketSupplyTier
    {
        LocalAbundant,  // Yerel üretim (Bol ve ucuz — alış için ideal)
        Standard,       // Standart piyasa
        HighDemand      // Yüksek talep / kıtlık (Pahalı — satış için ideal)
    }

    [Serializable]
    public class LiveMarketItem
    {
        public ItemDataSO Item;
        public int CurrentStock;
        public int InitialStock;
        public float BaseMultiplier;
        public float Saturation; // 0.0 = Doygunluk yok, 1.0 = Aşırı doymuş (fiyat dipte)
        public MarketSupplyTier SupplyTier;

        public LiveMarketItem(
            ItemDataSO item,
            int initialStock,
            float baseMultiplier,
            MarketSupplyTier supplyTier = MarketSupplyTier.Standard)
        {
            Item = item;
            CurrentStock = Mathf.Max(0, initialStock);
            InitialStock = Mathf.Max(1, initialStock);
            BaseMultiplier = baseMultiplier;
            Saturation = 0f;
            SupplyTier = supplyTier;
        }
    }

    /// <summary>
    /// Şehir pazarının dinamik arz-talep, doygunluk ve fiyatlandırma motorunu çalıştıran sınıf.
    /// <see cref="GameSession.CurrentNodeId"/> üzerinden aktif şehrin pazar verisini otomatik çözümler.
    /// </summary>
    public class CityMarket : MonoBehaviour
    {
        [Header("Veri Referansları")]
        [Tooltip("Varsayılan veya yedek şehir pazarı verisi")]
        [SerializeField] private CityMarketDataSO _marketData;
        [Tooltip("Haritadaki düğümleri çözümlemek için kullanılan rota veri tabanı")]
        [SerializeField] private RouteDatabaseSO _routeDatabase;
        [Tooltip("Şehir bazlı tanımlı tüm CityMarketDataSO varlıkları")]
        [SerializeField] private List<CityMarketDataSO> _allCityMarkets = new List<CityMarketDataSO>();
        [Tooltip("Oyundaki tüm ticaret mallarının (10 emtia) tam kataloğu")]
        [SerializeField] private List<ItemDataSO> _allTradeItems = new List<ItemDataSO>();

        [Header("Canlı Stok & Fiyatlar")]
        [SerializeField] private List<LiveMarketItem> _marketItems = new List<LiveMarketItem>();

        private LocationNodeSO _activeCityNode;
        private string _activeNodeId = GameSession.DefaultStartNodeId;
        private float _activeTaxRate = 0.08f;

        public event Action OnMarketUpdated;

        public CityMarketDataSO Data => _marketData;
        public LocationNodeSO ActiveCityNode => _activeCityNode;
        public string ActiveNodeId => _activeNodeId;
        public float ActiveTaxRate => _activeTaxRate;
        public IReadOnlyList<LiveMarketItem> MarketItems => _marketItems;
        public IReadOnlyList<ItemDataSO> AllTradeItems => _allTradeItems;

        private void Awake()
        {
            InitializeForCurrentSessionNode();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DayPassedEvent>(OnDayPassed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DayPassedEvent>(OnDayPassed);
        }

        private void OnDayPassed(DayPassedEvent evt)
        {
            if (Services.TryGet<GameSession>(out _))
            {
                // GameSession.AdvanceDays zaten ApplyDailyMarketRecovery çalıştırdı;
                // açık olan şehir pazarının canlı listesini oturum durumuyla senkronize et.
                ApplySessionSavedMarketState(_activeNodeId);
                OnMarketUpdated?.Invoke();
            }
            else
            {
                DailyMarketRecovery(evt.DaysAdded);
            }
        }

        /// <summary>
        /// <see cref="GameSession.CurrentNodeId"/> değerine göre aktif şehrin pazarını kurar,
        /// varsa diskten yüklenen kervan envanter kayıtlarını gerçek <see cref="ItemDataSO"/> referanslarıyla eşleştirir
        /// ve şehrin kayıtlı stok/doygunluk durumunu uygular.
        /// </summary>
        public void InitializeForCurrentSessionNode()
        {
            string currentNodeId = GameSession.DefaultStartNodeId;
            if (Services.TryGet<GameSession>(out var session))
            {
                session.SyncFromLegacyPrefsIfChanged();
                if (!string.IsNullOrWhiteSpace(session.CurrentNodeId))
                {
                    currentNodeId = session.CurrentNodeId;
                }

                session.ResolveSavedInventoryItems(BuildEffectiveItemCatalog());
            }

            _activeNodeId = currentNodeId;

            if (_routeDatabase != null)
            {
                _activeCityNode = _routeDatabase.FindNodeById(currentNodeId);
            }

            CityMarketDataSO matchedMarket = FindMarketDataForNode(currentNodeId);
            if (matchedMarket != null)
            {
                if (_activeCityNode == null) _activeCityNode = matchedMarket.CityNode;
                InitializeFromData(matchedMarket);
                AppendMissingCatalogItems(currentNodeId);
                ApplySessionSavedMarketState(currentNodeId);
                OnMarketUpdated?.Invoke();
                return;
            }

            if (_marketData != null &&
                _marketData.CityNode != null &&
                string.Equals(_marketData.CityNode.NodeId, currentNodeId, StringComparison.Ordinal))
            {
                if (_activeCityNode == null) _activeCityNode = _marketData.CityNode;
                InitializeFromData(_marketData);
                AppendMissingCatalogItems(currentNodeId);
                ApplySessionSavedMarketState(currentNodeId);
                OnMarketUpdated?.Invoke();
                return;
            }

            // Özel CityMarketDataSO henüz oluşturulmamış şehirler için katalog üzerinden deterministik pazar kur
            BuildDeterministicMarketForNode(currentNodeId, _activeCityNode);
            ApplySessionSavedMarketState(currentNodeId);
            OnMarketUpdated?.Invoke();
        }

        public void InitializeFromData(CityMarketDataSO data)
        {
            _marketData = data;
            _marketItems.Clear();

            if (_marketData == null) return;

            _activeTaxRate = _marketData.BaseTaxRate;
            if (_marketData.CityNode != null)
            {
                _activeCityNode = _marketData.CityNode;
            }

            // Yerel mallar (Bol üretim, ucuz)
            foreach (var entry in _marketData.LocalGoods)
            {
                if (entry != null && entry.Item != null && !ContainsItem(entry.Item.ItemId))
                {
                    _marketItems.Add(new LiveMarketItem(
                        entry.Item,
                        entry.BaseStock,
                        entry.PriceMultiplier,
                        MarketSupplyTier.LocalAbundant));
                }
            }

            // Talep edilen mallar (Kıtlık, pahalı)
            foreach (var entry in _marketData.DemandedGoods)
            {
                if (entry != null && entry.Item != null && !ContainsItem(entry.Item.ItemId))
                {
                    _marketItems.Add(new LiveMarketItem(
                        entry.Item,
                        entry.BaseStock,
                        entry.PriceMultiplier,
                        MarketSupplyTier.HighDemand));
                }
            }

            // Standart mallar
            foreach (var entry in _marketData.StandardGoods)
            {
                if (entry != null && entry.Item != null && !ContainsItem(entry.Item.ItemId))
                {
                    _marketItems.Add(new LiveMarketItem(
                        entry.Item,
                        entry.BaseStock,
                        entry.PriceMultiplier,
                        ClassifyMultiplier(entry.PriceMultiplier)));
                }
            }

            OnMarketUpdated?.Invoke();
        }

        private CityMarketDataSO FindMarketDataForNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || _allCityMarkets == null) return null;

            for (int i = 0; i < _allCityMarkets.Count; i++)
            {
                var marketSo = _allCityMarkets[i];
                if (marketSo != null && marketSo.CityNode != null &&
                    string.Equals(marketSo.CityNode.NodeId, nodeId, StringComparison.Ordinal))
                {
                    return marketSo;
                }
            }

            return null;
        }

        private List<ItemDataSO> BuildEffectiveItemCatalog()
        {
            var result = new List<ItemDataSO>();
            var seenIds = new HashSet<string>(StringComparer.Ordinal);

            void TryAdd(ItemDataSO item)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ItemId) && seenIds.Add(item.ItemId))
                {
                    result.Add(item);
                }
            }

            if (_allTradeItems != null)
            {
                for (int i = 0; i < _allTradeItems.Count; i++) TryAdd(_allTradeItems[i]);
            }

            void TryAddFromMarket(CityMarketDataSO m)
            {
                if (m == null) return;
                foreach (var e in m.LocalGoods) if (e != null) TryAdd(e.Item);
                foreach (var e in m.DemandedGoods) if (e != null) TryAdd(e.Item);
                foreach (var e in m.StandardGoods) if (e != null) TryAdd(e.Item);
            }

            TryAddFromMarket(_marketData);
            if (_allCityMarkets != null)
            {
                for (int i = 0; i < _allCityMarkets.Count; i++) TryAddFromMarket(_allCityMarkets[i]);
            }

            return result;
        }

        private void AppendMissingCatalogItems(string nodeId)
        {
            var catalog = BuildEffectiveItemCatalog();
            for (int i = 0; i < catalog.Count; i++)
            {
                var item = catalog[i];
                if (item == null || ContainsItem(item.ItemId)) continue;

                ComputeDeterministicEntry(nodeId, item, i, out int stock, out float mult, out MarketSupplyTier tier);
                _marketItems.Add(new LiveMarketItem(item, stock, mult, tier));
            }
        }

        private void BuildDeterministicMarketForNode(string nodeId, LocationNodeSO cityNode)
        {
            _marketItems.Clear();

            if (cityNode != null)
            {
                switch (cityNode.RulingFaction)
                {
                    case FactionType.Ottoman:
                        _activeTaxRate = 0.06f;
                        break;
                    case FactionType.Byzantine:
                        _activeTaxRate = 0.12f;
                        break;
                    default:
                        _activeTaxRate = 0.08f;
                        break;
                }
            }
            else if (_marketData != null)
            {
                _activeTaxRate = _marketData.BaseTaxRate;
            }
            else
            {
                _activeTaxRate = 0.08f;
            }

            var catalog = BuildEffectiveItemCatalog();
            for (int i = 0; i < catalog.Count; i++)
            {
                var item = catalog[i];
                if (item == null) continue;

                ComputeDeterministicEntry(nodeId, item, i, out int stock, out float mult, out MarketSupplyTier tier);
                _marketItems.Add(new LiveMarketItem(item, stock, mult, tier));
            }

            OnMarketUpdated?.Invoke();
        }

        private static void ComputeDeterministicEntry(
            string nodeId,
            ItemDataSO item,
            int index,
            out int stock,
            out float multiplier,
            out MarketSupplyTier tier)
        {
            int hash = GetStableHash((nodeId ?? GameSession.DefaultStartNodeId) + "_" + item.ItemId) + index * 17;
            int bucket = Mathf.Abs(hash) % 10;

            if (bucket < 3)
            {
                // Yerel üretim (Ucuz & bol stok)
                tier = MarketSupplyTier.LocalAbundant;
                multiplier = 0.72f + ((Mathf.Abs(hash / 10) % 10) * 0.01f); // 0.72 - 0.81
                stock = 45 + (Mathf.Abs(hash) % 35);
            }
            else if (bucket >= 7)
            {
                // Yüksek talep (Pahalı & sınırlı stok)
                tier = MarketSupplyTier.HighDemand;
                multiplier = 1.32f + ((Mathf.Abs(hash / 10) % 25) * 0.01f); // 1.32 - 1.56
                stock = 15 + (Mathf.Abs(hash) % 20);
            }
            else
            {
                // Standart piyasa
                tier = MarketSupplyTier.Standard;
                multiplier = 0.95f + ((Mathf.Abs(hash / 10) % 18) * 0.01f); // 0.95 - 1.12
                stock = 25 + (Mathf.Abs(hash) % 25);
            }
        }

        private static int GetStableHash(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < text.Length; i++)
                {
                    hash = (hash * 31) + text[i];
                }
                return hash;
            }
        }

        private static MarketSupplyTier ClassifyMultiplier(float mult)
        {
            if (mult <= 0.85f) return MarketSupplyTier.LocalAbundant;
            if (mult >= 1.25f) return MarketSupplyTier.HighDemand;
            return MarketSupplyTier.Standard;
        }

        private bool ContainsItem(string itemId)
        {
            return FindLiveItem(itemId) != null;
        }

        public LiveMarketItem FindLiveItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;

            for (int i = 0; i < _marketItems.Count; i++)
            {
                var item = _marketItems[i];
                if (item != null && item.Item != null &&
                    string.Equals(item.Item.ItemId, itemId, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Oyuncunun pazardan satın alırken ödeyeceği birim fiyat (Vergi, doygunluk indirimi ve stok kıtlığı primi dahil).
        /// </summary>
        public int GetBuyPrice(string itemId)
        {
            var liveItem = FindLiveItem(itemId);
            if (liveItem == null) return 0;

            float taxRate = _activeTaxRate;
            float saturationDiscount = Mathf.Clamp01(liveItem.Saturation * 0.40f);
            float scarcityBonus = CalculateScarcityBonus(liveItem);
            float effectiveMultiplier = liveItem.BaseMultiplier * (1.0f - saturationDiscount + scarcityBonus);

            float priceBeforeTax = liveItem.Item.BasePrice * Mathf.Max(0.25f, effectiveMultiplier);
            int finalPrice = Mathf.RoundToInt(priceBeforeTax * (1.0f + taxRate));

            return Mathf.Max(1, finalPrice);
        }

        /// <summary>
        /// Şehrin oyuncudan malı satın alırken ödeyeceği birim fiyat (Vergi kesintisi, doygunluk indirimi ve stok kıtlığı primi dahil).
        /// </summary>
        public int GetSellPrice(string itemId)
        {
            var liveItem = FindLiveItem(itemId);
            if (liveItem == null) return 0;

            float taxRate = _activeTaxRate;
            float saturationDiscount = Mathf.Clamp01(liveItem.Saturation * 0.50f);
            float scarcityBonus = CalculateScarcityBonus(liveItem);
            float effectiveMultiplier = liveItem.BaseMultiplier * (1.0f - saturationDiscount + scarcityBonus);

            float rawPrice = liveItem.Item.BasePrice * Mathf.Max(0.20f, effectiveMultiplier);
            int finalPrice = Mathf.RoundToInt(rawPrice * (1.0f - taxRate));

            return Mathf.Max(1, finalPrice);
        }

        /// <summary>
        /// Şehirdeki stok başlangıç dengesinin altına indiğinde oluşan kıtlık primini (0.00 – 0.20) hesaplar.
        /// </summary>
        public static float CalculateScarcityBonus(LiveMarketItem liveItem)
        {
            if (liveItem == null || liveItem.InitialStock <= 0 || liveItem.CurrentStock >= liveItem.InitialStock)
            {
                return 0f;
            }

            float missingRatio = Mathf.Clamp01((float)(liveItem.InitialStock - liveItem.CurrentStock) / liveItem.InitialStock);
            return missingRatio * 0.20f;
        }

        public bool BuyItem(CaravanData caravan, string itemId, int quantity)
        {
            if (caravan == null || quantity <= 0) return false;

            var liveItem = FindLiveItem(itemId);
            if (liveItem == null || liveItem.CurrentStock < quantity) return false;

            int unitPrice = GetBuyPrice(itemId);
            int totalPrice = unitPrice * quantity;

            if (caravan.CurrentAkce < totalPrice) return false;

            float totalWeight = liveItem.Item.UnitWeightKg * quantity;
            if (!caravan.CanCarry(totalWeight)) return false;

            // Önce pazar stoğu ve doygunluğunu güncelle ve oturuma kaydet (otomatik kayıt tetiklenmeden önce)
            liveItem.CurrentStock = Mathf.Max(0, liveItem.CurrentStock - quantity);
            liveItem.Saturation = Mathf.Max(0f, liveItem.Saturation - (quantity * 0.025f));
            PersistItemStateToSession(liveItem);

            // Ardından kervan akçesini ve yükünü güncelle
            caravan.ModifyAkce(-totalPrice);
            caravan.AddGoods(liveItem.Item, quantity, unitPrice);

            EventBus.Publish(new MarketTransactionEvent(itemId, quantity, unitPrice, true));
            OnMarketUpdated?.Invoke();
            return true;
        }

        public bool SellItem(CaravanData caravan, string itemId, int quantity)
        {
            if (caravan == null || quantity <= 0) return false;

            var liveItem = FindLiveItem(itemId);
            if (liveItem == null) return false;

            int unitPrice = GetSellPrice(itemId);
            int totalEarnings = unitPrice * quantity;

            // Kervanda yeterli mal var mı kontrol et
            var slot = caravan.FindSlot(itemId);
            if (slot == null || slot.Quantity < quantity) return false;

            // GDD §4 kuralı: Doygunluk Çarpanı (Market Saturation)
            // Aynı şehre arka arkaya çok mal satılırsa şehir o mala doyar ve fiyat düşer.
            // Önce pazar durumunu oturuma kaydet ki CaravanStatsChangedEvent ile tetiklenen kayıt güncel veriyi yazsın.
            liveItem.CurrentStock += quantity;
            liveItem.Saturation = Mathf.Clamp01(liveItem.Saturation + (quantity * 0.03f));
            PersistItemStateToSession(liveItem);

            bool removed = caravan.RemoveGoods(itemId, quantity);
            if (!removed) return false;

            caravan.ModifyAkce(totalEarnings);

            EventBus.Publish(new MarketTransactionEvent(itemId, quantity, unitPrice, false));
            OnMarketUpdated?.Invoke();
            return true;
        }

        /// <summary>
        /// Şehirde zaman geçtikçe pazarın doygunluğunu temizler ve stokları başlangıç dengesine yaklaştırır.
        /// </summary>
        public void DailyMarketRecovery(int days = 1)
        {
            if (days <= 0) return;

            for (int i = 0; i < _marketItems.Count; i++)
            {
                var item = _marketItems[i];
                if (item == null) continue;

                for (int d = 0; d < days; d++)
                {
                    item.Saturation = Mathf.Max(0f, item.Saturation - GameSession.DailySaturationRecoveryRate);
                    if (item.InitialStock > 0 && item.CurrentStock != item.InitialStock)
                    {
                        int diff = Mathf.Abs(item.InitialStock - item.CurrentStock);
                        int step = Mathf.Max(GameSession.MinDailyStockRecoveryStep, Mathf.CeilToInt(diff * GameSession.DailyStockRecoveryRatio));
                        item.CurrentStock = Mathf.RoundToInt(Mathf.MoveTowards(item.CurrentStock, item.InitialStock, step));
                    }
                }

                PersistItemStateToSession(item);
            }

            OnMarketUpdated?.Invoke();
        }

        private void ApplySessionSavedMarketState(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || !Services.TryGet<GameSession>(out var session))
            {
                return;
            }

            for (int i = 0; i < _marketItems.Count; i++)
            {
                var liveItem = _marketItems[i];
                if (liveItem == null || liveItem.Item == null || string.IsNullOrWhiteSpace(liveItem.Item.ItemId))
                {
                    continue;
                }

                if (session.TryGetMarketItemState(nodeId, liveItem.Item.ItemId, out var savedState))
                {
                    if (savedState.initialStock > 0)
                    {
                        liveItem.InitialStock = savedState.initialStock;
                    }
                    liveItem.CurrentStock = Mathf.Max(0, savedState.currentStock);
                    liveItem.Saturation = Mathf.Clamp01(savedState.saturation);
                }
                else
                {
                    session.SetMarketItemState(
                        nodeId,
                        liveItem.Item.ItemId,
                        liveItem.CurrentStock,
                        liveItem.InitialStock,
                        liveItem.Saturation);
                }
            }
        }

        private void PersistItemStateToSession(LiveMarketItem liveItem)
        {
            if (liveItem == null || liveItem.Item == null || string.IsNullOrWhiteSpace(liveItem.Item.ItemId))
            {
                return;
            }

            if (Services.TryGet<GameSession>(out var session))
            {
                string nodeId = !string.IsNullOrWhiteSpace(_activeNodeId)
                    ? _activeNodeId
                    : session.CurrentNodeId;

                session.SetMarketItemState(
                    nodeId,
                    liveItem.Item.ItemId,
                    liveItem.CurrentStock,
                    liveItem.InitialStock,
                    liveItem.Saturation);
            }
        }
    }
}
