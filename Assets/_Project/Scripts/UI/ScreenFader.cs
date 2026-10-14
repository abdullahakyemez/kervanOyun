using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Kervan.UI
{
    /// <summary>
    /// Sahneler arası yumuşak kararma (Fade In / Fade Out) geçişlerini yönetir.
    /// Ekranın en üstünde yer alan bir CanvasGroup ve siyah Image üzerinden çalışır.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [Header("Kararma Bileşenleri")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _fadeImage;

        [Header("Açılış Ayarları")]
        [Tooltip("Sahne açıldığında otomatik olarak aydınlanma (Fade In) yapılsın mı?")]
        [SerializeField] private bool _fadeInOnStart = true;
        [Tooltip("Açılış kararma süresi (saniye)")]
        [SerializeField] private float _defaultFadeDuration = 0.5f;

        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_fadeImage == null)
            {
                _fadeImage = GetComponentInChildren<Image>();
            }
        }

        private void Start()
        {
            if (_fadeInOnStart)
            {
                // Sahne ilk açıldığında tam siyah başlar ve aydınlanır
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.blocksRaycasts = true;
                }
                FadeIn(_defaultFadeDuration);
            }
            else
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.blocksRaycasts = false;
                }
            }
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
        /// Ekranı karartır ve ardından hedef sahneyi yükler.
        /// </summary>
        public void FadeToScene(string sceneName, float duration = 0.5f)
        {
            FadeOut(duration, () =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }

        private void StartFade(float targetAlpha, float duration, Action onComplete, bool blockRaycasts)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration, onComplete, blockRaycasts));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete, bool blockRaycasts)
        {
            if (_canvasGroup == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            _canvasGroup.blocksRaycasts = true;
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _canvasGroup.blocksRaycasts = blockRaycasts;

            _fadeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
