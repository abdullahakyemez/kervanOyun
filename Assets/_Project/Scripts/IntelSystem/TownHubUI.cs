using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Kervan.Core;
using Kervan.Audio;
using Kervan.UI;
using Kervan.TradeAndTravel;

namespace Kervan.IntelSystem
{
    /// <summary>
    /// Şehir ve Kervansaray merkez ekranının (<c>TownHubScene</c>) sekme kategorileri.
    /// </summary>
    public enum TownHubTab
    {
        Market = 0,
        Caravanserai = 1,
        Divan = 2
    }

    /// <summary>
    /// Dünya Haritasından bir şehre veya kervansaraya girildiğinde açılan 3 sekmeli
    /// (Pazar / Kervansaray / Divan) merkez ekranını yönetir.
    /// <see cref="GameSession.CurrentNodeId"/> üzerinden aktif <see cref="LocationNodeSO"/> verisini çözer,
    /// sekmeye göre tam ekran minyatür arka planı değiştirir ve Dünya Haritasına dönüşü sağlar.
    /// </summary>
    public class TownHubUI : MonoBehaviour
    {
        [Header("Veri Kaynakları (Location & Route)")]
        [Tooltip("Aktif düğümü (LocationNodeSO) CurrentNodeId ile bulmak için kullanılan rota veri tabanı")]
        [SerializeField] private RouteDatabaseSO _routeDatabase;
        [Tooltip("Editörde doğrudan TownHubScene'den başlatıldığında veya yedek olarak kullanılacak düğüm tanımı")]
        [SerializeField] private LocationNodeSO _fallbackNode;

        [Header("Tam Ekran Sekme Arka Planları")]
        [Tooltip("Tam ekran arka plan görselini gösteren Image bileşeni (BG_TownHub)")]
        [SerializeField] private Image _backgroundImage;
        [Tooltip("Pazar sekmesi aktifken gösterilecek minyatür arka plan (UI_BG_Market)")]
        [SerializeField] private Sprite _bgMarket;
        [Tooltip("Kervansaray sekmesi aktifken gösterilecek minyatür arka plan (UI_BG_Caravanserai)")]
        [SerializeField] private Sprite _bgCaravanserai;
        [Tooltip("Divan sekmesi aktifken gösterilecek minyatür arka plan (UI_BG_Divan)")]
        [SerializeField] private Sprite _bgDivan;

        [Header("Üst Bar & Şehir Başlık Sancağı")]
        [Tooltip("Üst sol kaynak ve takvim göstergesi (TopBarHUD)")]
        [SerializeField] private TopBarHUD _topBarHUD;
        [Tooltip("Şehir/Kervansaray adını ve bağlı olduğu devleti gösteren başlık metni")]
        [SerializeField] private TextMeshProUGUI _cityTitleText;
        [Tooltip("Şehir/Kervansaray alt açıklamasını veya tesis bilgisini gösteren metin (opsiyonel)")]
        [SerializeField] private TextMeshProUGUI _citySubtitleText;
        [Tooltip("Başlık sancağındaki düğüm tipi ikonu (Şehir / Han ikonu - opsiyonel)")]
        [SerializeField] private Image _cityNodeIconImage;
        [Tooltip("Dünya Haritasına dönüş (Şehirden Çık) butonu")]
        [SerializeField] private Button _returnToMapButton;
        [Tooltip("İşlem geri bildirim/durum mesajı metni (opsiyonel)")]
        [SerializeField] private TextMeshProUGUI _statusNotificationText;
        [Tooltip("Eski prototip uyumluluğu için akçe metni (TopBarHUD varsa boş bırakılabilir)")]
        [SerializeField] private TextMeshProUGUI _caravanAkceText;

        [Header("3'lü Sekme Çubuğu (Pazar / Kervansaray / Divan)")]
        [SerializeField] private Button _tabMarketButton;
        [SerializeField] private Button _tabCaravanseraiButton;
        [SerializeField] private Button _tabDivanButton;
        [Tooltip("Aktif sekme butonunun arka plan görseli (sidebarBtnOnClick)")]
        [SerializeField] private Sprite _tabActiveSprite;
        [Tooltip("Pasif sekme butonunun arka plan görseli (SidebarBtnOffClick)")]
        [SerializeField] private Sprite _tabInactiveSprite;
        [Tooltip("Aktif sekme metin rengi (Koyu Mürekkep #1E1815)")]
        [SerializeField] private Color _tabActiveTextColor = new Color(0.12f, 0.09f, 0.08f, 1f);
        [Tooltip("Pasif sekme metin rengi (Parşömen Kremi #F5E6D3)")]
        [SerializeField] private Color _tabInactiveTextColor = new Color(0.96f, 0.90f, 0.83f, 1f);
        [Tooltip("Kilitli/olmayan tesis sekmesi metin rengi")]
        [SerializeField] private Color _tabDisabledTextColor = new Color(0.60f, 0.56f, 0.52f, 0.55f);

        [Header("Sekme Ana Panelleri")]
        [Tooltip("Pazar (Alış-Satış) sekmesi kök paneli (Panel_Market)")]
        [SerializeField] private GameObject _panelMarket;
        [Tooltip("Kervansaray (Dinlenme, Erzak, Muhafız) sekmesi kök paneli (Panel_Caravanserai)")]
        [SerializeField] private GameObject _panelCaravanserai;
        [Tooltip("Divan (Makam, İtibar, İstihbarat, Dükkân) sekmesi kök paneli (Panel_Divan)")]
        [SerializeField] private GameObject _panelDivan;

        [Header("Divan, İstihbarat & Dükkân Elemanları (A6 Hazırlığı)")]
        [SerializeField] private ReputationManager _reputationManager;
        [SerializeField] private QuestManager _questManager;
        [SerializeField] private ShopManager _shopManager;
        [SerializeField] private TextMeshProUGUI _repOttomanText;
        [SerializeField] private TextMeshProUGUI _repSteppeText;
        [SerializeField] private TextMeshProUGUI _repSeljukText;
        [SerializeField] private TextMeshProUGUI _repByzantineText;
        [SerializeField] private Transform _submittedIntelContainer;
        [SerializeField] private List<IntelDataSO> _availableLocalIntels = new List<IntelDataSO>();
        [SerializeField] private Transform _intelListContainer;
        [SerializeField] private GameObject _intelRowTemplate;
        [SerializeField] private TextMeshProUGUI _shopStatusText;
        [SerializeField] private TextMeshProUGUI _shopIncomeText;
        [SerializeField] private Button _buyShopButton;
        [SerializeField] private Button _collectIncomeButton;

        private GameSession _gameSession;
        private CaravanData _caravanData;
        private LocationNodeSO _currentNode;
        private TownHubTab _activeTab = TownHubTab.Market;
        private bool _isMarketAvailable = true;
        private bool _isInnAvailable = true;
        private bool _isDivanAvailable = true;

        /// <summary>
        /// Şu an içinde bulunulan şehir veya kervansarayın veri modeli.
        /// </summary>
        public LocationNodeSO CurrentNode => _currentNode;

        /// <summary>
        /// Şu an açık olan sekme.
        /// </summary>
        public TownHubTab ActiveTab => _activeTab;

        private void Start()
        {
            ResolveSessionAndNode();

            if (_reputationManager == null) _reputationManager = ReputationManager.Instance;
            if (_questManager == null) _questManager = QuestManager.Instance;
            if (_shopManager == null) _shopManager = ShopManager.Instance;

            if (_intelRowTemplate != null) _intelRowTemplate.SetActive(false);

            SetupButtons();
            ConfigureAvailableFacilities();
            RefreshAllViews();

            // Düğüm tipine göre başlangıç sekmesini seç (Şehir -> Pazar, Kervansaray -> Han)
            TownHubTab initialTab = DetermineInitialTab();
            ShowTab(initialTab, playSound: false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
            EventBus.Subscribe<DayPassedEvent>(OnDayPassed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
            EventBus.Unsubscribe<DayPassedEvent>(OnDayPassed);
        }

        private void Update()
        {
            // Android Geri tuşu veya PC Esc tuşu ile Dünya Haritasına güvenli dönüş
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                OnReturnToMapClicked();
            }
        }

        /// <summary>
        /// <see cref="GameSession"/> üzerinden aktif düğüm kimliğini okur ve <see cref="LocationNodeSO"/> karşılığını çözer.
        /// </summary>
        private void ResolveSessionAndNode()
        {
            if (Services.TryGet<GameSession>(out var session))
            {
                _gameSession = session;
                _caravanData = session.Caravan;
            }
            else if (CaravanManager.Instance != null)
            {
                _caravanData = CaravanManager.Instance.Caravan;
            }

            string nodeId = _gameSession != null ? _gameSession.CurrentNodeId : GameSession.DefaultStartNodeId;

            if (_routeDatabase != null)
            {
                _currentNode = _routeDatabase.FindNodeById(nodeId);
            }

            if (_currentNode == null && _fallbackNode != null)
            {
                _currentNode = _fallbackNode;
            }

            if (_currentNode == null)
            {
                Debug.LogWarning($"[TownHubUI] '{nodeId}' kimliği için LocationNodeSO bulunamadı! RouteDatabase veya FallbackNode atanmalı.");
            }
        }

        private void SetupButtons()
        {
            if (_tabMarketButton != null)
            {
                _tabMarketButton.onClick.RemoveAllListeners();
                _tabMarketButton.onClick.AddListener(() => ShowTab(TownHubTab.Market, playSound: true));
            }

            if (_tabCaravanseraiButton != null)
            {
                _tabCaravanseraiButton.onClick.RemoveAllListeners();
                _tabCaravanseraiButton.onClick.AddListener(() => ShowTab(TownHubTab.Caravanserai, playSound: true));
            }

            if (_tabDivanButton != null)
            {
                _tabDivanButton.onClick.RemoveAllListeners();
                _tabDivanButton.onClick.AddListener(() => ShowTab(TownHubTab.Divan, playSound: true));
            }

            if (_returnToMapButton != null)
            {
                _returnToMapButton.onClick.RemoveAllListeners();
                _returnToMapButton.onClick.AddListener(OnReturnToMapClicked);
            }

            if (_buyShopButton != null)
            {
                _buyShopButton.onClick.RemoveAllListeners();
                _buyShopButton.onClick.AddListener(OnBuyShopClicked);
            }

            if (_collectIncomeButton != null)
            {
                _collectIncomeButton.onClick.RemoveAllListeners();
                _collectIncomeButton.onClick.AddListener(OnCollectIncomeClicked);
            }
        }

        /// <summary>
        /// Bulunulan düğümün <see cref="LocationNodeSO"/> özelliklerine göre hangi sekmelerin erişilebilir olduğunu belirler.
        /// </summary>
        private void ConfigureAvailableFacilities()
        {
            if (_currentNode == null)
            {
                _isMarketAvailable = true;
                _isInnAvailable = true;
                _isDivanAvailable = true;
                return;
            }

            bool isCity = _currentNode.NodeType == NodeType.City;
            _isMarketAvailable = _currentNode.HasMarket;
            _isInnAvailable = _currentNode.HasInn || _currentNode.NodeType == NodeType.Caravanserai;
            _isDivanAvailable = isCity;

            if (_tabMarketButton != null) _tabMarketButton.interactable = _isMarketAvailable;
            if (_tabCaravanseraiButton != null) _tabCaravanseraiButton.interactable = _isInnAvailable;
            if (_tabDivanButton != null) _tabDivanButton.interactable = _isDivanAvailable;
        }

        private TownHubTab DetermineInitialTab()
        {
            if (_currentNode != null && _currentNode.NodeType == NodeType.Caravanserai && _isInnAvailable)
            {
                return TownHubTab.Caravanserai;
            }

            if (_isMarketAvailable) return TownHubTab.Market;
            if (_isInnAvailable) return TownHubTab.Caravanserai;
            return TownHubTab.Divan;
        }

        /// <summary>
        /// İstenen sekmeyi (<see cref="TownHubTab"/>) açar, tam ekran arka planı ve sekme buton görsellerini günceller.
        /// </summary>
        public void ShowTab(TownHubTab tab, bool playSound = true)
        {
            if (tab == TownHubTab.Market && !_isMarketAvailable)
            {
                ShowNotification("Bu konaklama noktasında büyük şehir pazarı bulunmuyor.");
                return;
            }

            if (tab == TownHubTab.Caravanserai && !_isInnAvailable)
            {
                ShowNotification("Bu noktada kervansaray hizmeti bulunmuyor.");
                return;
            }

            if (tab == TownHubTab.Divan && !_isDivanAvailable)
            {
                ShowNotification("Kervansaraylarda beylik divanı bulunmaz; yalnız şehirlerde divana çıkılabilir.");
                return;
            }

            _activeTab = tab;

            if (_panelMarket != null) _panelMarket.SetActive(tab == TownHubTab.Market);
            if (_panelCaravanserai != null) _panelCaravanserai.SetActive(tab == TownHubTab.Caravanserai);
            if (_panelDivan != null) _panelDivan.SetActive(tab == TownHubTab.Divan);

            UpdateBackgroundForTab(tab);
            UpdateTabButtonVisuals();
            RefreshAllViews();

            if (playSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPaperRustle();
            }
        }

        /// <summary>
        /// Inspector veya UnityEvent üzerinden int indeks ile sekme değiştirmek için yardımcı metot (0: Pazar, 1: Han, 2: Divan).
        /// </summary>
        public void ShowTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex > 2) tabIndex = 0;
            ShowTab((TownHubTab)tabIndex, playSound: true);
        }

        private void UpdateBackgroundForTab(TownHubTab tab)
        {
            if (_backgroundImage == null) return;

            Sprite targetSprite = null;
            switch (tab)
            {
                case TownHubTab.Market:
                    targetSprite = _bgMarket;
                    break;
                case TownHubTab.Caravanserai:
                    targetSprite = _bgCaravanserai != null ? _bgCaravanserai : _bgMarket;
                    break;
                case TownHubTab.Divan:
                    targetSprite = _bgDivan != null ? _bgDivan : _bgMarket;
                    break;
            }

            if (targetSprite != null)
            {
                _backgroundImage.sprite = targetSprite;
            }
        }

        private void UpdateTabButtonVisuals()
        {
            ApplySingleTabVisual(_tabMarketButton, _activeTab == TownHubTab.Market, _isMarketAvailable);
            ApplySingleTabVisual(_tabCaravanseraiButton, _activeTab == TownHubTab.Caravanserai, _isInnAvailable);
            ApplySingleTabVisual(_tabDivanButton, _activeTab == TownHubTab.Divan, _isDivanAvailable);
        }

        private void ApplySingleTabVisual(Button button, bool isSelected, bool isAvailable)
        {
            if (button == null) return;

            Image btnImage = button.GetComponent<Image>();
            if (btnImage != null && _tabActiveSprite != null && _tabInactiveSprite != null)
            {
                btnImage.sprite = isSelected ? _tabActiveSprite : _tabInactiveSprite;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                if (!isAvailable)
                {
                    label.color = _tabDisabledTextColor;
                }
                else
                {
                    label.color = isSelected ? _tabActiveTextColor : _tabInactiveTextColor;
                }
            }
        }

        /// <summary>
        /// Üst başlık sancağını ve aktif sekme görünümlerini günceller.
        /// </summary>
        public void RefreshAllViews()
        {
            UpdateHeader();
            UpdateDivanTab();
            UpdateIntelTab();
            UpdateShopTab();
        }

        private void UpdateHeader()
        {
            string nodeName = _currentNode != null ? _currentNode.NodeName : "Söğüt";
            string factionLabel = _currentNode != null ? _currentNode.GetFactionDisplayName() : "Osmanlı";

            if (_cityTitleText != null)
            {
                _cityTitleText.text = $"{nodeName.ToUpper()} — {factionLabel}";
            }

            if (_citySubtitleText != null)
            {
                if (_currentNode != null)
                {
                    string typeLabel = _currentNode.NodeType == NodeType.Caravanserai ? "Menzil Kervansarayı" : "Şehir Merkezi";
                    _citySubtitleText.text = $"{typeLabel} · {_currentNode.MarketGoodsSummary}";
                }
                else
                {
                    _citySubtitleText.text = "Şehir Merkezi";
                }
            }

            if (_cityNodeIconImage != null && _currentNode != null && _currentNode.NodeIcon != null)
            {
                _cityNodeIconImage.sprite = _currentNode.NodeIcon;
                _cityNodeIconImage.enabled = true;
            }

            if (_caravanAkceText != null && _caravanData != null)
            {
                _caravanAkceText.text = $"Kese: {_caravanData.CurrentAkce} Akçe";
            }
        }

        private void OnCaravanStatsChanged(CaravanStatsChangedEvent evt)
        {
            UpdateHeader();
        }

        private void OnDayPassed(DayPassedEvent evt)
        {
            UpdateHeader();
        }

        /// <summary>
        /// Şehir/Kervansaray merkezinden çıkıp Dünya Haritasına (<see cref="SceneIds.WorldMap"/>) döner.
        /// </summary>
        public void OnReturnToMapClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            SceneLoader.Load(SceneIds.WorldMap);
        }

        #region Divan, İstihbarat ve Dükkân Yardımcıları (A6 Hazırlığı)

        private void UpdateDivanTab()
        {
            if (_reputationManager == null) return;

            if (_repOttomanText != null) _repOttomanText.text = FormatRepText(FactionType.Ottoman);
            if (_repSteppeText != null) _repSteppeText.text = FormatRepText(FactionType.Steppe);
            if (_repSeljukText != null) _repSeljukText.text = FormatRepText(FactionType.SeljukLegacy);
            if (_repByzantineText != null) _repByzantineText.text = FormatRepText(FactionType.Byzantine);

            if (_submittedIntelContainer != null && _intelRowTemplate != null && _questManager != null)
            {
                ClearContainer(_submittedIntelContainer, _intelRowTemplate);
                foreach (var intel in _questManager.CollectedIntels)
                {
                    var go = Instantiate(_intelRowTemplate, _submittedIntelContainer);
                    go.SetActive(true);
                    var titleTxt = go.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                    var btn = go.GetComponent<Button>();
                    if (titleTxt != null) titleTxt.text = $"Sunulabilir Bilgi: {intel.Title}";
                    if (btn != null) btn.interactable = false;
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
                        bool hasIntel = false;
                        foreach (var collected in _questManager.CollectedIntels)
                        {
                            if (string.Equals(collected.IntelId, intel.IntelId, StringComparison.Ordinal))
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

            string cityKey = _currentNode != null ? _currentNode.NodeName : "Söğüt";
            var shop = _shopManager.GetShop(cityKey);
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
            _questManager.TryGatherIntel(intel, out string msg);
            ShowNotification(msg);
            RefreshAllViews();
        }

        public void TrySubmitIntelFromUI(IntelDataSO intel)
        {
            if (_questManager == null) return;
            _questManager.SubmitIntel(intel, out string summary);
            ShowNotification(summary);
            RefreshAllViews();
        }

        private void OnBuyShopClicked()
        {
            if (_shopManager == null) return;
            string cityKey = _currentNode != null ? _currentNode.NodeName : "Söğüt";
            _shopManager.TryPurchaseShop(cityKey, out string msg);
            ShowNotification(msg);
            RefreshAllViews();
        }

        private void OnCollectIncomeClicked()
        {
            if (_shopManager == null) return;
            string cityKey = _currentNode != null ? _currentNode.NodeName : "Söğüt";
            int amount = _shopManager.CollectShopProfit(cityKey);
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

        #endregion

        public void ShowNotification(string message)
        {
            if (_statusNotificationText != null)
            {
                _statusNotificationText.text = message;
            }
            Debug.Log($"[TownHubUI] {message}");
        }
    }
}

