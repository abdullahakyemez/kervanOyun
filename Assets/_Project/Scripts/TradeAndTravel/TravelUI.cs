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

        [Header("Pusu & Karşılaşma Karar Kümesi (A5.1)")]
        [Tooltip("Karar gerektiren pusularda açılan 3'lü buton kümesi konteyneri")]
        [SerializeField] private GameObject _choiceContainer;

        [Tooltip("Kaçış denemesi butonu")]
        [SerializeField] private Button _btnChoiceFlee;

        [Tooltip("Kaçış butonu altındaki açıklama / şans metni")]
        [SerializeField] private TextMeshProUGUI _btnChoiceFleeText;

        [Tooltip("Haraç verme butonu")]
        [SerializeField] private Button _btnChoiceTribute;

        [Tooltip("Haraç butonu altındaki bedel metni")]
        [SerializeField] private TextMeshProUGUI _btnChoiceTributeText;

        [Tooltip("Savaş başlatma butonu")]
        [SerializeField] private Button _btnChoiceFight;

        [Tooltip("Savaş butonu üzerindeki metin")]
        [SerializeField] private TextMeshProUGUI _btnChoiceFightText;

        [Header("Pusu Uyarısı (A5 Hazırlık Modalı)")]
        [SerializeField] private GameObject _ambushPanelRoot;
        [SerializeField] private Button _enterCombatButton;

        private bool _hasArrived;
        private bool _isFastForward;
        private bool _mustEnterCombat;

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

            if (_btnChoiceFlee != null)
            {
                _btnChoiceFlee.onClick.AddListener(OnChoiceFleeClicked);
            }

            if (_btnChoiceTribute != null)
            {
                _btnChoiceTribute.onClick.AddListener(OnChoiceTributeClicked);
            }

            if (_btnChoiceFight != null)
            {
                _btnChoiceFight.onClick.AddListener(OnChoiceFightClicked);
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

            if (_btnChoiceFlee != null)
            {
                _btnChoiceFlee.onClick.RemoveListener(OnChoiceFleeClicked);
            }

            if (_btnChoiceTribute != null)
            {
                _btnChoiceTribute.onClick.RemoveListener(OnChoiceTributeClicked);
            }

            if (_btnChoiceFight != null)
            {
                _btnChoiceFight.onClick.RemoveListener(OnChoiceFightClicked);
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
                isArrivalModal: false,
                roadEvent: roadEvent);
        }

        private void ShowEventModal(
            string title,
            string description,
            string outcomeSummary,
            string outcomeHexColor,
            Sprite eventIcon,
            bool isArrivalModal,
            RoadEventSO roadEvent = null)
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
            _mustEnterCombat = false;

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

            bool isCombatChoice = roadEvent != null && roadEvent.RequiresCombatChoice;
            if (isCombatChoice && _choiceContainer != null)
            {
                _choiceContainer.SetActive(true);
                if (_eventCloseButton != null)
                {
                    _eventCloseButton.gameObject.SetActive(false);
                }

                // 1. Kaçış Butonu
                if (_btnChoiceFlee != null)
                {
                    _btnChoiceFlee.interactable = true;
                    if (_btnChoiceFleeText != null)
                    {
                        int chancePercent = Mathf.RoundToInt(roadEvent.FleeChance * 100f);
                        _btnChoiceFleeText.text = $"Kaç\n<size=18>(%{chancePercent} Şans)</size>";
                    }
                }

                // 2. Haraç Butonu
                if (_btnChoiceTribute != null)
                {
                    if (roadEvent.Category == RoadEventCategory.WildBeastAttack)
                    {
                        _btnChoiceTribute.interactable = false;
                        if (_btnChoiceTributeText != null)
                        {
                            _btnChoiceTributeText.text = "Haraç\n<size=18>(Geçersiz)</size>";
                        }
                    }
                    else
                    {
                        int cost = roadEvent.TributeAkceCost > 0 ? roadEvent.TributeAkceCost : 30;
                        int currentAkce = _travelManager != null && _travelManager.Caravan != null ? _travelManager.Caravan.CurrentAkce : 0;
                        bool canAfford = currentAkce >= cost;
                        _btnChoiceTribute.interactable = canAfford;
                        if (_btnChoiceTributeText != null)
                        {
                            _btnChoiceTributeText.text = canAfford
                                ? $"Haraç Ver\n<size=18>({cost} Akçe)</size>"
                                : $"Haraç Ver\n<size=18>(Yetersiz: {cost})</size>";
                        }
                    }
                }

                // 3. Savaş Butonu
                if (_btnChoiceFight != null)
                {
                    _btnChoiceFight.interactable = true;
                    if (_btnChoiceFightText != null)
                    {
                        _btnChoiceFightText.text = "Savaş!\n<size=18>(Kılıç Çek)</size>";
                    }
                }
            }
            else
            {
                if (_choiceContainer != null)
                {
                    _choiceContainer.SetActive(false);
                }

                if (_eventCloseButton != null)
                {
                    _eventCloseButton.gameObject.SetActive(true);
                }

                if (_eventCloseButtonText != null)
                {
                    _eventCloseButtonText.text = isArrivalModal ? "Haritaya Dön" : "Yola Devam Et";
                }
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

            if (_mustEnterCombat)
            {
                _mustEnterCombat = false;
                Debug.Log("[TravelUI] Kaçış başarısız oldu, savaşa geçiliyor -> CombatScene");
                SceneLoader.Load(SceneIds.Combat);
                return;
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

        private void OnChoiceFleeClicked()
        {
            if (_travelManager == null)
            {
                return;
            }

            AmbushResolutionResult result = _travelManager.ResolveAmbushFlee();

            if (_eventDescText != null)
            {
                _eventDescText.text = result.Message;
            }

            if (_eventOutcomeText != null)
            {
                _eventOutcomeText.gameObject.SetActive(true);
                string colorHex = result.Success ? "#2E7D32" : "#D9534F";
                _eventOutcomeText.text = $"<color={colorHex}><b>[{result.OutcomeSummary}]</b></color>";
            }

            if (_choiceContainer != null)
            {
                _choiceContainer.SetActive(false);
            }

            if (_eventCloseButton != null)
            {
                _eventCloseButton.gameObject.SetActive(true);
            }

            if (result.Success)
            {
                _mustEnterCombat = false;
                if (_eventCloseButtonText != null)
                {
                    _eventCloseButtonText.text = "Yola Devam Et";
                }
            }
            else
            {
                _mustEnterCombat = true;
                if (_eventCloseButtonText != null)
                {
                    _eventCloseButtonText.text = "Savaşa Gir!";
                }
            }

            RefreshStaticStats();
        }

        private void OnChoiceTributeClicked()
        {
            if (_travelManager == null)
            {
                return;
            }

            AmbushResolutionResult result = _travelManager.ResolveAmbushTribute();

            if (result.Success)
            {
                if (_eventDescText != null)
                {
                    _eventDescText.text = result.Message;
                }

                if (_eventOutcomeText != null)
                {
                    _eventOutcomeText.gameObject.SetActive(true);
                    _eventOutcomeText.text = $"<color=#DAA520><b>[{result.OutcomeSummary}]</b></color>";
                }

                if (_choiceContainer != null)
                {
                    _choiceContainer.SetActive(false);
                }

                if (_eventCloseButton != null)
                {
                    _eventCloseButton.gameObject.SetActive(true);
                }

                _mustEnterCombat = false;
                if (_eventCloseButtonText != null)
                {
                    _eventCloseButtonText.text = "Yola Devam Et";
                }

                RefreshStaticStats();
            }
            else
            {
                if (_eventOutcomeText != null)
                {
                    _eventOutcomeText.gameObject.SetActive(true);
                    _eventOutcomeText.text = $"<color=#D9534F><b>[{result.Message}]</b></color>";
                }
            }
        }

        private void OnChoiceFightClicked()
        {
            if (_travelManager != null)
            {
                _travelManager.ResolveAmbushFight();
            }

            Debug.Log("[TravelUI] Pusu savaşı seçildi -> CombatScene");
            SceneLoader.Load(SceneIds.Combat);
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
