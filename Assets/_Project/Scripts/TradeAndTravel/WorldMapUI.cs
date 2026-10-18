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

        [Header("Rota Veri Tabanı (Route Graph)")]
        [Tooltip("Şehirler arası komşuluk ve mesafe verilerini içeren RouteDatabaseSO varlığı")]
        [SerializeField] private RouteDatabaseSO _routeDatabase;

        [Header("Müzik & Ses Ayarları")]
        [Tooltip("Harita ve seyahat arka plan müziği (BGM_Travel)")]
        [SerializeField] private AudioClip _bgmMapMusic;

        [Header("Sahne İsimleri")]
        [SerializeField] private string _mainMenuSceneName = "MainMenuScene";
        [SerializeField] private string _travelSceneName = "TravelScene";
        [SerializeField] private string _townHubSceneName = "TownHubScene";

        private const string DefaultStartNodeId = GameSession.DefaultStartNodeId;

        private GameSession _gameSession;
        private RouteGraph _routeGraph;
        private string _currentNodeId = DefaultStartNodeId;
        private bool _isMuted = false;
        private AudioSource _localAudioSource;
        private MapNodeView[] _mapNodeViews;
        private readonly Dictionary<string, MapNodeView> _nodeViewsById = new Dictionary<string, MapNodeView>(StringComparer.Ordinal);

        private void Start()
        {
            FindMissingReferences();
            PlayMapMusic();
            LoadCurrentLocation();
            SetupRouteGraph();
            SetupNodeViews();
            RegisterListeners();
            UpdateTopResourceHUD();
            InitCurrentLocationMarker();
            InitVolumeState();

            // Başlangıçta alt bilgi kartını gizli tut
            if (_bottomNodeInfoCard != null)
            {
                _bottomNodeInfoCard.HideCard();
            }

            // Başlangıçta mevcut konuma (örn: node_sogut) yumuşakça odaklan
            FocusOnCurrentCity(true);
        }

        private void OnDestroy()
        {
            UnregisterNodeViews();
            UnregisterListeners();
        }

        private void FindMissingReferences()
        {
            if (_panZoomController == null) _panZoomController = FindAnyObjectByType<MapPanZoomController>();
            if (_inventorySidebar == null) _inventorySidebar = FindAnyObjectByType<InventorySidebarUI>();
            if (_bottomNodeInfoCard == null) _bottomNodeInfoCard = FindAnyObjectByType<BottomNodeInfoCardUI>();
            if (_currentNodeMarker == null) _currentNodeMarker = FindAnyObjectByType<CurrentNodeMarkerUI>();
            if (_utilityDrawer == null) _utilityDrawer = FindAnyObjectByType<MenuDrawerController>();
            if (_settingsPopup == null) _settingsPopup = FindAnyObjectByType<SettingsPopupUI>();
        }

        private void LoadCurrentLocation()
        {
            if (!Services.TryGet(out _gameSession))
            {
                _gameSession = new GameSession();
                Services.Register(_gameSession);
            }

            _gameSession.SyncFromLegacyPrefsIfChanged();
            _currentNodeId = !string.IsNullOrWhiteSpace(_gameSession.CurrentNodeId)
                ? _gameSession.CurrentNodeId
                : DefaultStartNodeId;

            Debug.Log($"[WorldMapUI] Oyuncunun mevcut düğüm kimliği GameSession üzerinden yüklendi: {_currentNodeId}");
        }

        private void SetupRouteGraph()
        {
            if (_routeDatabase == null)
            {
                Debug.LogWarning("[WorldMapUI] RouteDatabaseSO referansı (_routeDatabase) atanmamış! Lütfen Inspector üzerinden RouteDatabase varlığını bağlayın.", this);
                _routeGraph = new RouteGraph((RouteDatabaseSO)null);
            }
            else
            {
                _routeGraph = new RouteGraph(_routeDatabase);
                Debug.Log($"[WorldMapUI] RouteGraph kuruldu. Kayıtlı geçerli rota sayısı: {_routeGraph.RouteCount}");
            }

            Services.Register(_routeGraph);
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

        private void SetupNodeViews()
        {
            _nodeViewsById.Clear();
            _mapNodeViews = GetComponentsInChildren<MapNodeView>(true);

            if (_mapNodeViews == null || _mapNodeViews.Length == 0)
            {
                Debug.LogError("[WorldMapUI] Sahnede hiçbir MapNodeView bileşeni bulunamadı! Harita düğümlerine MapNodeView ekleyip LocationNodeSO atayın.", this);
                return;
            }

            foreach (var nodeView in _mapNodeViews)
            {
                if (nodeView == null) continue;

                nodeView.Clicked += OnNodeViewClicked;

                if (!string.IsNullOrWhiteSpace(nodeView.NodeId))
                {
                    if (_nodeViewsById.ContainsKey(nodeView.NodeId))
                    {
                        Debug.LogWarning($"[WorldMapUI] Yinelenen NodeId tespit edildi: '{nodeView.NodeId}' ({nodeView.gameObject.name}).", nodeView);
                    }
                    _nodeViewsById[nodeView.NodeId] = nodeView;
                }
            }
        }

        private void UnregisterNodeViews()
        {
            if (_mapNodeViews == null) return;

            foreach (var nodeView in _mapNodeViews)
            {
                if (nodeView != null)
                {
                    nodeView.Clicked -= OnNodeViewClicked;
                }
            }
        }

        private void InitCurrentLocationMarker()
        {
            if (_currentNodeMarker != null && TryGetCurrentNodeRect(out RectTransform currentRect))
            {
                _currentNodeMarker.AttachToNode(currentRect);
            }
        }

        private void FocusOnCurrentCity(bool instant)
        {
            if (_panZoomController != null && TryGetCurrentNodeRect(out RectTransform currentRect))
            {
                _panZoomController.FocusOnTarget(currentRect, 1.4f, instant);
            }
        }

        private bool TryGetCurrentNodeRect(out RectTransform rect)
        {
            if (_nodeViewsById.TryGetValue(_currentNodeId, out MapNodeView currentView) && currentView != null)
            {
                rect = currentView.RectTransform;
                return rect != null;
            }

            if (_nodeViewsById.TryGetValue(DefaultStartNodeId, out MapNodeView fallbackView) && fallbackView != null)
            {
                rect = fallbackView.RectTransform;
                return rect != null;
            }

            rect = null;
            return false;
        }

        public void UpdateTopResourceHUD()
        {
            var caravan = _gameSession != null ? _gameSession.Caravan : null;

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

        private void OnNodeViewClicked(MapNodeView nodeView)
        {
            if (nodeView == null || nodeView.NodeData == null) return;

            LocationNodeSO nodeSO = nodeView.NodeData;
            string clickedNodeId = nodeSO.NodeId;
            Debug.Log($"[WorldMapUI] Düğüm seçildi: {nodeSO.NodeName} ({clickedNodeId})");

            // 1. Haritayı yumuşakça bu düğüme odakla
            if (_panZoomController != null && nodeView.RectTransform != null)
            {
                _panZoomController.FocusOnTarget(nodeView.RectTransform, 1.4f, false);
            }

            // 2. Mevcut konum kontrolü (yalnızca ASCII NodeId ile)
            bool isCurrentLocation = string.Equals(clickedNodeId, _currentNodeId, StringComparison.Ordinal);

            // 3. Rota grafiğinden doğrudan veya aktarmalı en kısa yolu sorgula
            RoutePathResult pathResult = RoutePathResult.Unreachable;
            if (!isCurrentLocation && _routeGraph != null)
            {
                _routeGraph.TryFindPath(_currentNodeId, clickedNodeId, out pathResult);
            }

            // 4. Alttan açılan detay kartını göster
            if (_bottomNodeInfoCard != null)
            {
                _bottomNodeInfoCard.ShowNode(
                    nodeSO,
                    isCurrentLocation,
                    pathResult,
                    () => OnNodeActionConfirmed(nodeSO, isCurrentLocation, pathResult)
                );
            }
        }

        private void OnNodeActionConfirmed(LocationNodeSO targetNode, bool isCurrentLocation, RoutePathResult pathResult)
        {
            if (targetNode == null) return;

            if (isCurrentLocation)
            {
                // Bulunulan merkeze giriş yap (Pazar / Divan / Han)
                Debug.Log($"[WorldMapUI] {targetNode.NodeName} ({targetNode.NodeId}) merkezine giriş yapılıyor.");
                SceneManager.LoadScene(!string.IsNullOrEmpty(_townHubSceneName) ? _townHubSceneName : "MarketScene");
            }
            else
            {
                if (!pathResult.IsReachable || !pathResult.IsDirect)
                {
                    Debug.LogWarning($"[WorldMapUI] {_currentNodeId} noktasından {targetNode.NodeId} düğümüne doğrudan rota bulunmadığı için seyahat başlatılamaz.");
                    return;
                }

                int estimatedDays = Mathf.Max(1, pathResult.TotalDays);
                string routeId = pathResult.DirectRoute != null ? pathResult.DirectRoute.RouteId : string.Empty;
                Debug.Log($"[WorldMapUI] {_currentNodeId} noktasından {targetNode.NodeName} ({targetNode.NodeId}) yönüne '{routeId}' rotasıyla ({estimatedDays} gün) seyahat başlatılıyor.");

                if (_gameSession != null)
                {
                    _gameSession.StartTravel(targetNode.NodeId, estimatedDays, routeId);
                    _currentNodeId = _gameSession.CurrentNodeId;
                }

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
            _isMuted = AudioManager.Instance != null && AudioManager.Instance.IsMuted;
            UpdateVolumeIcon();
        }

        private void OnVolumeToggleClicked()
        {
            if (AudioManager.Instance != null)
            {
                _isMuted = AudioManager.Instance.ToggleMute();
                if (!_isMuted)
                {
                    AudioManager.Instance.PlayButtonClick();
                }
            }
            else
            {
                _isMuted = !_isMuted;
                if (_localAudioSource != null)
                {
                    _localAudioSource.mute = _isMuted;
                }
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
                    _localAudioSource.mute = _isMuted;
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
