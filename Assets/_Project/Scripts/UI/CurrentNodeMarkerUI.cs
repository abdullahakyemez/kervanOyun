using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Kervan.UI
{
    /// <summary>
    /// Oyuncunun Dünya Haritasında o anda bulunduğu düğümü (Şehir veya Han)
    /// belirten, altın ışıltılı halka ve dalgalanan sancak göstergesi.
    /// </summary>
    public class CurrentNodeMarkerUI : MonoBehaviour
    {
        [Header("Görsel Bileşenler")]
        [Tooltip("Düğümün etrafında nabız atan altın ışıltı halkası")]
        [SerializeField] private Image _glowRingImage;
        [Tooltip("Düğümün üzerinde duran bayrak/sancak pini")]
        [SerializeField] private Image _bannerPinImage;
        [Tooltip("Konum pin görseli (Atanmamışsa otomatik yüklenir)")]
        [SerializeField] private Sprite _pinSprite;
        [Tooltip("Mevcut Konum etiketi")]
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Nabız / Işıltı Animasyonu")]
        [SerializeField] private float _pulseSpeed = 2.5f;
        [SerializeField] private float _minAlpha = 0.45f;
        [SerializeField] private float _maxAlpha = 0.95f;
        [SerializeField] private float _minScale = 0.96f;
        [SerializeField] private float _maxScale = 1.08f;
        [SerializeField] private float _floatDistance = 6f; // Sancağın hafif yukarı-aşağı salınımı

        private RectTransform _markerRect;
        private RectTransform _targetNodeRect;
        private Vector2 _bannerBasePos;
        private RectTransform _bannerRect;

        private void Awake()
        {
            _markerRect = GetComponent<RectTransform>();

            // Harita düğümlerine tıklamayı engellememesi için tüm görsellerin raycastTarget'ını kapat
            var allImgs = GetComponentsInChildren<Image>(true);
            foreach (var img in allImgs)
            {
                img.raycastTarget = false;
            }

            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;

            AutoBindReferences();

            if (_bannerPinImage != null)
            {
                _bannerRect = _bannerPinImage.GetComponent<RectTransform>();
                _bannerBasePos = _bannerRect.anchoredPosition;
            }
        }

        public void AutoBindReferences()
        {
            if (_glowRingImage == null)
            {
                var ring = transform.Find("GlowRing") ?? transform.Find("Ring") ?? transform.Find("Glow");
                if (ring != null) _glowRingImage = ring.GetComponent<Image>();
            }

            if (_bannerPinImage == null)
            {
                var pin = transform.Find("BannerPin") ?? transform.Find("Pin") ?? transform.Find("Icon") ?? transform.Find("MarkerPin");
                if (pin != null) _bannerPinImage = pin.GetComponent<Image>();
            }

            if (_bannerPinImage != null && _bannerPinImage.sprite == null && _pinSprite != null)
            {
                _bannerPinImage.sprite = _pinSprite;
                _bannerPinImage.preserveAspect = true;
            }
        }

        private void Update()
        {
            AnimateMarker();
        }

        /// <summary>
        /// Göstergeyi belirtilen düğüme kenetler ve görünür kılar.
        /// </summary>
        public void AttachToNode(RectTransform nodeRect)
        {
            if (nodeRect == null) return;

            _targetNodeRect = nodeRect;

            if (_markerRect != null)
            {
                // Hedef düğümün ebeveynine bağlanarak düğümün üstüne oturur
                _markerRect.SetParent(_targetNodeRect, false);
                _markerRect.anchoredPosition = Vector2.zero;
                _markerRect.localScale = Vector3.one;
            }

            gameObject.SetActive(true);
        }

        private void AnimateMarker()
        {
            float sine = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f; // 0..1

            // 1. Altın Halka Nabız ve Saydamlık
            if (_glowRingImage != null)
            {
                Color c = _glowRingImage.color;
                c.a = Mathf.Lerp(_minAlpha, _maxAlpha, sine);
                _glowRingImage.color = c;

                float scale = Mathf.Lerp(_minScale, _maxScale, sine);
                _glowRingImage.rectTransform.localScale = new Vector3(scale, scale, 1f);
            }

            // 2. Sancak Pininin Hafif Salınımı (Yukarı-aşağı süzülme)
            if (_bannerRect != null)
            {
                float floatOffset = Mathf.Sin(Time.time * (_pulseSpeed * 0.8f)) * _floatDistance;
                _bannerRect.anchoredPosition = new Vector2(_bannerBasePos.x, _bannerBasePos.y + floatOffset);
            }
        }
    }
}
