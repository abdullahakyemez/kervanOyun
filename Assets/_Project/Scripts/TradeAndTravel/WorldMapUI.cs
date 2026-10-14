using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Kervan.Core;
using Kervan.Audio;
using Kervan.TradeAndTravel;

namespace Kervan.UI
{
    /// <summary>
    /// Dünya Haritası sahnesinin (WorldMapScene) merkezi orkestrasyon kontrolcüsü.
    /// - Üst Kaynak HUD'ını (Akçe, Erzak, Tarih)
    /// - Sol alt Kervan & Yük butonunu (Sidebar tetikleyici)
    /// - Sağ üst yardımcı araçlar çekmecesini (Ses açma/kapama, Ayarlar, Ana Menüye dönüş)
    /// - Oyuncunun bulunduğu konumu belirten altın çerçeve ve sancak göstergesini (CurrentNodeMarkerUI)
    /// - Şehir/Han düğümlerine tıklandığında alttan açılan detay kartını (BottomNodeInfoCardUI)
    /// - Harita gezinmesini (MapPanZoomController) ve düğüm odaklamasını yönetir.
    /// </summary>
    public class WorldMapUI : MonoBehaviour
    {
        [Header("Harita Gezinme (Pan & Zoom)")]
        [SerializeField] private MapPanZoomController _panZoomController;
        [Tooltip("Başlangıçta haritanın odaklanacağı ilk düğüm (örn: Node_Sogut)")]
        [SerializeField] private RectTransform _initialFocusNode;

        [Header("Üst Kaynak / HUD Barı")]
        [SerializeField] private TextMeshProUGUI _akceText;
        [SerializeField] private TextMeshProUGUI _erzakText;
        [SerializeField] private TextMeshProUGUI _dateText;

        [Header("Kervan & Yük Yan Çekmecesi")]
        [Tooltip("Sol altta yer alan Kervan ve envanter butonu")]
        [SerializeField] private Button _bottomLeftInventoryButton;
        [SerializeField] private InventorySidebarUI _inventorySidebar;

        [Header("Sağ Üst Yardımcı Araçlar (Utility Drawer)")]
        [Tooltip("Sağ üstteki açılır çekmece kontrolcüsü")]
        [SerializeField] private MenuDrawerController _utilityDrawer;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _volumeToggleButton;
        [SerializeField] private Image _volumeIconImage;
        [SerializeField] private Sprite _volumeOnSprite;
        [SerializeField] private Sprite _volumeOffSprite;
        [SerializeField] private Button _backToMenuButton;
        [SerializeField] private SettingsPopupUI _settingsPopup;

        [Header("Aktif Konum Göstergesi (Current Location Marker)")]
        [Tooltip("Kullanıcının bulunduğu düğüm üzerinde parıldayan altın halka ve sancak")]
        [SerializeField] private CurrentNodeMarkerUI _currentNodeMarker;

        [Header("Alttan Açılan Düğüm Bilgi Kartı (Slide-Up)")]
        [Tooltip("Yeni modüler alttan açılan şehir/han bilgi kartı")]
        [SerializeField] private BottomNodeInfoCardUI _bottomNodeInfoCard;

        [Header("Veri Referansları (ScriptableObjects)")]
        [SerializeField] private LocationNodeSO[] _locationNodeDataList;

        [Header("Müzik & Ses Ayarları")]
        [Tooltip("Harita ve seyahat arka plan müziği (BGM_Travel)")]
        [SerializeField] private AudioClip _bgmMapMusic;

        [Header("Sahne İsimleri")]
        [SerializeField] private string _mainMenuSceneName = "MainMenuScene";
        [SerializeField] private string _travelSceneName = "TravelScene";
        [SerializeField] private string _townHubSceneName = "TownHubScene";

        private string _currentCityName = "Söğüt";
        private string _selectedNodeName = "";
        private bool _isMuted = false;
        private AudioSource _localAudioSource;
        private readonly Dictionary<string, LocationNodeSO> _nodeDataMap = new Dictionary<string, LocationNodeSO>(StringComparer.OrdinalIgnoreCase);

        private void Awake()
        {
            BuildNodeDataDictionary();
        }

        private void Start()
        {
            FindMissingReferences();
            PlayMapMusic();
            LoadCurrentLocation();
            RegisterListeners();
            UpdateTopResourceHUD();
            SetupNodeButtons();
            InitCurrentLocationMarker();
            InitVolumeState();

            // Başlangıçta alt bilgi kartını gizli tut
            if (_bottomNodeInfoCard != null)
            {
                _bottomNodeInfoCard.HideCard();
            }

            // Başlangıçta mevcut konuma (örn: Söğüt) yumuşakça odaklan
            FocusOnCurrentCity(true);
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        private void BuildNodeDataDictionary()
        {
            if (_locationNodeDataList == null || _locationNodeDataList.Length == 0)
            {
                // Resources veya ScriptableObjects klasöründen dinamik yükleme dene
                _locationNodeDataList = Resources.LoadAll<LocationNodeSO>("Nodes");
            }

            if (_locationNodeDataList != null)
            {
                foreach (var node in _locationNodeDataList)
                {
                    if (node != null && !string.IsNullOrEmpty(node.NodeName))
                    {
                        _nodeDataMap[node.NodeName] = node;
                        if (!string.IsNullOrEmpty(node.NodeId))
                        {
                            _nodeDataMap[node.NodeId] = node;
                        }
                    }
                }
            }
        }

        private void FindMissingReferences()
        {
            if (_panZoomController == null) _panZoomController = FindAnyObjectByType<MapPanZoomController>();
            if (_inventorySidebar == null) _inventorySidebar = FindAnyObjectByType<InventorySidebarUI>();
            if (_bottomNodeInfoCard == null) _bottomNodeInfoCard = FindAnyObjectByType<BottomNodeInfoCardUI>();
            if (_currentNodeMarker == null) _currentNodeMarker = FindAnyObjectByType<CurrentNodeMarkerUI>();
            if (_utilityDrawer == null) _utilityDrawer = FindAnyObjectByType<MenuDrawerController>();
            if (_settingsPopup == null) _settingsPopup = FindAnyObjectByType<SettingsPopupUI>();

            // Sol alt Kervan butonu
            if (_bottomLeftInventoryButton == null)
            {
                GameObject invBtn = GameObject.Find("Btn_Inventory") ?? 
                                   GameObject.Find("BtnInventory") ?? 
                                   GameObject.Find("Btn_Caravan") ?? 
                                   GameObject.Find("BtnCaravan");
                if (invBtn != null) _bottomLeftInventoryButton = invBtn.GetComponent<Button>();
            }

            // Geri butonu
            if (_backToMenuButton == null)
            {
                GameObject backBtn = GameObject.Find("Btn_Back") ?? GameObject.Find("BtnBack") ?? GameObject.Find("Btn_Exit");
                if (backBtn != null) _backToMenuButton = backBtn.GetComponent<Button>();
            }
        }

        private void LoadCurrentLocation()
        {
            _currentCityName = PlayerPrefs.GetString("CurrentCity", "Söğüt");
            Debug.Log($"[WorldMapUI] Oyuncunun mevcut konumu yüklendi: {_currentCityName}");
        }

        private void RegisterListeners()
        {
            if (_bottomLeftInventoryButton != null) _bottomLeftInventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            if (_backToMenuButton != null) _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
            if (_volumeToggleButton != null) _volumeToggleButton.onClick.AddListener(OnVolumeToggleClicked);

            EventBus.Subscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
        }

        private void UnregisterListeners()
        {
            if (_bottomLeftInventoryButton != null) _bottomLeftInventoryButton.onClick.RemoveListener(OnInventoryButtonClicked);
            if (_backToMenuButton != null) _backToMenuButton.onClick.RemoveListener(OnBackToMenuClicked);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (_volumeToggleButton != null) _volumeToggleButton.onClick.RemoveListener(OnVolumeToggleClicked);

            EventBus.Unsubscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
        }

        private void SetupNodeButtons()
        {
            // Sahnede adı "Node_" ile başlayan tüm şehir/han düğüm butonlarını dinamik bul ve bağla
            Button[] allButtons = GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name.StartsWith("Node_"))
                {
                    string rawName = btn.gameObject.name.Substring(5);
                    string formattedName = rawName.Replace("_", " ");
                    RectTransform nodeRect = btn.GetComponent<RectTransform>();

                    // DİKKAT: RemoveAllListeners() çağrılmıyor; böylece UIButtonSound gibi ses dinleyicileri korunur
                    btn.onClick.AddListener(() => OnNodeClicked(formattedName, nodeRect));
                }
            }
        }

        private void InitCurrentLocationMarker()
        {
            RectTransform currentRect = FindNodeRectByName(_currentCityName);
            if (currentRect != null && _currentNodeMarker != null)
            {
                _currentNodeMarker.AttachToNode(currentRect);
            }
        }

        private void FocusOnCurrentCity(bool instant)
        {
            RectTransform currentRect = FindNodeRectByName(_currentCityName);
            if (currentRect != null && _panZoomController != null)
            {
                _panZoomController.FocusOnTarget(currentRect, 1.4f, instant);
            }
        }

        private RectTransform FindNodeRectByName(string cityName)
        {
            string cleanSearch = cityName.Replace(" ", "_").ToLowerInvariant();

            Button[] allButtons = GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name.ToLowerInvariant().Contains(cleanSearch))
                {
                    return btn.GetComponent<RectTransform>();
                }
            }

            // Fallback: Söğüt düğümü
            GameObject sogut = GameObject.Find("Node_Sogut");
            return sogut != null ? sogut.GetComponent<RectTransform>() : null;
        }

        public void UpdateTopResourceHUD()
        {
            var caravan = CaravanManager.Instance != null ? CaravanManager.Instance.Caravan : null;

            int akce = caravan != null ? caravan.CurrentAkce : 500;
            int food = caravan != null ? caravan.CurrentFood : 20;

            if (_akceText != null) _akceText.text = akce.ToString();
            if (_erzakText != null) _erzakText.text = $"{food} Gün";
            if (_dateText != null) _dateText.text = "14 Mayıs 1326";
        }

        private void OnCaravanStatsChanged(CaravanStatsChangedEvent evt)
        {
            if (_akceText != null) _akceText.text = evt.CurrentAkce.ToString();
            if (_erzakText != null) _erzakText.text = $"{evt.CurrentFood} Gün";
        }

        #region Düğüm Seçimi & Bilgi Kartı (Node Interaction)

        private void OnNodeClicked(string nodeName, RectTransform nodeRect)
        {
            _selectedNodeName = nodeName;
            Debug.Log($"[WorldMapUI] Düğüm seçildi: {nodeName}");

            // 1. Haritayı yumuşakça bu düğüme odakla
            if (_panZoomController != null && nodeRect != null)
            {
                _panZoomController.FocusOnTarget(nodeRect, 1.4f, false);
            }

            // 2. Düğüm verisini çözümle
            _nodeDataMap.TryGetValue(nodeName, out LocationNodeSO nodeSO);

            bool isCurrentLocation = string.Equals(_selectedNodeName, _currentCityName, StringComparison.OrdinalIgnoreCase);

            // Mesafe ve Erzak hesaplaması (Mock/Prosedürel rota formülü)
            int distanceDays = isCurrentLocation ? 0 : CalculateEstimatedDays(_currentCityName, nodeName);
            int foodRequired = distanceDays; // Gün başına 1 erzak birimi

            // 3. Alttan açılan detay kartını göster
            if (_bottomNodeInfoCard != null)
            {
                _bottomNodeInfoCard.ShowNode(
                    nodeName, 
                    nodeSO, 
                    isCurrentLocation, 
                    distanceDays, 
                    foodRequired, 
                    () => OnNodeActionConfirmed(nodeName, isCurrentLocation)
                );
            }
        }

        private int CalculateEstimatedDays(string fromCity, string toCity)
        {
            // Komşu şehir mesafeleri (Temel formül)
            string f = fromCity.ToLowerInvariant();
            string t = toCity.ToLowerInvariant();

            if (f.Contains("sogut") && t.Contains("iznik")) return 2;
            if (f.Contains("sogut") && t.Contains("bursa")) return 3;
            if (f.Contains("sogut") && t.Contains("bilecik")) return 1;
            if (f.Contains("sogut") && t.Contains("kutahya")) return 3;
            if (f.Contains("sogut") && t.Contains("domanic")) return 1;

            return UnityEngine.Random.Range(2, 5); // Varsayılan seyahat süresi
        }

        private void OnNodeActionConfirmed(string targetNodeName, bool isCurrentLocation)
        {
            if (isCurrentLocation)
            {
                // Bulunulan merkeze giriş yap (Pazar / Divan / Han)
                Debug.Log($"[WorldMapUI] {targetNodeName} merkezine giriş yapılıyor.");
                SceneManager.LoadScene(!string.IsNullOrEmpty(_townHubSceneName) ? _townHubSceneName : "MarketScene");
            }
            else
            {
                // Yeni merkeze seyahat başlat
                Debug.Log($"[WorldMapUI] {_currentCityName} noktasından {targetNodeName} yönüne seyahat başlatılıyor.");
                PlayerPrefs.SetString("TravelDestination", targetNodeName);
                PlayerPrefs.SetString("CurrentCity", targetNodeName); // Varış şehri olarak güncelle
                PlayerPrefs.Save();

                SceneManager.LoadScene(!string.IsNullOrEmpty(_travelSceneName) ? _travelSceneName : "TravelScene");
            }
        }

        #endregion

        #region Buton ve Çekmece Olayları (UI Handlers)

        private void OnInventoryButtonClicked()
        {
            if (_inventorySidebar != null)
            {
                _inventorySidebar.OpenSidebar();

                // Çekmece açılınca alttaki bilgi kartını kapat
                if (_bottomNodeInfoCard != null && _bottomNodeInfoCard.IsOpen)
                {
                    _bottomNodeInfoCard.HideCard();
                }
            }
            else
            {
                Debug.LogWarning("[WorldMapUI] InventorySidebar referansı boş!");
            }
        }

        private void OnSettingsClicked()
        {
            if (_settingsPopup != null)
            {
                _settingsPopup.Show();
            }
            else
            {
                Debug.LogWarning("[WorldMapUI] SettingsPopup referansı boş!");
            }
        }

        private void InitVolumeState()
        {
            _isMuted = AudioListener.volume <= 0.01f || (AudioManager.Instance != null && AudioManager.Instance.MasterVolume <= 0.01f);
            UpdateVolumeIcon();
        }

        private void OnVolumeToggleClicked()
        {
            _isMuted = !_isMuted;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(_isMuted ? 0f : 1f);
            }

            // Global ses seviyesini doğrudan kontrol et
            AudioListener.volume = _isMuted ? 0f : 1f;
            AudioListener.pause = _isMuted;

            if (_localAudioSource != null)
            {
                _localAudioSource.mute = _isMuted;
            }

            // Sahnede var olabilecek bağımsız AudioSource bileşenlerini de sustur
            var allSources = FindObjectsByType<AudioSource>();
            foreach (var src in allSources)
            {
                src.mute = _isMuted;
            }

            UpdateVolumeIcon();
        }

        private void UpdateVolumeIcon()
        {
            if (_volumeIconImage != null && _volumeOnSprite != null && _volumeOffSprite != null)
            {
                _volumeIconImage.sprite = _isMuted ? _volumeOffSprite : _volumeOnSprite;
            }
        }

        private void OnBackToMenuClicked()
        {
            Debug.Log("[WorldMapUI] Ana Menüye dönülüyor: " + _mainMenuSceneName);
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        private void PlayMapMusic()
        {
            // Eğer Main Camera üzerinde önceden PlayOnAwake ile bırakılmış rogue AudioSource varsa durdur
            var camAudio = Camera.main != null ? Camera.main.GetComponent<AudioSource>() : null;
            if (camAudio != null && camAudio.isPlaying)
            {
                camAudio.Stop();
                camAudio.playOnAwake = false;
                Debug.Log("[WorldMapUI] Main Camera üzerindeki çakışan AudioSource çift çalmayı engellemek için durduruldu.");
            }

            AudioClip clip = _bgmMapMusic;
            if (clip == null && AudioManager.Instance != null && AudioManager.Instance.Library != null)
            {
                clip = AudioManager.Instance.Library.BgmTravel;
            }

            if (clip == null)
            {
                // Resources veya varsayılan arama
                clip = Resources.Load<AudioClip>("Audio/Music/BGM_Travel");
            }

            if (clip != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayMusic(clip, 1.2f);
                }
                else
                {
                    // Sahnede AudioManager yoksa bile sessiz kalmasın, yerel çal
                    if (_localAudioSource == null)
                    {
                        _localAudioSource = gameObject.AddComponent<AudioSource>();
                        _localAudioSource.loop = true;
                        _localAudioSource.playOnAwake = false;
                    }
                    _localAudioSource.clip = clip;
                    _localAudioSource.volume = 0.75f;
                    _localAudioSource.Play();
                }
            }
            else
            {
                Debug.LogWarning("[WorldMapUI] Harita fon müziği (BGM_Travel) bulunamadı! Inspector'da BgmMapMusic alanına BGM_Travel.mp3 sürükleyin.");
            }
        }

        #endregion
    }
}
