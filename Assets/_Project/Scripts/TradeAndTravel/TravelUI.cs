using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Kervan.Audio;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// EKRAN.md standardinda 2D Side-View Seyahat Ekraninin arayuzunu yoneten sinif.
    /// Erzak, akce, takvim, rota ilerleme cubugu ve yol olaylari pencerelerini baglar.
    /// </summary>
    public class TravelUI : MonoBehaviour
    {
        [Header("Yoneticiler")]
        [SerializeField] private TravelManager _travelManager;

        [Header("Müzik & Ses")]
        [Tooltip("Seyahat fon müziği (BGM_Travel)")]
        [SerializeField] private AudioClip _bgmTravelClip;

        [Header("Ust Bolge Gostergeleri")]
        [SerializeField] private Slider _foodSlider;
        [SerializeField] private TextMeshProUGUI _foodText;
        [SerializeField] private TextMeshProUGUI _akceText;
        [SerializeField] private TextMeshProUGUI _calendarText;
        [SerializeField] private TextMeshProUGUI _routeInfoText;

        [Header("Alt Bolge: Ilerleme & Kontroller")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _originNodeText;
        [SerializeField] private TextMeshProUGUI _destNodeText;
        [SerializeField] private Button _pauseResumeButton;
        [SerializeField] private TextMeshProUGUI _pauseResumeText;

        [Header("Olay Penceresi (Event Modal)")]
        [SerializeField] private GameObject _eventModalRoot;
        [SerializeField] private TextMeshProUGUI _eventTitleText;
        [SerializeField] private TextMeshProUGUI _eventDescText;
        [SerializeField] private Button _eventCloseButton;

        [Header("Pusu Uyarisi")]
        [SerializeField] private GameObject _ambushPanelRoot;
        [SerializeField] private Button _enterCombatButton;

        private void Start()
        {
            PlayTravelMusic();

            if (_travelManager == null)
            {
                _travelManager = FindAnyObjectByType<TravelManager>();
            }

            if (_travelManager != null)
            {
                _travelManager.OnTravelProgressUpdated += HandleProgressUpdated;
                _travelManager.OnRandomEventTriggered += HandleRandomEvent;
                _travelManager.OnAmbushTriggered += HandleAmbushTriggered;
                _travelManager.OnDestinationReached += HandleDestinationReached;
            }

            if (_pauseResumeButton != null) _pauseResumeButton.onClick.AddListener(OnTogglePauseClicked);
            if (_eventCloseButton != null) _eventCloseButton.onClick.AddListener(OnCloseEventClicked);
            if (_enterCombatButton != null) _enterCombatButton.onClick.AddListener(OnEnterCombatClicked);

            if (_eventModalRoot != null) _eventModalRoot.SetActive(false);
            if (_ambushPanelRoot != null) _ambushPanelRoot.SetActive(false);

            RefreshStaticStats();
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

        private void OnDestroy()
        {
            if (_travelManager != null)
            {
                _travelManager.OnTravelProgressUpdated -= HandleProgressUpdated;
                _travelManager.OnRandomEventTriggered -= HandleRandomEvent;
                _travelManager.OnAmbushTriggered -= HandleAmbushTriggered;
                _travelManager.OnDestinationReached -= HandleDestinationReached;
            }
        }

        private void HandleProgressUpdated(float normalizedProgress, int currentDay)
        {
            if (_progressSlider != null) _progressSlider.value = normalizedProgress;

            if (_calendarText != null)
            {
                _calendarText.text = $"{currentDay}. Gün";
            }

            RefreshStaticStats();
        }

        private void RefreshStaticStats()
        {
            if (_travelManager == null || _travelManager.Caravan == null) return;

            if (_foodText != null) _foodText.text = $"{_travelManager.Caravan.CurrentFood} Gün";
            if (_foodSlider != null)
            {
                _foodSlider.maxValue = 30;
                _foodSlider.value = _travelManager.Caravan.CurrentFood;
            }

            if (_akceText != null)
            {
                _akceText.text = $"{_travelManager.Caravan.CurrentAkce} Akçe";
            }

            if (_travelManager.ActiveRoute != null)
            {
                if (_originNodeText != null) _originNodeText.text = _travelManager.ActiveRoute.FromNode?.NodeName;
                if (_destNodeText != null) _destNodeText.text = _travelManager.ActiveRoute.ToNode?.NodeName;
                if (_routeInfoText != null) _routeInfoText.text = $"{_travelManager.ActiveRoute.Terrain} - {_travelManager.ActiveRoute.Weather}";
            }
        }

        private void HandleRandomEvent(string title, string description)
        {
            if (_eventModalRoot != null)
            {
                _eventModalRoot.SetActive(true);
            }
            if (_eventTitleText != null) _eventTitleText.text = title;
            if (_eventDescText != null) _eventDescText.text = description;
        }

        private void HandleAmbushTriggered(TravelRouteSO route)
        {
            if (_ambushPanelRoot != null)
            {
                _ambushPanelRoot.SetActive(true);
            }
        }

        private void HandleDestinationReached(LocationNodeSO arrivedNode)
        {
            HandleRandomEvent("Menzile Ulaşıldı!", $"{arrivedNode.NodeName} kapılarına varıldı. Kervan güvenle şehre/hana girdi.");
        }

        private void OnTogglePauseClicked()
        {
            if (_travelManager == null) return;

            if (_travelManager.IsTraveling)
            {
                _travelManager.PauseTravel();
                if (_pauseResumeText != null) _pauseResumeText.text = "YOLA DEVAM ET";
            }
            else
            {
                _travelManager.ResumeTravel();
                if (_pauseResumeText != null) _pauseResumeText.text = "MOLA VER";
            }
        }

        private void OnCloseEventClicked()
        {
            if (_eventModalRoot != null) _eventModalRoot.SetActive(false);
        }

        private void OnEnterCombatClicked()
        {
            Debug.Log("[TravelUI] Pusu savasi baslatiliyor -> CombatScene");
            SceneManager.LoadScene("CombatScene");
        }
    }
}
