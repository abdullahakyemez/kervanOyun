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
        [SerializeField] private Color _disabledRouteColor = new Color(0.38f, 0.35f, 0.32f, 0.85f); // Pasif / yol yok gri-kahve

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
        private string GetMarketGoodsSummary(LocationNodeSO nodeData)
        {
            if (nodeData == null)
            {
                return "Pazar: Çeşitli Emtia";
            }

            if (nodeData.NodeType == NodeType.Caravanserai)
            {
                return "Han: Dinlenme & İkmal";
            }

            return nodeData.MarketGoodsSummary;
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
        /// Seçilen düğümün <see cref="LocationNodeSO"/> ve <see cref="RoutePathResult"/> rota bilgilerini doldurur
        /// ve kartı yumuşakça yukarı kaydırarak açar.
        /// </summary>
        public void ShowNode(
            LocationNodeSO nodeData,
            bool isCurrentLocation,
            RoutePathResult pathResult,
            Action onActionCallback)
        {
            if (nodeData == null)
            {
                Debug.LogError("[BottomNodeInfoCardUI] ShowNode çağrıldı ancak LocationNodeSO verisi boş!", this);
                return;
            }

            AutoBindReferences();
            _onActionConfirmed = onActionCallback;

            string displayName = nodeData.NodeName;
            bool isHan = nodeData.NodeType == NodeType.Caravanserai;
            bool canAct = isCurrentLocation || (pathResult.IsReachable && pathResult.IsDirect);

            // 1. Başlık ve İkon
            if (_nodeTitleText != null)
            {
                _nodeTitleText.text = displayName;
                _nodeTitleText.enableAutoSizing = true;
                _nodeTitleText.fontSizeMin = 18f;
                _nodeTitleText.fontSizeMax = 28f;
            }

            if (_nodeIconImage != null)
            {
                if (nodeData.NodeIcon != null)
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
                string faction = nodeData.GetFactionDisplayName();
                _nodeTypeText.text = isHan ? $"Kervansaray & Menzil ({faction})" : $"{displayName} — {faction} Mülkü";
                ConfigureBadgeText(_nodeTypeText);
            }

            // 3, 4, 5. Mesafe, Tehlike ve Lojistik Rozetleri
            if (isCurrentLocation)
            {
                if (_nodeDistanceText != null)
                {
                    _nodeDistanceText.text = "Mevcut Konum";
                    ConfigureBadgeText(_nodeDistanceText);
                }

                if (_dangerText != null)
                {
                    _dangerText.text = "%0 (Güvenli Alan)";
                    ConfigureBadgeText(_dangerText);
                }

                if (_nodeLogisticsText != null)
                {
                    _nodeLogisticsText.text = _dangerText != null ? "Garnizon İkmali" : "İkmal Tam | Güvenli";
                    ConfigureBadgeText(_nodeLogisticsText);
                }
            }
            else if (pathResult.IsReachable && pathResult.IsDirect)
            {
                int distanceDays = Mathf.Max(1, pathResult.TotalDays);
                int foodRequired = Mathf.Max(1, pathResult.TotalFoodRequired);
                float dangerRatio = pathResult.DirectRoute != null ? Mathf.Clamp01(pathResult.DirectRoute.DangerLevel) : 0.2f;
                int dangerPercent = Mathf.RoundToInt(dangerRatio * 100f);
                string dangerLabel = GetDangerLabel(dangerRatio);

                if (_nodeDistanceText != null)
                {
                    _nodeDistanceText.text = $"{distanceDays} Gün ({distanceDays * 40} km)";
                    ConfigureBadgeText(_nodeDistanceText);
                }

                if (_dangerText != null)
                {
                    _dangerText.text = $"%{dangerPercent} ({dangerLabel})";
                    ConfigureBadgeText(_dangerText);
                }

                if (_nodeLogisticsText != null)
                {
                    _nodeLogisticsText.text = _dangerText != null
                        ? $"{foodRequired} Günlük Erzak"
                        : $"{foodRequired}G Erzak | %{dangerPercent} Risk";
                    ConfigureBadgeText(_nodeLogisticsText);
                }
            }
            else if (pathResult.IsReachable && !pathResult.IsDirect)
            {
                string hopName = pathResult.FirstHopNode != null && !string.IsNullOrWhiteSpace(pathResult.FirstHopNode.NodeName)
                    ? pathResult.FirstHopNode.NodeName
                    : pathResult.FirstHopNodeId;

                if (_nodeDistanceText != null)
                {
                    _nodeDistanceText.text = "Doğrudan yol yok";
                    ConfigureBadgeText(_nodeDistanceText);
                }

                if (_dangerText != null)
                {
                    _dangerText.text = $"{hopName} üzerinden";
                    ConfigureBadgeText(_dangerText);
                }

                if (_nodeLogisticsText != null)
                {
                    _nodeLogisticsText.text = _dangerText != null
                        ? $"Toplam {pathResult.TotalDays} Gün ({pathResult.TotalFoodRequired}G Erzak)"
                        : $"{hopName} üzerinden {pathResult.TotalDays} Gün";
                    ConfigureBadgeText(_nodeLogisticsText);
                }
            }
            else
            {
                if (_nodeDistanceText != null)
                {
                    _nodeDistanceText.text = "Bağlantı yolu yok";
                    ConfigureBadgeText(_nodeDistanceText);
                }

                if (_dangerText != null)
                {
                    _dangerText.text = "Güzergâh kapalı";
                    ConfigureBadgeText(_dangerText);
                }

                if (_nodeLogisticsText != null)
                {
                    _nodeLogisticsText.text = "Rota tanımlı değil";
                    ConfigureBadgeText(_nodeLogisticsText);
                }
            }

            // 6. Pazar & Tesis Rozeti (Facilities - Şehirde alınabilecek mallar)
            if (_nodeFacilitiesText != null)
            {
                _nodeFacilitiesText.text = GetMarketGoodsSummary(nodeData);
                ConfigureBadgeText(_nodeFacilitiesText);
            }

            // 7. Eylem Butonu
            if (_actionButton != null)
            {
                _actionButton.interactable = canAct;
            }

            if (_actionButtonText != null)
            {
                _actionButtonText.enableAutoSizing = true;
                _actionButtonText.fontSizeMin = 13f;
                _actionButtonText.fontSizeMax = 20f;

                if (isCurrentLocation)
                {
                    _actionButtonText.text = isHan ? "Menzile Gir" : "Şehre Gir";
                }
                else if (canAct)
                {
                    _actionButtonText.text = "Yola Çık";
                }
                else
                {
                    _actionButtonText.text = "Doğrudan Yol Yok";
                }
            }

            if (_actionButtonImage != null)
            {
                if (isCurrentLocation)
                {
                    _actionButtonImage.color = _enterCityColor;
                }
                else if (canAct)
                {
                    _actionButtonImage.color = _travelColor;
                }
                else
                {
                    _actionButtonImage.color = _disabledRouteColor;
                }
            }

            // Açılış Animasyonu
            SlideTo(_visiblePosY);
            _isOpen = true;
            PlayOpenSound();
        }

        private static string GetDangerLabel(float dangerRatio)
        {
            if (dangerRatio <= 0.15f) return "Güvenli Yol";
            if (dangerRatio <= 0.35f) return "Orta Risk";
            if (dangerRatio <= 0.60f) return "Tehlikeli Geçit";
            return "Yüksek Pusu Riski";
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
            if (_actionButton != null && !_actionButton.interactable) return;
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
