using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Kervan.Audio;

namespace Kervan.UI
{
    /// <summary>
    /// Ana Menü sahnesinin (MainMenuScene) ana kontrolcüsü.
    /// Yeni Oyun, Devam Et, Giriş Yöntemleri (Google, Apple, Mail) ve 
    /// Açılır Çekmece (Drawer) üzerindeki hızlı ses, ayarlar ve çıkış işlemlerini yönetir.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Ana Eylem Butonları (Sol Taraf)")]
        [Tooltip("Yeni oyunu başlatan buton (Yeşil ahşap zemin)")]
        [SerializeField] private Button _newGameButton;

        [Tooltip("Kayıtlı oyuna devam eden buton (Kahverengi ahşap zemin)")]
        [SerializeField] private Button _continueButton;

        [Header("Giriş Seçenekleri (Alt Orta)")]
        [SerializeField] private Button _googleLoginButton;
        [SerializeField] private Button _appleLoginButton;
        [SerializeField] private Button _emailLoginButton;

        [Header("Sağ Alt Çekmece & Araçlar (Utility Drawer)")]
        [SerializeField] private MenuDrawerController _menuDrawer;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _volumeToggleButton;
        [SerializeField] private Image _volumeIconImage;
        [SerializeField] private Sprite _volumeOnSprite;
        [SerializeField] private Sprite _volumeOffSprite;

        [Header("Açılır Pencereler (Popups)")]
        [SerializeField] private SettingsPopupUI _settingsPopup;

        [Header("Müzik & Ses Ayarları")]
        [Tooltip("Ana menü fon müziği (BGM_Main)")]
        [SerializeField] private AudioClip _bgmMainMenu;
        [Tooltip("Tıklama sesi yedeği (SFX_ButtonClick)")]
        [SerializeField] private AudioClip _sfxClickFallback;
        [SerializeField] private string _gameSceneName = "WorldMapScene";

        private bool _isMuted = false;
        private AudioSource _localAudioSource;

        private void Start()
        {
            RegisterButtons();
            PlayMainMenuMusic();
            CheckSaveFileStatus();
        }

        private void OnDestroy()
        {
            UnregisterButtons();
        }

        private void RegisterButtons()
        {
            if (_newGameButton != null) _newGameButton.onClick.AddListener(OnNewGameClicked);
            if (_continueButton != null) _continueButton.onClick.AddListener(OnContinueClicked);

            if (_googleLoginButton != null) _googleLoginButton.onClick.AddListener(OnGoogleLoginClicked);
            if (_appleLoginButton != null) _appleLoginButton.onClick.AddListener(OnAppleLoginClicked);
            if (_emailLoginButton != null) _emailLoginButton.onClick.AddListener(OnEmailLoginClicked);

            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
            if (_exitButton != null) _exitButton.onClick.AddListener(OnExitClicked);
            if (_volumeToggleButton != null) _volumeToggleButton.onClick.AddListener(OnVolumeToggleClicked);
        }

        private void UnregisterButtons()
        {
            if (_newGameButton != null) _newGameButton.onClick.RemoveListener(OnNewGameClicked);
            if (_continueButton != null) _continueButton.onClick.RemoveListener(OnContinueClicked);

            if (_googleLoginButton != null) _googleLoginButton.onClick.RemoveListener(OnGoogleLoginClicked);
            if (_appleLoginButton != null) _appleLoginButton.onClick.RemoveListener(OnAppleLoginClicked);
            if (_emailLoginButton != null) _emailLoginButton.onClick.RemoveListener(OnEmailLoginClicked);

            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (_exitButton != null) _exitButton.onClick.RemoveListener(OnExitClicked);
            if (_volumeToggleButton != null) _volumeToggleButton.onClick.RemoveListener(OnVolumeToggleClicked);
        }

        private void PlayMainMenuMusic()
        {
            if (_bgmMainMenu == null)
            {
                Debug.LogWarning("[MainMenuUI] '_bgmMainMenu' alanı boş! Inspector'da BGM_Main ses dosyasını sürükleyin.");
                return;
            }

            // 1. Tercih: Merkezi AudioManager üzerinden çalma
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(_bgmMainMenu, 1.2f);
            }
            else
            {
                // 2. Yedek (Fallback): Sahnede AudioManager yoksa bile sessiz kalmasın, yerel çal
                Debug.LogWarning("[MainMenuUI] Sahnede AudioManager bulunamadı! 'Assets/_Project/Prefabs/Audio/AudioManager.prefab' sahneye sürüklenmeli. Geçici olarak yerel AudioSource ile çalınıyor.");
                if (_localAudioSource == null)
                {
                    _localAudioSource = gameObject.AddComponent<AudioSource>();
                    _localAudioSource.loop = true;
                    _localAudioSource.playOnAwake = false;
                }
                _localAudioSource.clip = _bgmMainMenu;
                _localAudioSource.volume = 0.75f;
                _localAudioSource.Play();
            }
        }

        private void CheckSaveFileStatus()
        {
            if (_continueButton != null)
            {
                _continueButton.interactable = true;
            }
        }

        #region Buton Olayları (Event Handlers)

        private void OnNewGameClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] Yeni Oyun başlatılıyor. Hedef sahne: " + _gameSceneName);
            SceneManager.LoadScene(_gameSceneName);
        }

        private void OnContinueClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] Devam Et (Kayıt Yükle) tıklandı.");
            SceneManager.LoadScene(_gameSceneName);
        }

        private void OnGoogleLoginClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] Google Play Girişi seçildi.");
        }

        private void OnAppleLoginClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] Apple Game Center Girişi seçildi.");
        }

        private void OnEmailLoginClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] E-posta / Hesap Girişi seçildi.");
        }

        private void OnSettingsClicked()
        {
            PlayClickSound();
            if (_settingsPopup != null)
            {
                _settingsPopup.Show();
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] Ayarlar penceresi (SettingsPopupUI) referansı boş!");
            }
        }

        private void OnVolumeToggleClicked()
        {
            PlayClickSound();
            _isMuted = !_isMuted;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(_isMuted ? 0f : 1f);
            }
            else if (_localAudioSource != null)
            {
                _localAudioSource.mute = _isMuted;
            }

            UpdateVolumeIcon();
        }

        private void UpdateVolumeIcon()
        {
            if (_volumeIconImage == null) return;
            if (_volumeOnSprite != null && _volumeOffSprite != null)
            {
                _volumeIconImage.sprite = _isMuted ? _volumeOffSprite : _volumeOnSprite;
            }
        }

        private void OnExitClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] Oyundan çıkılıyor...");
            Application.Quit();
        }

        private void PlayClickSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
            else if (_sfxClickFallback != null)
            {
                AudioSource.PlayClipAtPoint(_sfxClickFallback, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            }
        }

        #endregion
    }
}
