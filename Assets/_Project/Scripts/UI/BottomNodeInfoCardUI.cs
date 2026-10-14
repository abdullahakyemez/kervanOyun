using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Audio;
using Kervan.TradeAndTravel;

namespace Kervan.UI
{
    /// <summary>
    /// Dünya Haritasında bir şehir veya hana tıklandığında ekranın altından yukarı kayarak (Slide-Up)
    /// açılan detaylı bilgi ve seyahat kartını yönetir.
    /// Oyuncu aynı şehirdeyse "Şehre Gir / Pazar", farklı şehirdeyse "Kervanı Yola Çıkar" aksiyonunu sunar.
    /// </summary>
    public class BottomNodeInfoCardUI : MonoBehaviour
    {
        [Header("Panel ve Animasyon")]
        [Tooltip("Kaydırılacak kartın RectTransform bileşeni")]
        [SerializeField] private RectTransform _cardRect;
        [Tooltip("Kart görünürkenki Y konumu (Alt kenardan mesafe)")]
        [SerializeField] private float _visiblePosY = 30f;
        [Tooltip("Kart gizliykenki Y konumu (Ekranın altına saklanma)")]
        [SerializeField] private float _hiddenPosY = -400f;
        [Tooltip("Kayma animasyonu süresi (saniye)")]
        [SerializeField] private float _slideDuration = 0.25f;

        [Header("Metin Alanları")]
        [SerializeField] private TextMeshProUGUI _nodeTitleText;
        [SerializeField] private TextMeshProUGUI _nodeTypeText;
        [SerializeField] private TextMeshProUGUI _nodeDistanceText;
        [SerializeField] private TextMeshProUGUI _nodeLogisticsText;
        [SerializeField] private TextMeshProUGUI _nodeFacilitiesText;
        [SerializeField] private TextMeshProUGUI _dangerText;

        [Header("Eylem ve Kapatma Butonları")]
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _actionButtonText;
        [SerializeField] private Image _actionButtonImage;
        [SerializeField] private Button _closeButton;

        [Header("Görsel Vurgular & İkonlar")]
        [SerializeField] private Image _nodeIconImage;
        [SerializeField] private Sprite _cityIconSprite;
        [SerializeField] private Sprite _hanIconSprite;

        [Header("Buton Renk Temaları")]
        [SerializeField] private Color _enterCityColor = new Color(0.20f, 0.55f, 0.30f); // Yeşil
        [SerializeField] private Color _travelColor = new Color(0.65f, 0.45f, 0.20f);    // Ahşap kahve / sarı

        [Header("Ses Efektleri (Opsiyonel)")]
        [Tooltip("Kart açılırken çalacak parşömen hışırtısı sesi (SFX_PaperRustle). Boş bırakılırsa AudioManager'daki PaperRustle sesi çalar.")]
        [SerializeField] private AudioClip _openSoundOverride;
        [Tooltip("Kart kapanırken çalacak özel ses. Boş bırakılırsa varsayılan tık sesi çalar.")]
        [SerializeField] private AudioClip _closeSoundOverride;

        private Coroutine _slideCoroutine;
        private Action _onActionConfirmed;
        private bool _isOpen = false;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (_cardRect == null)
            {
                _cardRect = GetComponent<RectTransform>();
            }
            AutoBindReferences();
        }

        public void AutoBindReferences()
        {
            if (_nodeTitleText == null)
            {
                var t = transform.Find("BottomTitle") ?? transform.Find("TitleText");
                if (t != null) _nodeTitleText = t.GetComponent<TextMeshProUGUI>();
                if (_nodeTitleText == null)
                {
                    var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var txt in allTexts)
                    {
                        string n = txt.gameObject.name.ToLowerInvariant();
                        if (n.Contains("title") || n.Contains("baslik"))
                        {
                            _nodeTitleText = txt;
                            break;
                        }
                    }
                }
            }

            if (_dangerText == null)
            {
                var d = transform.Find("BottomDown/BottomInfo/Danger/DangerInfo") ?? 
                        transform.Find("BottomDown/Danger/DangerInfo") ??
                        transform.Find("Danger/DangerInfo");
                if (d != null) _dangerText = d.GetComponent<TextMeshProUGUI>();
                if (_dangerText == null)
                {
                    var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var txt in allTexts)
                    {
                        if (txt.gameObject.name.ToLowerInvariant().Contains("danger"))
                        {
                            _dangerText = txt;
                            break;
                        }
                    }
                }
            }
        }

        private void ConfigureBadgeText(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 9f;
            tmp.fontSizeMax = 17f;
            tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.textWrappingMode = TextWrappingModes.Normal;
        }

        /// <summary>
        /// Seçilen düğümün (Şehir veya Han) ticaret mallarını rozet kutusu için özetler.
        /// </summary>
        private string GetMarketGoodsSummary(string nodeName, LocationNodeSO nodeData)
        {
            bool isHan = (nodeName ?? "").ToLowerInvariant().Contains("han") || 
                         (nodeName ?? "").ToLowerInvariant().Contains("oba") || 
                         (nodeData != null && nodeData.NodeType == NodeType.Caravanserai);

            if (isHan)
            {
                return "Han: Dinlenme & İkmal";
            }

            // Şehir merkezlerinde bol bulunan ve kervanla alınıp satılabilecek başlıca ticaret malları
            string n = (nodeName ?? "").ToLowerInvariant();
            if (n.Contains("söğüt") || n.Contains("sogut")) return "Pazar: Buğday, İpek";
            if (n.Contains("iznik") || n.Contains("iznık")) return "Pazar: Çini, Zeytin, İpek";
            if (n.Contains("bursa")) return "Pazar: İpek, Çelik, Kürk";
            if (n.Contains("bilecik")) return "Pazar: Dövme Demir, Kereste";
            if (n.Contains("kütahya") || n.Contains("kutahya")) return "Pazar: Çini, Bakır, Yün";
            if (n.Contains("domaniç") || n.Contains("domanic")) return "Pazar: Yün, Deri, At";
            if (n.Contains("eskişehir") || n.Contains("eskisehir")) return "Pazar: Buğday, Lületaşı";
            if (n.Contains("gemlik")) return "Pazar: Zeytinyağı, İncir";
            if (n.Contains("yalova")) return "Pazar: Kereste, Meyve";

            return "Pazar: Çeşitli Emtia";
        }

        private void Start()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(HideCard);
            }

            if (_actionButton != null)
            {
                _actionButton.onClick.AddListener(OnActionButtonClicked);
            }

            // Başlangıçta kartı gizli konumda tut
            SetPositionInstant(_hiddenPosY);
            _isOpen = false;
        }

        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveListener(HideCard);
            if (_actionButton != null) _actionButton.onClick.RemoveListener(OnActionButtonClicked);
        }

        /// <summary>
        /// Seçilen düğümün bilgilerini doldurur ve kartı yumuşakça yukarı kaydırarak açar.
        /// </summary>
        public void ShowNode(
            string nodeName, 
            LocationNodeSO nodeData, 
            bool isCurrentLocation, 
            int distanceDays, 
            int foodRequired, 
            Action onActionCallback)
        {
            AutoBindReferences();
            _onActionConfirmed = onActionCallback;
            bool isHan = nodeName.ToLower().Contains("han") || nodeName.ToLower().Contains("oba") || 
                         (nodeData != null && nodeData.NodeType == NodeType.Caravanserai);

            // 1. Başlık ve İkon
            if (_nodeTitleText != null)
            {
                _nodeTitleText.text = nodeName;
                _nodeTitleText.enableAutoSizing = true;
                _nodeTitleText.fontSizeMin = 18f;
                _nodeTitleText.fontSizeMax = 28f;
            }

            if (_nodeIconImage != null)
            {
                if (nodeData != null && nodeData.NodeIcon != null)
                {
                    _nodeIconImage.sprite = nodeData.NodeIcon;
                }
                else
                {
                    _nodeIconImage.sprite = isHan ? _hanIconSprite : _cityIconSprite;
                }
            }

            // 2. Düğüm Türü / Üst Rozet
            if (_nodeTypeText != null)
            {
                string faction = nodeData != null ? nodeData.RulingFaction.ToString() : "Osmanlı";
                _nodeTypeText.text = isHan ? "Kervansaray & Menzil" : $"{nodeName} — {faction} Mülkü";
                ConfigureBadgeText(_nodeTypeText);
            }

            // 3. Konum Rozeti (Location)
            if (_nodeDistanceText != null)
            {
                _nodeDistanceText.text = isCurrentLocation ? "Mevcut Konum" : $"{distanceDays} Gün ({distanceDays * 40} km)";
                ConfigureBadgeText(_nodeDistanceText);
            }

            // 4. Tehlike Rozeti (Danger)
            if (_dangerText != null)
            {
                _dangerText.text = isCurrentLocation ? "%0 (Güvenli Alan)" : "%15 (Güvenli Yol)";
                ConfigureBadgeText(_dangerText);
            }

            // 5. Erzak Rozeti (Logistics)
            if (_nodeLogisticsText != null)
            {
                if (_dangerText != null)
                {
                    _nodeLogisticsText.text = isCurrentLocation ? "Garnizon İkmali" : $"{foodRequired} Günlük Erzak";
                }
                else
                {
                    // Eğer sahnede DangerInfo text'i bağlı değilse ikisini kısa formatta birleştir
                    _nodeLogisticsText.text = isCurrentLocation ? "İkmal Tam | Güvenli" : $"{foodRequired}G Erzak | %15 Risk";
                }
                ConfigureBadgeText(_nodeLogisticsText);
            }

            // 6. Pazar & Tesis Rozeti (Facilities - Şehirde alınabilecek mallar)
            if (_nodeFacilitiesText != null)
            {
                _nodeFacilitiesText.text = GetMarketGoodsSummary(nodeName, nodeData);
                ConfigureBadgeText(_nodeFacilitiesText);
            }

            // 7. Eylem Butonu
            if (_actionButtonText != null)
            {
                _actionButtonText.enableAutoSizing = true;
                _actionButtonText.fontSizeMin = 13f;
                _actionButtonText.fontSizeMax = 20f;
                if (isCurrentLocation)
                {
                    _actionButtonText.text = isHan ? "Menzile Gir ➔" : "Şehre Gir ➔";
                }
                else
                {
                    _actionButtonText.text = "Yola Çık ➔";
                }
            }

            if (_actionButtonImage != null)
            {
                _actionButtonImage.color = isCurrentLocation ? _enterCityColor : _travelColor;
            }

            // Açılış Animasyonu
            SlideTo(_visiblePosY);
            _isOpen = true;
            PlayOpenSound();
        }

        /// <summary>
        /// Bilgi kartını aşağı kaydırarak gizler.
        /// </summary>
        public void HideCard()
        {
            if (!_isOpen) return;

            SlideTo(_hiddenPosY);
            _isOpen = false;
            PlayCloseSound();
        }

        private void OnActionButtonClicked()
        {
            PlayButtonClickSound();
            _onActionConfirmed?.Invoke();
        }

        private void SlideTo(float targetY)
        {
            if (_cardRect == null) return;

            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
            }

            _slideCoroutine = StartCoroutine(SlideRoutine(targetY));
        }

        private IEnumerator SlideRoutine(float targetY)
        {
            float startY = _cardRect.anchoredPosition.y;
            float elapsed = 0f;

            while (elapsed < _slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _slideDuration);
                _cardRect.anchoredPosition = new Vector2(_cardRect.anchoredPosition.x, Mathf.Lerp(startY, targetY, t));
                yield return null;
            }

            _cardRect.anchoredPosition = new Vector2(_cardRect.anchoredPosition.x, targetY);
            _slideCoroutine = null;
        }

        private void SetPositionInstant(float y)
        {
            if (_cardRect != null)
            {
                _cardRect.anchoredPosition = new Vector2(_cardRect.anchoredPosition.x, y);
            }
        }

        private void PlayOpenSound()
        {
            if (_openSoundOverride != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(_openSoundOverride, 0.85f);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(_openSoundOverride, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.85f);
                }
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPaperRustle();
            }
            else
            {
                AudioClip fallbackClip = Resources.Load<AudioClip>("Audio/SFX/SFX_PaperRustle");
                if (fallbackClip != null)
                {
                    AudioSource.PlayClipAtPoint(fallbackClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.85f);
                }
            }
        }

        private void PlayCloseSound()
        {
            if (_closeSoundOverride != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(_closeSoundOverride, 0.7f);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(_closeSoundOverride, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.7f);
                }
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        private void PlayButtonClickSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}
