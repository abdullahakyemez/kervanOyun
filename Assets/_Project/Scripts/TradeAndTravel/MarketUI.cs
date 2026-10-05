using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// EKRAN.md standardinda cift panelli parsomen sehir pazari arayuzu (Market UI).
    /// Kervan envanteri ile sehir tezgahini, miktar kaydiricisini ve arbitraj kar gostergelerini yonetir.
    /// </summary>
    public class MarketUI : MonoBehaviour
    {
        [Header("Yoneticiler")]
        [SerializeField] private CityMarket _cityMarket;
        [SerializeField] private CaravanData _caravanData = new CaravanData();

        [Header("Sol Panel: Kervan")]
        [SerializeField] private TextMeshProUGUI _caravanAkceText;
        [SerializeField] private Slider _weightSlider;
        [SerializeField] private TextMeshProUGUI _weightText;
        [SerializeField] private Transform _caravanListContainer;

        [Header("Sag Panel: Sehir Pazari")]
        [SerializeField] private TextMeshProUGUI _marketTitleText;
        [SerializeField] private Transform _marketListContainer;

        [Header("Alt Panel: Islem & Miktar")]
        [SerializeField] private TextMeshProUGUI _selectedItemNameText;
        [SerializeField] private Slider _quantitySlider;
        [SerializeField] private TextMeshProUGUI _quantityValueText;
        [SerializeField] private TextMeshProUGUI _totalPriceText;
        [SerializeField] private TextMeshProUGUI _totalWeightChangeText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _sellButton;
        [SerializeField] private Button _leaveMarketButton;

        private ItemDataSO _selectedItem;
        private int _selectedQuantity = 1;

        public CaravanData Caravan => _caravanData;

        private void Start()
        {
            if (_cityMarket == null)
            {
                _cityMarket = FindAnyObjectByType<CityMarket>();
            }

            if (_cityMarket != null)
            {
                _cityMarket.OnMarketUpdated += RefreshAll;
            }

            if (_quantitySlider != null)
            {
                _quantitySlider.onValueChanged.AddListener(OnQuantityChanged);
            }

            if (_buyButton != null) _buyButton.onClick.AddListener(OnBuyClicked);
            if (_sellButton != null) _sellButton.onClick.AddListener(OnSellClicked);
            if (_leaveMarketButton != null) _leaveMarketButton.onClick.AddListener(OnLeaveMarketClicked);

            // Test icin baslangicta kervana biraz mal ve akce ver
            if (_caravanData.Inventory.Count == 0 && _cityMarket != null && _cityMarket.MarketItems.Count > 0)
            {
                _caravanData.ModifyAkce(400);
            }

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_cityMarket != null)
            {
                _cityMarket.OnMarketUpdated -= RefreshAll;
            }
        }

        public void SelectItem(ItemDataSO item)
        {
            _selectedItem = item;
            _selectedQuantity = 1;

            if (_selectedItemNameText != null)
            {
                _selectedItemNameText.text = item != null ? item.ItemName : "Bir mal seçiniz";
            }

            UpdateTradeCalculations();
        }

        private void OnQuantityChanged(float val)
        {
            _selectedQuantity = Mathf.Max(1, Mathf.RoundToInt(val));
            UpdateTradeCalculations();
        }

        private void UpdateTradeCalculations()
        {
            if (_selectedItem == null || _cityMarket == null)
            {
                if (_quantityValueText != null) _quantityValueText.text = "0";
                if (_totalPriceText != null) _totalPriceText.text = "Toplam: 0 Akçe";
                if (_totalWeightChangeText != null) _totalWeightChangeText.text = "Ağırlık: 0 Kg";
                if (_buyButton != null) _buyButton.interactable = false;
                if (_sellButton != null) _sellButton.interactable = false;
                return;
            }

            if (_quantityValueText != null)
            {
                _quantityValueText.text = $"{_selectedQuantity} Adet";
            }

            int buyUnitPrice = _cityMarket.GetBuyPrice(_selectedItem.ItemId);
            int sellUnitPrice = _cityMarket.GetSellPrice(_selectedItem.ItemId);

            int totalBuyPrice = buyUnitPrice * _selectedQuantity;
            int totalSellPrice = sellUnitPrice * _selectedQuantity;
            float totalWeight = _selectedItem.UnitWeightKg * _selectedQuantity;

            if (_totalPriceText != null)
            {
                _totalPriceText.text = $"Alış: {totalBuyPrice} Akçe | Satış: {totalSellPrice} Akçe";
            }

            if (_totalWeightChangeText != null)
            {
                _totalWeightChangeText.text = $"Ağırlık: ±{totalWeight:F1} Kg";
            }

            // Buton aktiflik kontrolleri
            var liveItem = FindLiveItem(_selectedItem.ItemId);
            bool canBuy = (liveItem != null && liveItem.CurrentStock >= _selectedQuantity &&
                           _caravanData.CurrentAkce >= totalBuyPrice && _caravanData.CanCarry(totalWeight));

            var caravanSlot = FindCaravanSlot(_selectedItem.ItemId);
            bool canSell = (caravanSlot != null && caravanSlot.Quantity >= _selectedQuantity);

            if (_buyButton != null) _buyButton.interactable = canBuy;
            if (_sellButton != null) _sellButton.interactable = canSell;
        }

        private void RefreshAll()
        {
            // 1. Kervan Bilgileri
            if (_caravanAkceText != null) _caravanAkceText.text = $"{_caravanData.CurrentAkce} Akçe";
            if (_weightSlider != null)
            {
                _weightSlider.maxValue = _caravanData.MaxWeightCapacity;
                _weightSlider.value = _caravanData.CalculateCurrentWeight();
            }
            if (_weightText != null)
            {
                _weightText.text = $"{_caravanData.CalculateCurrentWeight():F1} / {_caravanData.MaxWeightCapacity} Kg";
            }

            // 2. Sehir Basligi
            if (_marketTitleText != null && _cityMarket != null && _cityMarket.Data != null)
            {
                string cityName = _cityMarket.Data.CityNode != null ? _cityMarket.Data.CityNode.NodeName : "Şehir Pazarı";
                _marketTitleText.text = $"{cityName} (Vergi: %{Mathf.RoundToInt(_cityMarket.Data.BaseTaxRate * 100)})";
            }

            UpdateTradeCalculations();
        }

        private void OnBuyClicked()
        {
            if (_selectedItem == null || _cityMarket == null) return;
            bool success = _cityMarket.BuyItem(_caravanData, _selectedItem.ItemId, _selectedQuantity);
            if (success)
            {
                RefreshAll();
            }
        }

        private void OnSellClicked()
        {
            if (_selectedItem == null || _cityMarket == null) return;
            bool success = _cityMarket.SellItem(_caravanData, _selectedItem.ItemId, _selectedQuantity);
            if (success)
            {
                RefreshAll();
            }
        }

        private void OnLeaveMarketClicked()
        {
            Debug.Log("[MarketUI] Pazardan cikildi, Seyahat sahnesine donuluyor.");
            SceneManager.LoadScene("TravelScene");
        }

        private LiveMarketItem FindLiveItem(string itemId)
        {
            if (_cityMarket == null) return null;
            foreach (var item in _cityMarket.MarketItems)
            {
                if (item.Item != null && item.Item.ItemId == itemId) return item;
            }
            return null;
        }

        private CaravanItemSlot FindCaravanSlot(string itemId)
        {
            foreach (var slot in _caravanData.Inventory)
            {
                if (slot.Item != null && slot.Item.ItemId == itemId) return slot;
            }
            return null;
        }
    }
}
