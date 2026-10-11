using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Core;
using Kervan.TradeAndTravel;

namespace Kervan.IntelSystem
{
    public class TownHubUI : MonoBehaviour
    {
        [Header("Yönetici Referansları")]
        [SerializeField] private ReputationManager _reputationManager;
        [SerializeField] private QuestManager _questManager;
        [SerializeField] private ShopManager _shopManager;
        private CaravanData _caravanData;

        [Header("Mevcut Konum")]
        [SerializeField] private string _currentCityName = "Söğüt";
        [SerializeField] private FactionType _cityFaction = FactionType.Ottoman;

        [Header("Üst Bilgi")]
        [SerializeField] private TextMeshProUGUI _cityTitleText;
        [SerializeField] private TextMeshProUGUI _caravanAkceText;
        [SerializeField] private TextMeshProUGUI _statusNotificationText;

        [Header("Sekme Panelleri")]
        [SerializeField] private GameObject _panelDivan;
        [SerializeField] private GameObject _panelIntel;
        [SerializeField] private GameObject _panelShop;

        [Header("Sekme Butonları")]
        [SerializeField] private Button _tabDivanButton;
        [SerializeField] private Button _tabIntelButton;
        [SerializeField] private Button _tabShopButton;

        [Header("Divan & İtibar Elemanları")]
        [SerializeField] private TextMeshProUGUI _repOttomanText;
        [SerializeField] private TextMeshProUGUI _repSteppeText;
        [SerializeField] private TextMeshProUGUI _repSeljukText;
        [SerializeField] private TextMeshProUGUI _repByzantineText;
        [SerializeField] private Transform _submittedIntelContainer;

        [Header("Han & İstihbarat Elemanları")]
        [SerializeField] private List<IntelDataSO> _availableLocalIntels = new List<IntelDataSO>();
        [SerializeField] private Transform _intelListContainer;
        [SerializeField] private GameObject _intelRowTemplate;

        [Header("Dükkân Elemanları")]
        [SerializeField] private TextMeshProUGUI _shopStatusText;
        [SerializeField] private TextMeshProUGUI _shopIncomeText;
        [SerializeField] private Button _buyShopButton;
        [SerializeField] private Button _collectIncomeButton;

        private void Start()
        {
            if (_reputationManager == null) _reputationManager = ReputationManager.Instance;
            if (_questManager == null) _questManager = QuestManager.Instance;
            if (_shopManager == null) _shopManager = ShopManager.Instance;
            if (_caravanData == null && CaravanManager.Instance != null) _caravanData = CaravanManager.Instance.Caravan;

            if (_intelRowTemplate != null) _intelRowTemplate.SetActive(false);

            SetupTabButtons();
            RefreshAllViews();
            ShowTab(0);
        }

        private void SetupTabButtons()
        {
            if (_tabDivanButton != null) _tabDivanButton.onClick.AddListener(() => ShowTab(0));
            if (_tabIntelButton != null) _tabIntelButton.onClick.AddListener(() => ShowTab(1));
            if (_tabShopButton != null) _tabShopButton.onClick.AddListener(() => ShowTab(2));

            if (_buyShopButton != null) _buyShopButton.onClick.AddListener(OnBuyShopClicked);
            if (_collectIncomeButton != null) _collectIncomeButton.onClick.AddListener(OnCollectIncomeClicked);
        }

        public void ShowTab(int tabIndex)
        {
            if (_panelDivan != null) _panelDivan.SetActive(tabIndex == 0);
            if (_panelIntel != null) _panelIntel.SetActive(tabIndex == 1);
            if (_panelShop != null) _panelShop.SetActive(tabIndex == 2);

            RefreshAllViews();
        }

        public void RefreshAllViews()
        {
            UpdateHeader();
            UpdateDivanTab();
            UpdateIntelTab();
            UpdateShopTab();
        }

        private void UpdateHeader()
        {
            if (_cityTitleText != null)
            {
                string factionLabel = _reputationManager != null ? _reputationManager.GetFactionDisplayName(_cityFaction) : _cityFaction.ToString();
                _cityTitleText.text = $"{_currentCityName.ToUpper()} HANI & {factionLabel.ToUpper()} DİVANI";
            }

            if (_caravanAkceText != null)
            {
                int gold = _caravanData != null ? _caravanData.CurrentAkce : 500;
                _caravanAkceText.text = $"Kese: {gold} Akçe";
            }
        }

        private void UpdateDivanTab()
        {
            if (_reputationManager == null) return;

            if (_repOttomanText != null) _repOttomanText.text = FormatRepText(FactionType.Ottoman);
            if (_repSteppeText != null) _repSteppeText.text = FormatRepText(FactionType.Steppe);
            if (_repSeljukText != null) _repSeljukText.text = FormatRepText(FactionType.SeljukLegacy);
            if (_repByzantineText != null) _repByzantineText.text = FormatRepText(FactionType.Byzantine);

            // Populate Submitted Intels
            if (_submittedIntelContainer != null && _intelRowTemplate != null && _questManager != null)
            {
                ClearContainer(_submittedIntelContainer, _intelRowTemplate);
                foreach (var intel in _questManager.CollectedIntels)
                {
                    // Basic display for collected ones
                    var go = Instantiate(_intelRowTemplate, _submittedIntelContainer);
                    go.SetActive(true);
                    var titleTxt = go.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                    var btn = go.GetComponent<Button>();
                    if (titleTxt != null) titleTxt.text = $"Sunulabilir Bilgi: {intel.Title}";
                    if (btn != null) btn.interactable = false; // Add real submit logic later
                }
            }
        }

        private void UpdateIntelTab()
        {
            if (_intelListContainer != null && _intelRowTemplate != null && _questManager != null)
            {
                ClearContainer(_intelListContainer, _intelRowTemplate);
                foreach (var intel in _availableLocalIntels)
                {
                    if (intel == null) continue;
                    var go = Instantiate(_intelRowTemplate, _intelListContainer);
                    go.SetActive(true);
                    
                    var titleTxt = go.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                    if (titleTxt != null) titleTxt.text = intel.Title;
                    
                    var btn = go.GetComponent<Button>();
                    if (btn != null)
                    {
                        var capturedIntel = intel;
                        btn.onClick.AddListener(() => TryGatherIntelFromUI(capturedIntel));
                        // Instead of HasIntel, check if the list Contains it
                        bool hasIntel = false;
                        foreach(var collected in _questManager.CollectedIntels)
                        {
                            if (collected.IntelId == intel.IntelId)
                            {
                                hasIntel = true;
                                break;
                            }
                        }
                        btn.interactable = !hasIntel;
                    }
                }
            }
        }

        private void ClearContainer(Transform container, GameObject template)
        {
            if (container == null) return;
            foreach (Transform child in container)
            {
                if (child.gameObject != template)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private string FormatRepText(FactionType faction)
        {
            int score = _reputationManager.GetReputation(faction);
            var level = _reputationManager.GetReputationLevel(faction);
            string factionName = _reputationManager.GetFactionDisplayName(faction);
            float taxMult = _reputationManager.GetTaxMultiplier(faction);
            string taxDesc = taxMult < 1.0f ? $"(Vergi İndirimi: %{(1f - taxMult) * 100:0})" : "(Standart Vergi)";

            return $"<b>{factionName}</b>: {score} [{level}] {taxDesc}";
        }

        private void UpdateShopTab()
        {
            if (_shopManager == null) return;

            var shop = _shopManager.GetShop(_currentCityName);
            if (shop == null)
            {
                if (_shopStatusText != null) _shopStatusText.text = "Bu şehirde dükkân mülkiyeti bulunmuyor.";
                if (_buyShopButton != null) _buyShopButton.gameObject.SetActive(false);
                if (_collectIncomeButton != null) _collectIncomeButton.gameObject.SetActive(false);
                return;
            }

            if (!shop.isOwned)
            {
                if (_shopStatusText != null)
                {
                    _shopStatusText.text = $"<b>{shop.cityName} Han Tezgâhı</b>\nBedel: {shop.purchaseCost} Akçe | Gerekli İtibar: En az {shop.minReputationRequired}\nGünlük Net Pasif Getiri: +{shop.NetDailyProfit} Akçe/Gün";
                }
                if (_buyShopButton != null)
                {
                    _buyShopButton.gameObject.SetActive(true);
                    var btnText = _buyShopButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = $"DÜKKÂNI AL ({shop.purchaseCost} Akçe)";
                }
                if (_collectIncomeButton != null) _collectIncomeButton.gameObject.SetActive(false);
                if (_shopIncomeText != null) _shopIncomeText.text = "Mülkiyet yok.";
            }
            else
            {
                if (_shopStatusText != null)
                {
                    _shopStatusText.text = $"<b>{shop.cityName} Han Tezgâhı (Faal)</b>\nKalfa tezgâh başında. Günlük Kâr: +{shop.NetDailyProfit} Akçe.";
                }
                if (_buyShopButton != null) _buyShopButton.gameObject.SetActive(false);
                if (_collectIncomeButton != null)
                {
                    _collectIncomeButton.gameObject.SetActive(true);
                    var btnText = _collectIncomeButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) btnText.text = $"KASAYI TAHSİL ET ({shop.storedGoldInTill} Akçe)";
                }
                if (_shopIncomeText != null)
                {
                    _shopIncomeText.text = $"Kasada Biriken Kâr: <b>{shop.storedGoldInTill} Akçe</b>";
                }
            }
        }

        public void TryGatherIntelFromUI(IntelDataSO intel)
        {
            if (_questManager == null) return;
            bool success = _questManager.TryGatherIntel(intel, out string msg);
            ShowNotification(msg);
            RefreshAllViews();
        }

        public void TrySubmitIntelFromUI(IntelDataSO intel)
        {
            if (_questManager == null) return;
            bool success = _questManager.SubmitIntel(intel, out string summary);
            ShowNotification(summary);
            RefreshAllViews();
        }

        private void OnBuyShopClicked()
        {
            if (_shopManager == null) return;
            bool success = _shopManager.TryPurchaseShop(_currentCityName, out string msg);
            ShowNotification(msg);
            RefreshAllViews();
        }

        private void OnCollectIncomeClicked()
        {
            if (_shopManager == null) return;
            int amount = _shopManager.CollectShopProfit(_currentCityName);
            if (amount > 0)
            {
                ShowNotification($"Dükkân kasasından {amount} Akçe kervan hazinesine eklendi!");
            }
            else
            {
                ShowNotification("Kasada henüz tahsil edilecek kâr birikmemiş.");
            }
            RefreshAllViews();
        }

        private void ShowNotification(string message)
        {
            if (_statusNotificationText != null)
            {
                _statusNotificationText.text = message;
            }
            Debug.Log($"[TownHubUI] {message}");
        }
    }
}
// Trigger recompile
