using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Core;
using Kervan.Core.Save;
using Kervan.Audio;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Şehir merkezindeki (<c>TownHubScene</c> -> <c>Panel_Market</c>) alış-satış arayüzünü yöneten görünüm sınıfı.
    /// Kervan ambarındaki malların ortalama alış maliyetini, bu şehirdeki satış fiyatına göre
    /// kâr/zarar yüzdesini (<c>+%Kâr ▲</c>) ve pazarın yerel üretim / talep durumunu canlı gösterir.
    /// </summary>
    public class MarketUI : MonoBehaviour
    {
        [Header("Yöneticiler")]
        [SerializeField] private CityMarket _cityMarket;
        [SerializeField] private CaravanData _caravanData = new CaravanData();

        [Header("Sol Sütun: Kervan Ambarı")]
        [SerializeField] private TextMeshProUGUI _caravanAkceText;
        [SerializeField] private Slider _weightSlider;
        [Tooltip("LeftColumn_Caravan -> Header_Caravan -> WeightText")]
        [SerializeField] private TextMeshProUGUI _weightText;
        [Tooltip("LeftColumn_Caravan -> CaravanScroll -> Viewport -> Content")]
        [SerializeField] private Transform _caravanListContainer;
        [SerializeField] private GameObject _caravanRowTemplate;

        [Header("Sağ Sütun: Şehir Pazarı")]
        [SerializeField] private TextMeshProUGUI _marketTitleText;
        [Tooltip("RightColumn_Market -> Header_Market -> TaxText (veya WeightText)")]
        [SerializeField] private TextMeshProUGUI _marketTaxText;
        [Tooltip("RightColumn_Market -> MarketScroll -> Viewport -> Content")]
        [SerializeField] private Transform _marketListContainer;
        [SerializeField] private GameObject _marketRowTemplate;

        [Header("Alt Tezgâh: Seçili Mal & Miktar Kontrolleri")]
        [Tooltip("BottomBar_Trade -> SelectedItemSlot -> SelectedItemIcon")]
        [SerializeField] private Image _selectedItemIconImage;
        [Tooltip("BottomBar_Trade -> SelectedItemText")]
        [SerializeField] private TextMeshProUGUI _selectedItemNameText;
        [Tooltip("BottomBar_Trade -> QuantityControls -> QuantitySummaryText")]
        [SerializeField] private TextMeshProUGUI _quantitySummaryText;
        [Tooltip("BottomBar_Trade -> QuantityControls -> SliderRow -> Btn_Minus")]
        [SerializeField] private Button _minusQuantityButton;
        [Tooltip("BottomBar_Trade -> QuantityControls -> SliderRow -> QuantitySlider")]
        [SerializeField] private Slider _quantitySlider;
        [Tooltip("BottomBar_Trade -> QuantityControls -> SliderRow -> Btn_Plus")]
        [SerializeField] private Button _plusQuantityButton;
        [Tooltip("BottomBar_Trade -> QuantityControls -> SliderRow -> Btn_Max")]
        [SerializeField] private Button _maxQuantityButton;
        [SerializeField] private TextMeshProUGUI _quantityValueText;
        [SerializeField] private TextMeshProUGUI _totalPriceText;
        [SerializeField] private TextMeshProUGUI _totalWeightChangeText;
        [Tooltip("BottomBar_Trade -> TradeButtons -> Btn_Buy")]
        [SerializeField] private Button _buyButton;
        [Tooltip("BottomBar_Trade -> TradeButtons -> Btn_Sell")]
        [SerializeField] private Button _sellButton;
        [SerializeField] private Button _leaveMarketButton;

        [Header("Satır Görsel Teması (Kurumsal Çerçeveler)")]
        [Tooltip("Satır arka plan görseli (UI_BottomMenuInfoBG.png)")]
        [SerializeField] private Sprite _rowBackgroundSprite;
        [Tooltip("Eşya ikonu rozet çerçevesi (UI_BG_Itemicon.png)")]
        [SerializeField] private Sprite _itemIconFrameSprite;
        [Tooltip("Satır metinlerinde kullanılacak yazı tipi (Boşsa mevcut metinlerden otomatik alınır)")]
        [SerializeField] private TMP_FontAsset _fontAsset;

        private GameSession _gameSession;
        private ItemDataSO _selectedItem;
        private int _selectedQuantity = 1;
        private bool _suppressSliderCallback;
        private readonly List<GameObject> _spawnedCaravanRows = new List<GameObject>();
        private readonly List<GameObject> _spawnedMarketRows = new List<GameObject>();

        public CaravanData Caravan => _caravanData;

        private void Awake()
        {
            ResolveFontFallback();
        }

        private void Start()
        {
            BindGameSession();

            if (_cityMarket == null)
            {
                _cityMarket = GetComponent<CityMarket>();
                if (_cityMarket == null) _cityMarket = FindAnyObjectByType<CityMarket>();
            }

            if (_cityMarket != null)
            {
                _cityMarket.InitializeForCurrentSessionNode();
                _cityMarket.OnMarketUpdated += RefreshAll;
            }

            if (_quantitySlider != null)
            {
                _quantitySlider.wholeNumbers = true;
                _quantitySlider.minValue = 1f;
                _quantitySlider.onValueChanged.AddListener(OnQuantityChanged);
            }

            if (_minusQuantityButton != null) _minusQuantityButton.onClick.AddListener(OnMinusQuantityClicked);
            if (_plusQuantityButton != null) _plusQuantityButton.onClick.AddListener(OnPlusQuantityClicked);
            if (_maxQuantityButton != null) _maxQuantityButton.onClick.AddListener(OnMaxQuantityClicked);
            if (_buyButton != null) _buyButton.onClick.AddListener(OnBuyClicked);
            if (_sellButton != null) _sellButton.onClick.AddListener(OnSellClicked);
            if (_leaveMarketButton != null) _leaveMarketButton.onClick.AddListener(OnLeaveMarketClicked);

            EventBus.Subscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
            EventBus.Subscribe<LocationChangedEvent>(OnLocationChanged);

            if (_caravanRowTemplate != null) _caravanRowTemplate.SetActive(false);
            if (_marketRowTemplate != null) _marketRowTemplate.SetActive(false);

            RefreshAll();

            // Başlangıçta ilk pazar malını otomatik seç ki alt bar boş kalmasın
            if (_selectedItem == null && _cityMarket != null && _cityMarket.MarketItems.Count > 0)
            {
                SelectItem(_cityMarket.MarketItems[0].Item);
            }
        }

        private void OnEnable()
        {
            if (_gameSession != null)
            {
                RefreshAll();
            }
        }

        private void OnDestroy()
        {
            if (_cityMarket != null)
            {
                _cityMarket.OnMarketUpdated -= RefreshAll;
            }

            if (_quantitySlider != null) _quantitySlider.onValueChanged.RemoveListener(OnQuantityChanged);
            if (_minusQuantityButton != null) _minusQuantityButton.onClick.RemoveListener(OnMinusQuantityClicked);
            if (_plusQuantityButton != null) _plusQuantityButton.onClick.RemoveListener(OnPlusQuantityClicked);
            if (_maxQuantityButton != null) _maxQuantityButton.onClick.RemoveListener(OnMaxQuantityClicked);
            if (_buyButton != null) _buyButton.onClick.RemoveListener(OnBuyClicked);
            if (_sellButton != null) _sellButton.onClick.RemoveListener(OnSellClicked);
            if (_leaveMarketButton != null) _leaveMarketButton.onClick.RemoveListener(OnLeaveMarketClicked);

            EventBus.Unsubscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
            EventBus.Unsubscribe<LocationChangedEvent>(OnLocationChanged);
        }

        private void BindGameSession()
        {
            if (!Services.TryGet(out _gameSession))
            {
                _gameSession = new GameSession();
                Services.Register(_gameSession);
            }

            _gameSession.SyncFromLegacyPrefsIfChanged();
            _caravanData = _gameSession.Caravan;
        }

        private void ResolveFontFallback()
        {
            if (_fontAsset != null) return;
            if (_weightText != null && _weightText.font != null) _fontAsset = _weightText.font;
            else if (_selectedItemNameText != null && _selectedItemNameText.font != null) _fontAsset = _selectedItemNameText.font;
            else if (_quantitySummaryText != null && _quantitySummaryText.font != null) _fontAsset = _quantitySummaryText.font;
        }

        private void OnCaravanStatsChanged(CaravanStatsChangedEvent evt)
        {
            UpdateHeaderIndicators();
            UpdateTradeCalculations();
        }

        private void OnLocationChanged(LocationChangedEvent evt)
        {
            if (_cityMarket != null)
            {
                _cityMarket.InitializeForCurrentSessionNode();
            }
            RefreshAll();
        }

        public void SelectItem(ItemDataSO item)
        {
            _selectedItem = item;
            _selectedQuantity = 1;

            UpdateSliderRangeForSelectedItem();
            SetQuantityInternal(1);
            UpdateSelectedItemInfo();
            UpdateTradeCalculations();
        }

        private void OnMinusQuantityClicked()
        {
            SetQuantityInternal(_selectedQuantity - 1);
            UpdateTradeCalculations();
        }

        private void OnPlusQuantityClicked()
        {
            int maxAllowed = CalculateMaxSliderLimit();
            SetQuantityInternal(Mathf.Min(maxAllowed, _selectedQuantity + 1));
            UpdateTradeCalculations();
        }

        private void OnMaxQuantityClicked()
        {
            if (_selectedItem == null) return;

            int maxBuy = CalculateMaxAffordableAndCarryableBuy(_selectedItem);
            int maxSell = CalculateMaxSellable(_selectedItem);
            int targetMax = maxBuy > 0 ? maxBuy : (maxSell > 0 ? maxSell : 1);

            UpdateSliderRangeForSelectedItem();
            SetQuantityInternal(targetMax);
            UpdateTradeCalculations();
        }

        private void OnQuantityChanged(float val)
        {
            if (_suppressSliderCallback) return;
            _selectedQuantity = Mathf.Max(1, Mathf.RoundToInt(val));
            UpdateTradeCalculations();
        }

        private void SetQuantityInternal(int quantity)
        {
            int maxLimit = CalculateMaxSliderLimit();
            _selectedQuantity = Mathf.Clamp(quantity, 1, Mathf.Max(1, maxLimit));

            if (_quantitySlider != null)
            {
                _suppressSliderCallback = true;
                _quantitySlider.minValue = 1f;
                _quantitySlider.maxValue = Mathf.Max(1, maxLimit);
                _quantitySlider.value = _selectedQuantity;
                _suppressSliderCallback = false;
            }
        }

        private void UpdateSliderRangeForSelectedItem()
        {
            if (_quantitySlider == null) return;
            int maxLimit = CalculateMaxSliderLimit();
            _suppressSliderCallback = true;
            _quantitySlider.minValue = 1f;
            _quantitySlider.maxValue = Mathf.Max(1, maxLimit);
            _quantitySlider.value = Mathf.Clamp(_selectedQuantity, 1, Mathf.Max(1, maxLimit));
            _suppressSliderCallback = false;
        }

        private int CalculateMaxSliderLimit()
        {
            if (_selectedItem == null) return 1;
            int maxBuy = CalculateMaxAffordableAndCarryableBuy(_selectedItem);
            int maxSell = CalculateMaxSellable(_selectedItem);
            var liveItem = _cityMarket != null ? _cityMarket.FindLiveItem(_selectedItem.ItemId) : null;
            int stockLimit = liveItem != null ? Mathf.Min(liveItem.CurrentStock, 50) : 10;

            return Mathf.Max(1, Mathf.Max(maxBuy, Mathf.Max(maxSell, stockLimit)));
        }

        private int CalculateMaxAffordableAndCarryableBuy(ItemDataSO item)
        {
            if (item == null || _cityMarket == null || _caravanData == null) return 0;

            var liveItem = _cityMarket.FindLiveItem(item.ItemId);
            if (liveItem == null || liveItem.CurrentStock <= 0) return 0;

            int unitBuyPrice = Mathf.Max(1, _cityMarket.GetBuyPrice(item.ItemId));
            int maxByAkce = _caravanData.CurrentAkce / unitBuyPrice;

            float remainingCapacityKg = Mathf.Max(0f, _caravanData.MaxWeightCapacity - _caravanData.CalculateCurrentWeight());
            int maxByWeight = item.UnitWeightKg > 0.001f
                ? Mathf.FloorToInt((remainingCapacityKg + 0.0001f) / item.UnitWeightKg)
                : liveItem.CurrentStock;

            return Mathf.Max(0, Mathf.Min(liveItem.CurrentStock, Mathf.Min(maxByAkce, maxByWeight)));
        }

        private int CalculateMaxSellable(ItemDataSO item)
        {
            if (item == null || _caravanData == null) return 0;
            var slot = _caravanData.FindSlot(item.ItemId);
            return slot != null ? Mathf.Max(0, slot.Quantity) : 0;
        }

        private void UpdateSelectedItemInfo()
        {
            if (_selectedItem == null)
            {
                if (_selectedItemIconImage != null)
                {
                    _selectedItemIconImage.sprite = null;
                    _selectedItemIconImage.enabled = false;
                }
                if (_selectedItemNameText != null)
                {
                    _selectedItemNameText.text = "İşlem yapmak için listeden bir ticaret malı seçiniz.";
                }
                return;
            }

            if (_selectedItemIconImage != null)
            {
                _selectedItemIconImage.sprite = _selectedItem.Icon;
                _selectedItemIconImage.enabled = _selectedItem.Icon != null;
            }

            if (_selectedItemNameText != null && _cityMarket != null)
            {
                int buyUnit = _cityMarket.GetBuyPrice(_selectedItem.ItemId);
                int sellUnit = _cityMarket.GetSellPrice(_selectedItem.ItemId);
                var slot = _caravanData != null ? _caravanData.FindSlot(_selectedItem.ItemId) : null;

                string profitBadge = BuildProfitComparisonText(slot, sellUnit);
                _selectedItemNameText.text =
                    $"<b>{_selectedItem.ItemName}</b> ({_selectedItem.UnitWeightKg:F1} Kg)\n" +
                    $"<size=85%>Birim Alış: <color=#FFD966>{buyUnit} Akçe</color>  |  Satış: <color=#FFD966>{sellUnit} Akçe</color>{profitBadge}</size>";
            }
        }

        private void UpdateTradeCalculations()
        {
            if (_selectedItem == null || _cityMarket == null || _caravanData == null)
            {
                if (_quantitySummaryText != null) _quantitySummaryText.text = "Miktar: 0 Adet  |  Toplam: 0 Akçe (0.0 Kg)";
                if (_quantityValueText != null) _quantityValueText.text = "0";
                if (_totalPriceText != null) _totalPriceText.text = "Toplam: 0 Akçe";
                if (_totalWeightChangeText != null) _totalWeightChangeText.text = "Ağırlık: 0 Kg";
                if (_buyButton != null) _buyButton.interactable = false;
                if (_sellButton != null) _sellButton.interactable = false;
                return;
            }

            int buyUnitPrice = _cityMarket.GetBuyPrice(_selectedItem.ItemId);
            int sellUnitPrice = _cityMarket.GetSellPrice(_selectedItem.ItemId);

            int totalBuyPrice = buyUnitPrice * _selectedQuantity;
            int totalSellPrice = sellUnitPrice * _selectedQuantity;
            float totalWeight = _selectedItem.UnitWeightKg * _selectedQuantity;

            if (_quantitySummaryText != null)
            {
                _quantitySummaryText.text =
                    $"Miktar: <b>{_selectedQuantity} Adet</b> ({totalWeight:F1} Kg)  |  " +
                    $"Alış: <b>{totalBuyPrice}</b> Akçe · Satış: <b>{totalSellPrice}</b> Akçe";
            }

            if (_quantityValueText != null) _quantityValueText.text = $"{_selectedQuantity} Adet";
            if (_totalPriceText != null) _totalPriceText.text = $"Alış: {totalBuyPrice} Akçe | Satış: {totalSellPrice} Akçe";
            if (_totalWeightChangeText != null) _totalWeightChangeText.text = $"Ağırlık: ±{totalWeight:F1} Kg";

            var liveItem = _cityMarket.FindLiveItem(_selectedItem.ItemId);
            bool canBuy = liveItem != null &&
                          liveItem.CurrentStock >= _selectedQuantity &&
                          _caravanData.CurrentAkce >= totalBuyPrice &&
                          _caravanData.CanCarry(totalWeight);

            var caravanSlot = _caravanData.FindSlot(_selectedItem.ItemId);
            bool canSell = caravanSlot != null && caravanSlot.Quantity >= _selectedQuantity;

            if (_buyButton != null) _buyButton.interactable = canBuy;
            if (_sellButton != null) _sellButton.interactable = canSell;
        }

        private void UpdateHeaderIndicators()
        {
            if (_caravanData == null) return;

            float currentWeight = _caravanData.CalculateCurrentWeight();
            float maxWeight = _caravanData.MaxWeightCapacity;

            if (_caravanAkceText != null)
            {
                _caravanAkceText.text = $"{_caravanData.CurrentAkce:N0} Akçe";
            }

            if (_weightSlider != null)
            {
                _weightSlider.maxValue = maxWeight;
                _weightSlider.value = currentWeight;
            }

            if (_weightText != null)
            {
                _weightText.text = $"Yük: {currentWeight:F1} / {maxWeight:F0} Kg";
            }

            if (_cityMarket != null)
            {
                int taxPercent = Mathf.RoundToInt(_cityMarket.ActiveTaxRate * 100f);
                if (_marketTaxText != null)
                {
                    _marketTaxText.text = $"Vergi: %{taxPercent}";
                }

                if (_marketTitleText != null)
                {
                    string cityName = _cityMarket.ActiveCityNode != null
                        ? _cityMarket.ActiveCityNode.NodeName
                        : "Şehir Pazarı";
                    _marketTitleText.text = $"{cityName.ToUpperInvariant()} PAZARI";
                }
            }
        }

        public void RefreshAll()
        {
            if (_gameSession == null) BindGameSession();

            UpdateHeaderIndicators();
            RebuildMarketRows();
            RebuildCaravanRows();
            UpdateSliderRangeForSelectedItem();
            UpdateSelectedItemInfo();
            UpdateTradeCalculations();
        }

        private void RebuildMarketRows()
        {
            if (_marketListContainer == null || _cityMarket == null) return;

            ClearSpawnedRows(_spawnedMarketRows);

            var items = _cityMarket.MarketItems;
            if (items == null || items.Count == 0)
            {
                CreateEmptyStateRow(_marketListContainer, _spawnedMarketRows, "Bu şehir pazarında şu an satışta emtia bulunmuyor.");
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var liveItem = items[i];
                if (liveItem == null || liveItem.Item == null) continue;

                int buyPrice = _cityMarket.GetBuyPrice(liveItem.Item.ItemId);
                int sellPrice = _cityMarket.GetSellPrice(liveItem.Item.ItemId);
                string tierBadge = FormatSupplyTierBadge(liveItem.SupplyTier);
                bool isSelected = _selectedItem != null &&
                                  string.Equals(_selectedItem.ItemId, liveItem.Item.ItemId, StringComparison.Ordinal);

                string titleLine = $"<b>{liveItem.Item.ItemName}</b>  {tierBadge}";
                string subLine = $"Stok: <b>{liveItem.CurrentStock} Adet</b>  ·  Birim Ağırlık: {liveItem.Item.UnitWeightKg:F1} Kg";
                string rightPriceLine =
                    $"<color=#FFD966>Alış: <b>{buyPrice}</b> Akçe</color>\n" +
                    $"<size=88%><color=#D8C3A5>Satış: {sellPrice} Akçe</color></size>";

                GameObject rowGo = CreateStyledTradeRow(
                    _marketListContainer,
                    liveItem.Item,
                    titleLine,
                    subLine,
                    rightPriceLine,
                    isSelected);

                _spawnedMarketRows.Add(rowGo);
            }
        }

        private void RebuildCaravanRows()
        {
            if (_caravanListContainer == null || _caravanData == null) return;

            ClearSpawnedRows(_spawnedCaravanRows);

            var inventory = _caravanData.Inventory;
            int validCount = 0;
            if (inventory != null)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    var slot = inventory[i];
                    if (slot == null || slot.Item == null || slot.Quantity <= 0) continue;
                    validCount++;

                    int citySellPrice = _cityMarket != null ? _cityMarket.GetSellPrice(slot.Item.ItemId) : slot.AveragePurchasePrice;
                    int avgBuy = Mathf.Max(1, slot.AveragePurchasePrice);
                    int diffPerUnit = citySellPrice - avgBuy;
                    int pct = Mathf.RoundToInt(((float)diffPerUnit / avgBuy) * 100f);

                    string rightBadge;
                    if (diffPerUnit > 0)
                    {
                        rightBadge = $"Satış: <b>{citySellPrice}</b> Akçe\n<color=#52D669><b>+%{pct} Kâr ▲</b> (+{diffPerUnit * slot.Quantity})</color>";
                    }
                    else if (diffPerUnit < 0)
                    {
                        rightBadge = $"Satış: <b>{citySellPrice}</b> Akçe\n<color=#E5534B><b>%{pct} Zarar ▼</b> ({diffPerUnit * slot.Quantity})</color>";
                    }
                    else
                    {
                        rightBadge = $"Satış: <b>{citySellPrice}</b> Akçe\n<color=#E6B800>%0 Başabaş ▬</color>";
                    }

                    bool isSelected = _selectedItem != null &&
                                      string.Equals(_selectedItem.ItemId, slot.Item.ItemId, StringComparison.Ordinal);

                    string titleLine = $"<b>{slot.Item.ItemName}</b>  <color=#E6B800>({slot.Quantity} Adet)</color>";
                    string subLine = $"Ort. Alış: <b>{avgBuy} Akçe</b>  ·  Toplam: {slot.TotalWeight:F1} Kg";

                    GameObject rowGo = CreateStyledTradeRow(
                        _caravanListContainer,
                        slot.Item,
                        titleLine,
                        subLine,
                        rightBadge,
                        isSelected);

                    _spawnedCaravanRows.Add(rowGo);
                }
            }

            if (validCount == 0)
            {
                CreateEmptyStateRow(
                    _caravanListContainer,
                    _spawnedCaravanRows,
                    "Kervan ambarında henüz ticaret emtiası bulunmuyor.\nSağdaki şehir pazarından mal satın alabilirsiniz.");
            }
        }

        private GameObject CreateStyledTradeRow(
            Transform parent,
            ItemDataSO item,
            string titleLine,
            string subLine,
            string rightLine,
            bool isSelected)
        {
            var rowObj = new GameObject("TradeRow_" + item.ItemId, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            rowObj.transform.SetParent(parent, false);

            var layoutElem = rowObj.GetComponent<LayoutElement>();
            layoutElem.minHeight = 92f;
            layoutElem.preferredHeight = 92f;
            layoutElem.flexibleWidth = 1f;

            var bgImg = rowObj.GetComponent<Image>();
            if (_rowBackgroundSprite != null)
            {
                bgImg.sprite = _rowBackgroundSprite;
                bgImg.type = Image.Type.Simple;
            }
            bgImg.color = isSelected
                ? new Color(1f, 0.92f, 0.72f, 1f)
                : new Color(0.92f, 0.88f, 0.84f, 0.96f);

            // Sol İkon Yuvası
            var iconFrameObj = new GameObject("IconSlot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconFrameObj.transform.SetParent(rowObj.transform, false);
            var iconFrameRt = iconFrameObj.GetComponent<RectTransform>();
            iconFrameRt.anchorMin = new Vector2(0f, 0.5f);
            iconFrameRt.anchorMax = new Vector2(0f, 0.5f);
            iconFrameRt.pivot = new Vector2(0f, 0.5f);
            iconFrameRt.anchoredPosition = new Vector2(16f, 0f);
            iconFrameRt.sizeDelta = new Vector2(68f, 68f);

            var iconFrameImg = iconFrameObj.GetComponent<Image>();
            if (_itemIconFrameSprite != null)
            {
                iconFrameImg.sprite = _itemIconFrameSprite;
            }
            else
            {
                iconFrameImg.color = new Color(0.18f, 0.11f, 0.09f, 0.9f);
            }

            if (item.Icon != null)
            {
                var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObj.transform.SetParent(iconFrameObj.transform, false);
                var iconRt = iconObj.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = Vector2.zero;
                iconRt.sizeDelta = new Vector2(52f, 52f);

                var iconImg = iconObj.GetComponent<Image>();
                iconImg.sprite = item.Icon;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }

            // Orta Başlık & Alt Bilgi Metni
            var infoObj = new GameObject("InfoText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            infoObj.transform.SetParent(rowObj.transform, false);
            var infoRt = infoObj.GetComponent<RectTransform>();
            infoRt.anchorMin = new Vector2(0f, 0f);
            infoRt.anchorMax = new Vector2(1f, 1f);
            infoRt.offsetMin = new Vector2(98f, 8f);
            infoRt.offsetMax = new Vector2(-220f, -8f);

            var infoTmp = infoObj.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) infoTmp.font = _fontAsset;
            infoTmp.text = $"{titleLine}\n<size=84%><color=#D8C3A5>{subLine}</color></size>";
            infoTmp.fontSize = 20f;
            infoTmp.color = new Color(0.96f, 0.90f, 0.83f, 1f);
            infoTmp.alignment = TextAlignmentOptions.MidlineLeft;
            infoTmp.raycastTarget = false;

            // Sağ Fiyat / Kâr Rozeti Metni
            var rightObj = new GameObject("RightPriceText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            rightObj.transform.SetParent(rowObj.transform, false);
            var rightRt = rightObj.GetComponent<RectTransform>();
            rightRt.anchorMin = new Vector2(1f, 0f);
            rightRt.anchorMax = new Vector2(1f, 1f);
            rightRt.pivot = new Vector2(1f, 0.5f);
            rightRt.anchoredPosition = new Vector2(-18f, 0f);
            rightRt.sizeDelta = new Vector2(200f, 0f);

            var rightTmp = rightObj.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) rightTmp.font = _fontAsset;
            rightTmp.text = rightLine;
            rightTmp.fontSize = 19f;
            rightTmp.color = new Color(0.96f, 0.90f, 0.83f, 1f);
            rightTmp.alignment = TextAlignmentOptions.MidlineRight;
            rightTmp.raycastTarget = false;

            var btn = rowObj.GetComponent<Button>();
            var capturedItem = item;
            btn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                SelectItem(capturedItem);
                RebuildMarketRows();
                RebuildCaravanRows();
            });

            return rowObj;
        }

        private void CreateEmptyStateRow(Transform parent, List<GameObject> trackingList, string message)
        {
            var emptyObj = new GameObject("EmptyStateRow", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            emptyObj.transform.SetParent(parent, false);

            var layoutElem = emptyObj.GetComponent<LayoutElement>();
            layoutElem.minHeight = 140f;
            layoutElem.preferredHeight = 140f;
            layoutElem.flexibleWidth = 1f;

            var tmp = emptyObj.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) tmp.font = _fontAsset;
            tmp.text = message;
            tmp.fontSize = 20f;
            tmp.color = new Color(0.85f, 0.78f, 0.68f, 0.85f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            trackingList.Add(emptyObj);
        }

        private static void ClearSpawnedRows(List<GameObject> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    Destroy(list[i]);
                }
            }
            list.Clear();
        }

        private static string FormatSupplyTierBadge(MarketSupplyTier tier)
        {
            switch (tier)
            {
                case MarketSupplyTier.LocalAbundant:
                    return "<size=82%><color=#52D669>[Yerel Üretim · Ucuz]</color></size>";
                case MarketSupplyTier.HighDemand:
                    return "<size=82%><color=#FF9F43>[Yüksek Talep · Pahalı]</color></size>";
                default:
                    return "<size=82%><color=#C8B8A6>[Standart]</color></size>";
            }
        }

        private static string BuildProfitComparisonText(CaravanItemSlot slot, int citySellPrice)
        {
            if (slot == null || slot.Quantity <= 0 || slot.AveragePurchasePrice <= 0)
            {
                return string.Empty;
            }

            int diff = citySellPrice - slot.AveragePurchasePrice;
            int pct = Mathf.RoundToInt(((float)diff / slot.AveragePurchasePrice) * 100f);
            if (diff > 0)
            {
                return $"  |  <color=#52D669><b>+%{pct} Kâr ▲</b> (Ort. Maliyet: {slot.AveragePurchasePrice})</color>";
            }
            if (diff < 0)
            {
                return $"  |  <color=#E5534B><b>%{pct} Zarar ▼</b> (Ort. Maliyet: {slot.AveragePurchasePrice})</color>";
            }
            return $"  |  <color=#E6B800>%0 Başabaş (Ort. Maliyet: {slot.AveragePurchasePrice})</color>";
        }

        private void OnBuyClicked()
        {
            if (_selectedItem == null || _cityMarket == null || _caravanData == null) return;

            bool success = _cityMarket.BuyItem(_caravanData, _selectedItem.ItemId, _selectedQuantity);
            if (success)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCoins();
                }

                SaveSessionState();
                RefreshAll();
            }
        }

        private void OnSellClicked()
        {
            if (_selectedItem == null || _cityMarket == null || _caravanData == null) return;

            bool success = _cityMarket.SellItem(_caravanData, _selectedItem.ItemId, _selectedQuantity);
            if (success)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayCoins();
                }

                SaveSessionState();
                RefreshAll();
            }
        }

        private void SaveSessionState()
        {
            if (_gameSession != null && Services.TryGet<SaveService>(out var saveService))
            {
                saveService.SaveSession(_gameSession);
            }
        }

        private void OnLeaveMarketClicked()
        {
            SceneLoader.Load(SceneIds.WorldMap);
        }
    }
}

