using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Kervan.UI
{
    /// <summary>
    /// Dünya Haritası üzerinde serbestçe gezinmeyi (Pan/Drag) ve
    /// iki parmakla / fare tekerleğiyle yakınlaşıp uzaklaşmayı (Pinch & Scroll Zoom) yönetir.
    /// Harita görselinin ekran sınırları dışına kaçmasını ve siyah boşlukların görünmesini
    /// dinamik sınır kilitleme (Strict Boundary Clamping) ile kesin olarak engeller.
    /// </summary>
    public class MapPanZoomController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler
    {
        private const float DefaultScreenDpi = 160f;
        private const float PinchZoomMultiplier = 0.4f;

        [Header("Hedef İçerik ve Görünüm")]
        [Tooltip("Yakınlaştırılıp kaydırılacak harita görseli ve düğümlerin bulunduğu RectTransform")]
        [SerializeField] private RectTransform _contentRect;

        [Tooltip("Haritanın içinde bulunduğu görünür pencere (Viewport). Boşsa bu objenin RectTransform'u kullanılır.")]
        [SerializeField] private RectTransform _viewportRect;

        [Header("Zoom Sınırları")]
        [Tooltip("İzin verilen taban zoom (Harita ekranı tam dolduracak dinamik minimum değerin altına asla inemez)")]
        [SerializeField] private float _minZoom = 0.85f;

        [Tooltip("En yakınlaşılmış ölçek (Detaylı şehir görünümü)")]
        [SerializeField] private float _maxZoom = 4.0f;

        [Tooltip("Oyun başladığındaki varsayılan yakınlık")]
        [SerializeField] private float _initialZoom = 2.0f;

        [Tooltip("Fare tekerleği yakınlaşma hassasiyeti")]
        [SerializeField] private float _scrollSensitivity = 0.2f;

        [Tooltip("Yumuşak hareket sönümleme hızı")]
        [SerializeField] private float _smoothSpeed = 12f;

        [Header("Başlangıç Odağı")]
        [Tooltip("Başlangıçta odaklanılacak şehir düğümü (örn: Node_Sogut). Boşsa otomatik aranır.")]
        [SerializeField] private RectTransform _initialFocusTarget;

        private float _currentZoom = 2.0f;
        private float _targetZoom = 2.0f;
        private Vector2 _targetPosition;
        private bool _isDragging = false;
        private Canvas _rootCanvas;

        // Mobil Pinch Zoom takibi
        private float _lastTouchDistance = 0f;

        private void Awake()
        {
            _rootCanvas = GetComponentInParent<Canvas>();

            if (_viewportRect == null)
            {
                _viewportRect = GetComponent<RectTransform>();
            }

            if (_contentRect == null && transform.childCount > 0)
            {
                _contentRect = transform.GetChild(0).GetComponent<RectTransform>();
            }

            EnsureMapBackgroundFit();
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Start()
        {
            Canvas.ForceUpdateCanvases();
            EnsureMapBackgroundFit();

            float minSafeZoom = CalculateMinimumSafeZoom();
            _initialZoom = Mathf.Max(_initialZoom, minSafeZoom);
            _currentZoom = _initialZoom;
            _targetZoom = _initialZoom;

            if (_contentRect != null)
            {
                _contentRect.localScale = Vector3.one * _currentZoom;
                _targetPosition = _contentRect.anchoredPosition;

                // Başlangıç düğümünü otomatik bul (Node_Sogut)
                if (_initialFocusTarget == null)
                {
                    Transform sogut = _contentRect.Find("Node_Sogut");
                    if (sogut != null)
                    {
                        _initialFocusTarget = sogut.GetComponent<RectTransform>();
                    }
                    else
                    {
                        // Alt objelerde ara
                        var allRt = _contentRect.GetComponentsInChildren<RectTransform>(true);
                        foreach (var rt in allRt)
                        {
                            if (rt.gameObject.name.ToLowerInvariant().Contains("sogut"))
                            {
                                _initialFocusTarget = rt;
                                break;
                            }
                        }
                    }
                }

                if (_initialFocusTarget != null)
                {
                    FocusOnTarget(_initialFocusTarget, _initialZoom, true);
                }
                else
                {
                    ClampPosition();
                }
            }
        }

        private void Update()
        {
            if (_contentRect == null) return;

            // Ekran boyutu değiştiğinde veya simulatörde minimum güvenli zoomu dinamik koru
            float minSafeZoom = CalculateMinimumSafeZoom();
            if (_targetZoom < minSafeZoom)
            {
                _targetZoom = minSafeZoom;
            }

            HandleMobilePinchZoom();
            UpdateSmoothMovement();
        }

        /// <summary>
        /// Harita görselinin (MapBackground) MapContent sınırlarını %100 doldurmasını garantiler.
        /// Böylece kenarlarda siyah boşluk kalması engellenir.
        /// </summary>
        private void EnsureMapBackgroundFit()
        {
            if (_contentRect == null) return;

            Transform bg = _contentRect.Find("MapBackground") ?? _contentRect.Find("Background");
            if (bg != null && bg.TryGetComponent<RectTransform>(out var bgRt))
            {
                if (bgRt.anchorMin == Vector2.zero && bgRt.anchorMax == Vector2.one)
                {
                    bgRt.sizeDelta = Vector2.zero;
                    bgRt.anchoredPosition = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// Haritanın hiçbir şekilde ekran dışına taşmayıp siyah boşluk göstermemesi için
        /// gereken en düşük güvenli zoom oranını hesaplar.
        /// </summary>
        public float CalculateMinimumSafeZoom()
        {
            if (_contentRect == null || _viewportRect == null) return _minZoom;

            Vector2 vpSize = _viewportRect.rect.size;
            if (vpSize.x <= 0f || vpSize.y <= 0f)
            {
                vpSize = new Vector2(Screen.width, Screen.height);
            }

            Vector2 contentSize = _contentRect.rect.size;
            if (contentSize.x <= 0f || contentSize.y <= 0f) return _minZoom;

            // Hem genişlikte hem yükseklikte ekranı tam doldurması için gerekli asgari ölçek
            float fitX = vpSize.x / contentSize.x;
            float fitY = vpSize.y / contentSize.y;
            float requiredMin = Mathf.Max(fitX, fitY);

            return Mathf.Max(_minZoom, requiredMin);
        }

        private void HandleMobilePinchZoom()
        {
            var activeTouches = Touch.activeTouches;
            if (activeTouches.Count == 2)
            {
                Touch touch0 = activeTouches[0];
                Touch touch1 = activeTouches[1];

                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    _lastTouchDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
                    return;
                }

                if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    float newDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
                    float deltaDistance = newDistance - _lastTouchDistance;
                    _lastTouchDistance = newDistance;

                    float dpi = Screen.dpi > 0f ? Screen.dpi : DefaultScreenDpi;
                    float zoomDelta = (deltaDistance / dpi) * PinchZoomMultiplier;
                    SetTargetZoom(_targetZoom + zoomDelta);
                }
            }
        }

        private void UpdateSmoothMovement()
        {
            if (Mathf.Abs(_currentZoom - _targetZoom) > 0.001f)
            {
                _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.unscaledDeltaTime * _smoothSpeed);
                _contentRect.localScale = Vector3.one * _currentZoom;
            }
            else
            {
                _currentZoom = _targetZoom;
                _contentRect.localScale = Vector3.one * _currentZoom;
            }

            if (!_isDragging)
            {
                _contentRect.anchoredPosition = Vector2.Lerp(_contentRect.anchoredPosition, _targetPosition, Time.unscaledDeltaTime * _smoothSpeed);
            }

            ClampPosition();
        }

        #region EventSystem Sürükleme ve Kaydırma Arayüzleri

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Touch.activeTouches.Count > 1) return;
            _isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_contentRect == null || Touch.activeTouches.Count > 1)
            {
                _isDragging = false;
                return;
            }

            float scaleFactor = _rootCanvas != null && _rootCanvas.scaleFactor > 0f ? _rootCanvas.scaleFactor : 1f;
            Vector2 delta = eventData.delta / scaleFactor;

            _contentRect.anchoredPosition += delta;
            _targetPosition = _contentRect.anchoredPosition;
            ClampPosition();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            _targetPosition = _contentRect.anchoredPosition;
            ClampPosition();
        }

        public void OnScroll(PointerEventData eventData)
        {
            float zoomChange = eventData.scrollDelta.y * _scrollSensitivity;
            SetTargetZoom(_targetZoom + zoomChange);
        }

        #endregion

        public void SetTargetZoom(float zoom)
        {
            float minSafeZoom = CalculateMinimumSafeZoom();
            _targetZoom = Mathf.Clamp(zoom, minSafeZoom, _maxZoom);
        }

        public void FocusOnTarget(RectTransform targetRect, float targetZoom = 2.0f, bool immediate = false)
        {
            if (_contentRect == null || targetRect == null) return;

            SetTargetZoom(targetZoom);

            Vector3 worldPos = targetRect.position;
            Vector2 localPos = _contentRect.InverseTransformPoint(worldPos);

            _targetPosition = GetClampedPosition(-localPos * _targetZoom, _targetZoom);

            if (immediate)
            {
                _currentZoom = _targetZoom;
                _contentRect.localScale = Vector3.one * _currentZoom;
                _contentRect.anchoredPosition = _targetPosition;
            }
        }

        /// <summary>
        /// Haritanın kenarlarının ekran içine düşüp siyah boşluk göstermesini kesin olarak engelleyen sınır hesaplayıcı.
        /// </summary>
        private Vector2 GetClampedPosition(Vector2 pos, float zoom)
        {
            if (_contentRect == null || _viewportRect == null) return pos;

            Vector2 contentSize = _contentRect.rect.size * zoom;
            Vector2 viewportSize = _viewportRect.rect.size;

            if (viewportSize.x <= 0f || viewportSize.y <= 0f)
            {
                viewportSize = new Vector2(Screen.width, Screen.height);
            }

            // Harita ekrandan büyük olduğu sürece sınırları kesin olarak kilitler
            float maxX = Mathf.Max(0f, (contentSize.x - viewportSize.x) * 0.5f);
            float minX = -maxX;

            float maxY = Mathf.Max(0f, (contentSize.y - viewportSize.y) * 0.5f);
            float minY = -maxY;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            return pos;
        }

        private void ClampPosition()
        {
            if (_contentRect == null) return;
            _targetPosition = GetClampedPosition(_targetPosition, _targetZoom);
            _contentRect.anchoredPosition = GetClampedPosition(_contentRect.anchoredPosition, _currentZoom);
        }
    }
}
