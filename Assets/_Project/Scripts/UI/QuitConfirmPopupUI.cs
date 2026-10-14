using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Kervan.Audio;

namespace Kervan.UI
{
    /// <summary>
    /// Oyundan çıkış butonuna basıldığında açılan parşömen onay penceresi.
    /// Kazara tıklamaları önler, oyuncudan onay alır veya menüye geri döndürür.
    /// </summary>
    public class QuitConfirmPopupUI : MonoBehaviour
    {
        [Header("Onay ve İptal Butonları")]
        [Tooltip("Çıkışı onaylayan buton (Evet)")]
        [SerializeField] private Button _confirmQuitButton;

        [Tooltip("Çıkışı iptal edip pencereyi kapatan buton (Hayır / Vazgeç)")]
        [SerializeField] private Button _cancelQuitButton;

        [Header("Ses Efektleri")]
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;

        [Header("Pencere Animasyonu")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.2f;

        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            if (_confirmQuitButton != null)
            {
                _confirmQuitButton.onClick.AddListener(OnConfirmQuitClicked);
            }

            if (_cancelQuitButton != null)
            {
                _cancelQuitButton.onClick.AddListener(Hide);
            }

            // Başlangıçta pencereyi gizle
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_confirmQuitButton != null) _confirmQuitButton.onClick.RemoveListener(OnConfirmQuitClicked);
            if (_cancelQuitButton != null) _cancelQuitButton.onClick.RemoveListener(Hide);
        }

        /// <summary>
        /// Onay penceresini parşömen sesiyle birlikte açar.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            PlayOpenAudio();

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(1f, true));
        }

        /// <summary>
        /// Onay penceresini kapatır.
        /// </summary>
        public void Hide()
        {
            PlayCloseAudio();

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(0f, false));
        }

        private void OnConfirmQuitClicked()
        {
            PlayCloseAudio();
            Debug.Log("[QuitConfirmPopupUI] Çıkış onaylandı. Oyun kapatılıyor...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void PlayOpenAudio()
        {
            if (_openSound != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(_openSound, 1.0f, 0f);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(_openSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
                }
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayParchment();
            }
        }

        private void PlayCloseAudio()
        {
            if (_closeSound != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(_closeSound, 1.0f, 0f);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(_closeSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
                }
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        private IEnumerator FadeRoutine(float targetAlpha, bool isVisible)
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(isVisible);
                yield break;
            }

            _canvasGroup.blocksRaycasts = isVisible;
            _canvasGroup.interactable = isVisible;

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;

            if (!isVisible)
            {
                gameObject.SetActive(false);
            }

            _fadeCoroutine = null;
        }
    }
}
