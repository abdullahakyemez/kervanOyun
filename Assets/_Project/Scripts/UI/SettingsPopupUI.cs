using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Audio;

namespace Kervan.UI
{
    /// <summary>
    /// UI_Frame_Market üzerine oturan ses ve oyun ayarları penceresi (Popup).
    /// Müzik ve ses efekti düzeylerini ayarlar, görsel yüzde metinlerini günceller
    /// ve parşömen açılış sesini (SFX_PaperRustle) çalar.
    /// </summary>
    public class SettingsPopupUI : MonoBehaviour
    {
        [Header("Ses Efektleri")]
        [Tooltip("Pencere açılırken çalacak parşömen hışırtısı sesi (SFX_PaperRustle)")]
        [SerializeField] private AudioClip _openSound;
        [Tooltip("Pencere kapanırken çalacak ses (SFX_ButtonClick)")]
        [SerializeField] private AudioClip _closeSound;

        [Header("Slider Kontrolleri")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        [Header("Yüzde / Değer Göstergeleri (TextMeshPro)")]
        [Tooltip("Müzik ses seviyesini gösteren metin (örn: %75)")]
        [SerializeField] private TextMeshProUGUI _musicValueText;
        [Tooltip("Ses efekti seviyesini gösteren metin (örn: %100)")]
        [SerializeField] private TextMeshProUGUI _sfxValueText;

        [Header("Butonlar")]
        [SerializeField] private Button _closeButton;

        [Header("Pencere Animasyonu")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.2f;

        private Coroutine _fadeCoroutine;
        private float _lastSfxTestTime = 0f;

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
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            if (_musicSlider != null)
            {
                _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            // Başlangıçta pencereyi gizle
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Hide);
            if (_musicSlider != null) _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }

        /// <summary>
        /// Ayarlar penceresini açar, mevcut ses düzeylerini slider'lara yükler ve parşömen açılış sesini çalar.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            // Mevcut ses düzeylerini AudioManager'dan oku ve slider'lara senkronize et
            SyncVolumeToSliders();

            // Parşömen açılış sesini çal
            PlayOpenAudio();

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(1f, true));
        }

        /// <summary>
        /// Ayarlar penceresini kapatır ve kapanış sesini çalar.
        /// </summary>
        public void Hide()
        {
            PlayCloseAudio();

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(0f, false));
        }

        private void SyncVolumeToSliders()
        {
            float musicVol = 0.75f;
            float sfxVol = 1.0f;

            if (AudioManager.Instance != null)
            {
                musicVol = AudioManager.Instance.MusicVolume;
                sfxVol = AudioManager.Instance.SfxVolume;
            }

            if (_musicSlider != null)
            {
                _musicSlider.SetValueWithoutNotify(musicVol);
                UpdateMusicLabel(musicVol);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.SetValueWithoutNotify(sfxVol);
                UpdateSfxLabel(sfxVol);
            }
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }

            UpdateMusicLabel(value);
            Debug.Log($"[SettingsPopupUI] Müzik Sesi Ayarlandı: %{Mathf.RoundToInt(value * 100)}");
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);

                // Slider çekilirken oyuncunun yeni ses düzeyini hemen duyabilmesi için test sesi çal (saniyede en fazla 4 kez)
                if (Time.unscaledTime - _lastSfxTestTime > 0.2f)
                {
                    _lastSfxTestTime = Time.unscaledTime;
                    AudioManager.Instance.PlayButtonClick();
                }
            }

            UpdateSfxLabel(value);
            Debug.Log($"[SettingsPopupUI] Efekt Sesi Ayarlandı: %{Mathf.RoundToInt(value * 100)}");
        }

        private void UpdateMusicLabel(float value)
        {
            if (_musicValueText != null)
            {
                _musicValueText.text = $"%{Mathf.RoundToInt(value * 100)}";
            }
        }

        private void UpdateSfxLabel(float value)
        {
            if (_sfxValueText != null)
            {
                _sfxValueText.text = $"%{Mathf.RoundToInt(value * 100)}";
            }
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
                    // Fallback: Sahnede AudioManager yoksa bile doğrudan çal
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

            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            _canvasGroup.blocksRaycasts = isVisible;
            _canvasGroup.interactable = isVisible;

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
