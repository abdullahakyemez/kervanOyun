using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Kervan.Core;
using Kervan.Audio;
using Kervan.UI;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// 2D Side-View Seyahat Ekranının (TravelScene) arayüzünü yöneten görünüm sınıfı.
    /// İlerleme çubuğu, çıkış/varış şehirleri, 1x/2x tempo, Mola Ver kontrolü ve
    /// <see cref="TravelParallaxUI"/> / <see cref="CaravanConvoyUI"/> görsel akış senkronizasyonunu sağlar.
    /// </summary>
    public class TravelUI : MonoBehaviour
    {
        private const float NormalSpeed = 1f;
        private const float FastSpeed = 2f;
        private const float AutoReturnDelaySeconds = 1.2f;

        [Header("Yöneticiler & Görsel Akış")]
        [Tooltip("Seyahat mantığını yürüten TravelManager bileşeni")]
        [SerializeField] private TravelManager _travelManager;

        [Tooltip("Arka plan katmanlarını kaydıran TravelParallaxUI bileşeni")]
        [SerializeField] private TravelParallaxUI _parallaxUI;

        [Tooltip("Kervan birimlerinin yürüyüş salınımını yöneten CaravanConvoyUI bileşeni")]
        [SerializeField] private CaravanConvoyUI _convoyUI;

        [Header("Müzik & Ses")]
        [Tooltip("Seyahat fon müziği (BGM_Travel)")]
        [SerializeField] private AudioClip _bgmTravelClip;

        [Header("Üst Bölge (Opsiyonel — TopBar_HUD Prefabı Varsa Boş Bırakılabilir)")]
        [SerializeField] private Slider _foodSlider;
        [SerializeField] private TextMeshProUGUI _foodText;
        [SerializeField] private TextMeshProUGUI _akceText;
        [SerializeField] private TextMeshProUGUI _calendarText;
        [SerializeField] private TextMeshProUGUI _routeInfoText;

        [Header("Alt Bölge: İlerleme & Kontroller")]
        [Tooltip("0..1 arası seyahat ilerlemesini gösteren Slider")]
        [SerializeField] private Slider _progressSlider;

        [Tooltip("Slider yerine Filled Image kullanılıyorsa (opsiyonel)")]
        [SerializeField] private Image _progressFillImage;

        [Tooltip("Çıkış şehri adı (ör. Söğüt)")]
        [SerializeField] private TextMeshProUGUI _originNodeText;

        [Tooltip("Varış şehri adı (ör. İznik)")]
        [SerializeField] private TextMeshProUGUI _destNodeText;

        [Tooltip("Mola Ver / Yola Devam Et butonu")]
        [SerializeField] private Button _pauseResumeButton;

        [Tooltip("Mola butonunun üzerindeki metin")]
        [SerializeField] private TextMeshProUGUI _pauseResumeText;

        [Tooltip("1x / 2x yürüyüş temposu değiştirme butonu")]
        [SerializeField] private Button _speedToggleButton;

        [Tooltip("Tempo butonunun üzerindeki metin (1x / 2x)")]
        [SerializeField] private TextMeshProUGUI _speedToggleText;

        [Header("Olay & Varış Penceresi (EventModal)")]
        [Tooltip("Olay ve varış bildirim penceresinin kök GameObject'i")]
        [SerializeField] private GameObject _eventModalRoot;

        [Tooltip("Olay penceresi başlık metni")]
        [SerializeField] private TextMeshProUGUI _eventTitleText;

        [Tooltip("Olay penceresi hikâye/açıklama metni")]
        [SerializeField] private TextMeshProUGUI _eventDescText;

        [Tooltip("Olayın kervana etkisini gösteren özet metin (Boş bırakılırsa açıklama metninin altına eklenir)")]
        [SerializeField] private TextMeshProUGUI _eventOutcomeText;

        [Tooltip("Olay penceresindeki opsiyonel ikon görseli")]
        [SerializeField] private Image _eventIconImage;

        [Tooltip("Olay penceresini kapatıp yola devam ettiren / haritaya döndüren buton")]
        [SerializeField] private Button _eventCloseButton;

        [Tooltip("Kapatma butonunun üzerindeki metin ('Yola Devam Et' / 'Haritaya Dön')")]
        [SerializeField] private TextMeshProUGUI _eventCloseButtonText;

        [Header("Pusu Uyarısı (A5 Hazırlık Modalı)")]
        [SerializeField] private GameObject _ambushPanelRoot;
        [SerializeField] private Button _enterCombatButton;

        private bool _hasArrived;
        private bool _isFastForward;

        private void Reset()
        {
            _travelManager = FindAnyObjectByType<TravelManager>();
            _parallaxUI = FindAnyObjectByType<TravelParallaxUI>();
            _convoyUI = FindAnyObjectByType<CaravanConvoyUI>();
        }

        private void Awake()
        {
            if (_travelManager == null)
            {
                _travelManager = FindAnyObjectByType<TravelManager>();
            }

            if (_parallaxUI == null)
            {
                _parallaxUI = FindAnyObjectByType<TravelParallaxUI>();
            }

            if (_convoyUI == null)
            {
                _convoyUI = FindAnyObjectByType<CaravanConvoyUI>();
            }

            if (_eventCloseButtonText == null && _eventCloseButton != null)
            {
                _eventCloseButtonText = _eventCloseButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        private void Start()
        {
            PlayTravelMusic();

            if (_travelManager != null)
            {
                _travelManager.OnTravelProgressUpdated += HandleProgressUpdated;
                _travelManager.OnRoadEventTriggered += HandleRoadEventTriggered;
                _travelManager.OnAmbushTriggered += HandleAmbushTriggered;
                _travelManager.OnDestinationReached += HandleDestinationReached;
            }

            if (_pauseResumeButton != null)
            {
                _pauseResumeButton.onClick.AddListener(OnTogglePauseClicked);
            }

            if (_speedToggleButton != null)
            {
                _speedToggleButton.onClick.AddListener(OnToggleSpeedClicked);
            }

            if (_eventCloseButton != null)
            {
                _eventCloseButton.onClick.AddListener(OnCloseEventClicked);
            }

            if (_enterCombatButton != null)
            {
                _enterCombatButton.onClick.AddListener(OnEnterCombatClicked);
            }

            if (_eventModalRoot != null)
            {
                _eventModalRoot.SetActive(false);
            }

            if (_ambushPanelRoot != null)
            {
                _ambushPanelRoot.SetActive(false);
            }

            if (_progressSlider != null)
            {
                _progressSlider.wholeNumbers = false;
                _progressSlider.minValue = 0f;
                _progressSlider.maxValue = 1f;
                _progressSlider.value = 0f;
            }

            ApplySpeedState(false);
            SyncVisualFlowWithTravelState();
            RefreshStaticStats();
        }

        private void OnDestroy()
        {
            if (_travelManager != null)
            {
                _travelManager.OnTravelProgressUpdated -= HandleProgressUpdated;
                _travelManager.OnRoadEventTriggered -= HandleRoadEventTriggered;
                _travelManager.OnAmbushTriggered -= HandleAmbushTriggered;
                _travelManager.OnDestinationReached -= HandleDestinationReached;
            }

            if (_pauseResumeButton != null)
            {
                _pauseResumeButton.onClick.RemoveListener(OnTogglePauseClicked);
            }

            if (_speedToggleButton != null)
            {
                _speedToggleButton.onClick.RemoveListener(OnToggleSpeedClicked);
            }

            if (_eventCloseButton != null)
            {
                _eventCloseButton.onClick.RemoveListener(OnCloseEventClicked);
            }

            if (_enterCombatButton != null)
            {
                _enterCombatButton.onClick.RemoveListener(OnEnterCombatClicked);
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleBackNavigation();
            }
        }

        private void HandleBackNavigation()
        {
            if (_eventModalRoot != null && _eventModalRoot.activeSelf)
            {
                OnCloseEventClicked();
                return;
            }

            if (_travelManager != null && !_hasArrived)
            {
                OnTogglePauseClicked();
            }
        }

        private void PlayTravelMusic()
        {
            AudioClip clip = _bgmTravelClip;
            if (clip == null && AudioManager.Instance != null && AudioManager.Instance.Library != null)
            {
                clip = AudioManager.Instance.Library.BgmTravel;
            }

            if (clip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(clip, 1.0f);
            }
        }

        private void HandleProgressUpdated(float normalizedProgress, int currentDay)
        {
            if (_progressSlider != null)
            {
                _progressSlider.value = normalizedProgress;
            }

            if (_progressFillImage != null)
            {
                _progressFillImage.fillAmount = normalizedProgress;
            }

            if (_calendarText != null)
            {
                _calendarText.text = GameCalendar.GetFormattedDate(currentDay);
            }

            RefreshStaticStats();
        }

        private void RefreshStaticStats()
        {
            if (_travelManager == null || _travelManager.Caravan == null)
            {
                return;
            }

            if (_foodText != null)
            {
                _foodText.text = $"{_travelManager.Caravan.CurrentFood} Gün";
            }

            if (_foodSlider != null)
            {
                _foodSlider.maxValue = 30f;
                _foodSlider.value = _travelManager.Caravan.CurrentFood;
            }

            if (_akceText != null)
            {
                _akceText.text = $"{_travelManager.Caravan.CurrentAkce:N0}";
            }

            LocationNodeSO origin = _travelManager.OriginNode;
            LocationNodeSO dest = _travelManager.DestinationNode;

            if (_originNodeText != null && origin != null)
            {
                _originNodeText.text = origin.NodeName;
            }

            if (_destNodeText != null && dest != null)
            {
                _destNodeText.text = dest.NodeName;
            }

            if (_routeInfoText != null && _travelManager.ActiveRoute != null)
            {
                string terrainLabel = GetTerrainDisplayName(_travelManager.ActiveRoute.Terrain);
                string weatherLabel = GetWeatherDisplayName(_travelManager.ActiveRoute.Weather);
                _routeInfoText.text = $"{_travelManager.CurrentRouteDay}. Gün / {_travelManager.TotalRouteDays} Gün · {terrainLabel} · {weatherLabel}";
            }
        }

        private void HandleRoadEventTriggered(RoadEventSO roadEvent, string outcomeSummary)
        {
            if (roadEvent == null)
            {
                return;
            }

            string hexColor = GetOutcomeHexColor(roadEvent);
            ShowEventModal(
                roadEvent.EventTitle,
                roadEvent.EventDescription,
                outcomeSummary,
                hexColor,
                roadEvent.EventIcon,
                isArrivalModal: false);
        }

        private void ShowEventModal(
            string title,
            string description,
            string outcomeSummary,
            string outcomeHexColor,
            Sprite eventIcon,
            bool isArrivalModal)
        {
            if (_eventModalRoot == null)
            {
                return;
            }

            if (_travelManager != null && !_hasArrived)
            {
                _travelManager.PauseTravel();
                SyncVisualFlowWithTravelState();
            }

            _eventModalRoot.SetActive(true);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayParchment();
            }

            if (_eventTitleText != null)
            {
                _eventTitleText.text = title;
            }

            if (_eventOutcomeText != null)
            {
                if (_eventDescText != null)
                {
                    _eventDescText.text = description;
                }

                _eventOutcomeText.gameObject.SetActive(!string.IsNullOrEmpty(outcomeSummary));
                _eventOutcomeText.text = !string.IsNullOrEmpty(outcomeSummary)
                    ? $"<color={outcomeHexColor}>{outcomeSummary}</color>"
                    : string.Empty;
            }
            else if (_eventDescText != null)
            {
                _eventDescText.text = !string.IsNullOrEmpty(outcomeSummary)
                    ? $"{description}\n\n<color={outcomeHexColor}><b>[{outcomeSummary}]</b></color>"
                    : description;
            }

            if (_eventIconImage != null)
            {
                bool hasIcon = eventIcon != null;
                _eventIconImage.gameObject.SetActive(hasIcon);
                if (hasIcon)
                {
                    _eventIconImage.sprite = eventIcon;
                }
            }

            if (_eventCloseButtonText != null)
            {
                _eventCloseButtonText.text = isArrivalModal ? "Haritaya Dön" : "Yola Devam Et";
            }

            RefreshStaticStats();
        }

        private static string GetOutcomeHexColor(RoadEventSO roadEvent)
        {
            if (roadEvent == null)
            {
                return "#D9B359"; // AntiqueGold
            }

            if (roadEvent.Category == RoadEventCategory.Positive || roadEvent.AkceDelta > 0 || roadEvent.FoodDelta > 0 || roadEvent.HealDelta > 0)
            {
                return "#2E7D32"; // MoraleHigh (Yeşil)
            }

            if (roadEvent.Category == RoadEventCategory.BanditAmbush ||
                roadEvent.Category == RoadEventCategory.WildBeastAttack ||
                roadEvent.AkceDelta < 0 ||
                roadEvent.FoodDelta < 0 ||
                roadEvent.HealDelta < 0)
            {
                return "#D9534F"; // Okunaklı Kırmızı / Tehlike
            }

            return "#D9B359"; // AntiqueGold
        }

        private void HandleAmbushTriggered(TravelRouteSO route)
        {
            SyncVisualFlowWithTravelState();

            if (_ambushPanelRoot != null)
            {
                _ambushPanelRoot.SetActive(true);
            }
            else if (_eventModalRoot == null && _travelManager != null)
            {
                // Ne AmbushPanel ne de EventModal sahnede kurulmadıysa yolculuğun kilitlenmemesi için devam ettir
                _travelManager.ResumeTravel();
                SyncVisualFlowWithTravelState();
            }
        }

        private void HandleDestinationReached(LocationNodeSO arrivedNode)
        {
            _hasArrived = true;
            SetVisualFlowPaused(true);

            if (_progressSlider != null)
            {
                _progressSlider.value = 1f;
            }

            if (_progressFillImage != null)
            {
                _progressFillImage.fillAmount = 1f;
            }

            string cityName = arrivedNode != null ? arrivedNode.NodeName : "Hedef Menzil";
            if (_routeInfoText != null)
            {
                _routeInfoText.text = $"{cityName} kapılarına ulaşıldı!";
            }

            if (_eventModalRoot != null)
            {
                ShowEventModal(
                    "Menzile Ulaşıldı!",
                    $"{cityName} kapılarına varıldı. Kervan yükü ve muhafızlarıyla güvenle menzile girdi.",
                    $"Konum güncellendi: {cityName}",
                    "#D9B359",
                    arrivedNode != null ? arrivedNode.NodeIcon : null,
                    isArrivalModal: true);
            }
            else
            {
                StartCoroutine(ReturnToWorldMapAfterDelay(AutoReturnDelaySeconds));
            }
        }

        private IEnumerator ReturnToWorldMapAfterDelay(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            SceneLoader.Load(SceneIds.WorldMap);
        }

        private void OnTogglePauseClicked()
        {
            if (_travelManager == null || _hasArrived)
            {
                return;
            }

            if (_travelManager.IsTraveling)
            {
                _travelManager.PauseTravel();
            }
            else
            {
                _travelManager.ResumeTravel();
            }

            SyncVisualFlowWithTravelState();
        }

        private void OnToggleSpeedClicked()
        {
            ApplySpeedState(!_isFastForward);
        }

        private void ApplySpeedState(bool fastForward)
        {
            _isFastForward = fastForward;
            float multiplier = _isFastForward ? FastSpeed : NormalSpeed;

            if (_travelManager != null)
            {
                _travelManager.SetSpeedMultiplier(multiplier);
            }

            if (_parallaxUI != null)
            {
                _parallaxUI.SetSpeedMultiplier(multiplier);
            }

            if (_convoyUI != null && _parallaxUI == null)
            {
                _convoyUI.SetSpeedMultiplier(multiplier);
            }

            if (_speedToggleText != null)
            {
                _speedToggleText.text = _isFastForward ? "2x Tempo" : "1x Tempo";
            }
        }

        private void SyncVisualFlowWithTravelState()
        {
            bool isPaused = _travelManager == null || !_travelManager.IsTraveling;
            SetVisualFlowPaused(isPaused);

            if (_pauseResumeText != null)
            {
                _pauseResumeText.text = isPaused ? "Yola Devam Et" : "Mola Ver";
            }
        }

        private void SetVisualFlowPaused(bool paused)
        {
            if (_parallaxUI != null)
            {
                _parallaxUI.SetPaused(paused);
            }

            if (_convoyUI != null)
            {
                _convoyUI.SetPaused(paused);
            }
        }

        private void OnCloseEventClicked()
        {
            if (_eventModalRoot != null)
            {
                _eventModalRoot.SetActive(false);
            }

            if (_hasArrived)
            {
                SceneLoader.Load(SceneIds.WorldMap);
                return;
            }

            if (_travelManager != null && !_travelManager.IsTraveling)
            {
                _travelManager.ResumeTravel();
                SyncVisualFlowWithTravelState();
            }
        }

        private void OnEnterCombatClicked()
        {
            Debug.Log("[TravelUI] Pusu savaşı başlatılıyor -> CombatScene");
            SceneLoader.Load(SceneIds.Combat);
        }

        private static string GetTerrainDisplayName(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Steppe: return "Bozkır";
                case TerrainType.Forest: return "Orman Yolu";
                case TerrainType.MountainPass: return "Dağ Geçidi";
                case TerrainType.Swamp: return "Bataklık İz";
                default: return "Kervan Yolu";
            }
        }

        private static string GetWeatherDisplayName(WeatherCondition weather)
        {
            switch (weather)
            {
                case WeatherCondition.Clear: return "Açık Hava";
                case WeatherCondition.HeavyRain: return "Sağanak Yağış";
                case WeatherCondition.ExtremeHeat: return "Kavurucu Sıcak";
                case WeatherCondition.SnowStorm: return "Kar Fırtınası";
                default: return "Açık Hava";
            }
        }
    }
}
