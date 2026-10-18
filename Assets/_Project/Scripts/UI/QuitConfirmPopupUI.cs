using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Audio;

namespace Kervan.UI
{
    /// <summary>
    /// Oyundan çıkış veya mevcut kaydın üzerine yeni oyun başlatma gibi kritik işlemlerde
    /// açılan parşömen onay penceresi. Kazara tıklamaları önler ve oyuncudan onay alır.
    /// </summary>
    public class QuitConfirmPopupUI : MonoBehaviour
    {
        [Header("Onay ve İptal Butonları")]
        [Tooltip("İşlemi onaylayan buton (Evet)")]
        [SerializeField] private Button _confirmQuitButton;

        [Tooltip("İşlemi iptal edip pencereyi kapatan buton (Hayır / Vazgeç)")]
        [SerializeField] private Button _cancelQuitButton;

        [Header("Opsiyonel Metin Alanları (Özel Onay Mesajı İçin)")]
        [Tooltip("Onay penceresi başlık metni (opsiyonel)")]
        [SerializeField] private TMP_Text _titleText;

        [Tooltip("Onay penceresi açıklama metni (opsiyonel)")]
        [SerializeField] private TMP_Text _messageText;

        [Header("Ses Efektleri")]
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;

        [Header("Pencere Animasyonu")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.2f;

        private Coroutine _fadeCoroutine;
        private Action _customConfirmAction;
        private string _defaultTitle;
        private string _defaultMessage;
        private bool _capturedDefaults;

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

            CaptureDefaultTexts();
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
        /// Varsayılan çıkış onay penceresini parşömen sesiyle birlikte açar.
        /// </summary>
        public void Show()
        {
            CaptureDefaultTexts();
            _customConfirmAction = null;
            RestoreDefaultTexts();
            OpenPopup();
        }

        /// <summary>
        /// Özel bir onay eylemi (ör. mevcut kaydı silip Yeni Oyun başlatma) için onay penceresini açar.
        /// </summary>
        public void ShowCustomConfirm(Action onConfirm, string customTitle = null, string customMessage = null)
        {
            CaptureDefaultTexts();
            _customConfirmAction = onConfirm;

            if (_titleText != null && !string.IsNullOrWhiteSpace(customTitle))
            {
                _titleText.text = customTitle;
            }

            if (_messageText != null && !string.IsNullOrWhiteSpace(customMessage))
            {
                _messageText.text = customMessage;
            }

            OpenPopup();
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

        private void OpenPopup()
        {
            gameObject.SetActive(true);
            PlayOpenAudio();

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(1f, true));
        }

        private void CaptureDefaultTexts()
        {
            if (_capturedDefaults) return;
            _capturedDefaults = true;

            if (_titleText != null) _defaultTitle = _titleText.text;
            if (_messageText != null) _defaultMessage = _messageText.text;
        }

        private void RestoreDefaultTexts()
        {
            if (!_capturedDefaults) return;

            if (_titleText != null && _defaultTitle != null) _titleText.text = _defaultTitle;
            if (_messageText != null && _defaultMessage != null) _messageText.text = _defaultMessage;
        }

        private void OnConfirmQuitClicked()
        {
            if (_customConfirmAction != null)
            {
                Action callback = _customConfirmAction;
                _customConfirmAction = null;
                Hide();
                callback.Invoke();
                return;
            }

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
