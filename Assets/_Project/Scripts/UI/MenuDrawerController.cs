using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Kervan.Audio;

namespace Kervan.UI
{
    /// <summary>
    /// Yardımcı araçlar çekmecesini (ses, ayarlar, çıkış) ok butonuna tıklandığında
    /// pürüzsüzce açıp kapatan kontrolcü.
    /// </summary>
    public class MenuDrawerController : MonoBehaviour
    {
        [Header("Çekmece Bileşenleri")]
        [Tooltip("Açma/kapatma tetikleyici butonu")]
        [SerializeField] private Button _toggleButton;

        [Tooltip("Ok ikonunu barındıran Image bileşeni")]
        [SerializeField] private Image _arrowImage;

        [Tooltip("Çekmece açıkken gösterilecek kapatma oku (Right Arrow)")]
        [SerializeField] private Sprite _arrowOpenSprite;

        [Tooltip("Çekmece kapalıyken gösterilecek açma oku (Left Arrow)")]
        [SerializeField] private Sprite _arrowClosedSprite;

        [Tooltip("İçinde butonları barındıran çekmece paneli")]
        [SerializeField] private RectTransform _drawerPanel;

        [Header("Animasyon & Başlangıç Ayarları")]
        [SerializeField] private float _animationDuration = 0.25f;
        [Tooltip("Oyun ilk açıldığında çekmece açık mı başlasın? (Varsayılan: false / kapalı)")]
        [SerializeField] private bool _startOpen = false;

        private bool _isOpen;
        private Coroutine _animCoroutine;
        private Vector2 _openPosition;
        private Vector2 _closedPosition;
        private CanvasGroup _canvasGroup;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            if (_drawerPanel != null)
            {
                _canvasGroup = _drawerPanel.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = _drawerPanel.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            if (_toggleButton != null)
            {
                _toggleButton.onClick.AddListener(ToggleDrawer);
            }

            if (_drawerPanel != null)
            {
                // Editördeki mevcut konumu açık (open) pozisyon olarak al
                _openPosition = _drawerPanel.anchoredPosition;

                // Çekmece panelinin genişliği kadar sağa kaydırarak gizleme konumu belirle
                float panelWidth = _drawerPanel.rect.width > 0 ? _drawerPanel.rect.width : 250f;
                _closedPosition = new Vector2(_openPosition.x + panelWidth, _openPosition.y);
            }

            _isOpen = _startOpen;
            ApplyStateInstant(_isOpen);
        }

        private void OnDestroy()
        {
            if (_toggleButton != null)
            {
                _toggleButton.onClick.RemoveListener(ToggleDrawer);
            }
        }

        /// <summary>
        /// Çekmecenin açık/kapalı durumunu tersine çevirir.
        /// </summary>
        public void ToggleDrawer()
        {
            if (_isOpen)
            {
                CloseDrawer();
            }
            else
            {
                OpenDrawer();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        public void OpenDrawer()
        {
            _isOpen = true;
            if (_drawerPanel != null)
            {
                _drawerPanel.gameObject.SetActive(true);
            }

            UpdateArrowIcon();
            StartTransition(_openPosition, 1f, true);
        }

        public void CloseDrawer()
        {
            _isOpen = false;
            UpdateArrowIcon();
            StartTransition(_closedPosition, 0f, false);
        }

        private void UpdateArrowIcon()
        {
            if (_arrowImage == null) return;
            // Açıkken sağ ok, kapalıyken sol ok (veya tersi)
            _arrowImage.sprite = _isOpen ? _arrowOpenSprite : _arrowClosedSprite;
        }

        private void ApplyStateInstant(bool open)
        {
            if (_drawerPanel != null)
            {
                _drawerPanel.anchoredPosition = open ? _openPosition : _closedPosition;
                _drawerPanel.gameObject.SetActive(open);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = open ? 1f : 0f;
                _canvasGroup.interactable = open;
                _canvasGroup.blocksRaycasts = open;
            }

            UpdateArrowIcon();
        }

        private void StartTransition(Vector2 targetPos, float targetAlpha, bool interactable)
        {
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(AnimateTransition(targetPos, targetAlpha, interactable));
        }

        private IEnumerator AnimateTransition(Vector2 targetPos, float targetAlpha, bool interactable)
        {
            if (_drawerPanel == null) yield break;

            Vector2 startPos = _drawerPanel.anchoredPosition;
            float startAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;
            float elapsed = 0f;

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = interactable;
            }

            while (elapsed < _animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _animationDuration);
                t = Mathf.Sin(t * Mathf.PI * 0.5f);

                _drawerPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                }

                yield return null;
            }

            _drawerPanel.anchoredPosition = targetPos;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = targetAlpha;
                _canvasGroup.interactable = interactable;
            }

            // Kapatma işlemi tamamen bittiğinde paneli deaktive et (raycast ve performans için)
            if (!interactable)
            {
                _drawerPanel.gameObject.SetActive(false);
            }

            _animCoroutine = null;
        }
    }
}
