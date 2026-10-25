using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Kervan.Core;

namespace Kervan.UI
{
    /// <summary>
    /// Sahneler arası yumuşak kararma (Fade In / Fade Out) geçişlerini yöneten kalıcı UI bileşeni.
    /// Ekranın en üstünde yer alan bağımsız bir CanvasGroup ve siyah Image üzerinden çalışır.
    /// Kalıcı servis olarak Services havuzuna kaydolur ve sahne geçişlerinde çift dokunmayı engeller.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [Header("Kararma Bileşenleri")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _fadeImage;

        [Header("Açılış Ayarları")]
        [Tooltip("Sahne ilk açıldığında otomatik olarak aydınlanma (Fade In) yapılsın mı?")]
        [SerializeField] private bool _fadeInOnStart = true;
        [Tooltip("Açılış kararma süresi (saniye)")]
        [SerializeField] private float _defaultFadeDuration = 0.5f;

        private Coroutine _fadeCoroutine;

        public bool IsFading => _fadeCoroutine != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        /// <summary>
        /// Sahne veya Boot hiyerarşisinde fader yoksa dahi çalışma anında bağımsız,
        /// kalıcı (DontDestroyOnLoad) bir ScreenFader objesi üreterek hazırlar.
        /// </summary>
        public static ScreenFader EnsureInstance()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var existing = FindAnyObjectByType<ScreenFader>();
            if (existing != null)
            {
                Instance = existing;
                if (existing.transform.parent == null)
                {
                    DontDestroyOnLoad(existing.gameObject);
                }

                if (!Services.IsRegistered<ScreenFader>())
                {
                    Services.Register(existing);
                }

                return existing;
            }

            // Sahne veya prefab'da yoksa dinamik olarak kalıcı fader oluşturulur
            var rootGo = new GameObject("[ScreenFader]");
            DontDestroyOnLoad(rootGo);

            var canvas = rootGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767; // Tüm arayüz katmanlarının en üstünde görünür

            var scaler = rootGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            rootGo.AddComponent<GraphicRaycaster>();

            var canvasGroup = rootGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            var imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(rootGo.transform, false);

            var rect = imageGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = imageGo.AddComponent<Image>();
            image.color = Color.black;

            var fader = rootGo.AddComponent<ScreenFader>();
            fader._canvasGroup = canvasGroup;
            fader._fadeImage = image;

            Instance = fader;

            if (!Services.IsRegistered<ScreenFader>())
            {
                Services.Register(fader);
            }

            return fader;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Kalıcı ScreenFader zaten mevcutsa sahne içi geçici fader kendini devre dışı bırakır
                enabled = false;
                gameObject.SetActive(false);
                return;
            }

            Instance = this;

            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_fadeImage == null)
            {
                _fadeImage = GetComponentInChildren<Image>();
            }

            if (!Services.IsRegistered<ScreenFader>())
            {
                Services.Register(this);
            }
        }

        private void Start()
        {
            if (_fadeInOnStart && _canvasGroup != null && _canvasGroup.alpha > 0.05f)
            {
                FadeIn(_defaultFadeDuration);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Kararma opaklığını doğrudan belirler.
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = Mathf.Clamp01(alpha);
            _canvasGroup.blocksRaycasts = _canvasGroup.alpha > 0.01f;
        }

        /// <summary>
        /// Ekranı siyahtan aydınlığa geçirir (Fade In).
        /// </summary>
        public void FadeIn(float duration, Action onComplete = null)
        {
            StartFade(0f, duration, onComplete, false);
        }

        /// <summary>
        /// Ekranı aydınlıktan siyaha karartır (Fade Out).
        /// </summary>
        public void FadeOut(float duration, Action onComplete = null)
        {
            StartFade(1f, duration, onComplete, true);
        }

        /// <summary>
        /// SceneLoader için Coroutine formatında karartma (Fade Out).
        /// </summary>
        public IEnumerator FadeOutRoutine(float duration)
        {
            yield return FadeRoutine(1f, duration, null, true);
        }

        /// <summary>
        /// SceneLoader için Coroutine formatında aydınlatma (Fade In).
        /// </summary>
        public IEnumerator FadeInRoutine(float duration)
        {
            yield return FadeRoutine(0f, duration, null, false);
        }

        /// <summary>
        /// Geriye dönük uyumluluk: Ekranı karartır ve hedef sahneyi SceneLoader üzerinden yükler.
        /// </summary>
        public void FadeToScene(string sceneName, float duration = 0.5f)
        {
            SceneLoader.Load(sceneName, duration);
        }

        private void StartFade(float targetAlpha, float duration, Action onComplete, bool blockRaycasts)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration, onComplete, blockRaycasts));
        }

        /// <summary>
        /// Kararma/aydınlanma animasyonunun ana coroutine döngüsü.
        /// </summary>
        public IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete, bool blockRaycasts)
        {
            if (_canvasGroup == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            _canvasGroup.blocksRaycasts = true;
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.01f, duration);

            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / safeDuration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _canvasGroup.blocksRaycasts = blockRaycasts;

            _fadeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
