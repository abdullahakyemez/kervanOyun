using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using Kervan.Core;
using Kervan.Core.Save;
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
        [SerializeField] private QuitConfirmPopupUI _quitConfirmPopup;
        [Tooltip("Mevcut kayıt varken Yeni Oyun'a basıldığında açılacak onay penceresi (Popup_NewGameConfirm)")]
        [SerializeField] private QuitConfirmPopupUI _newGameConfirmPopup;

        [Header("Ekran Geçişi & Kararma (Screen Fader)")]
        [Tooltip("Sahneler arası yumuşak kararma sağlayan fader referansı")]
        [SerializeField] private ScreenFader _screenFader;
        [SerializeField] private float _fadeDuration = 0.5f;

        [Header("Sürüm Bilgisi (Version Tag)")]
        [SerializeField] private TextMeshProUGUI _versionText;
        [SerializeField] private string _versionString = "v0.1.0 Alpha — Uç Beyliği Kervanı";

        [Header("Geliştirici / Test Seçenekleri")]
        [Tooltip("Test amaçlı Devam Et butonunu daima aktif tutmak için işaretleyin.")]
        [SerializeField] private bool _forceEnableContinueForTesting = false;

        [Header("Müzik & Ses Ayarları")]
        [Tooltip("Ana menü fon müziği (BGM_Main)")]
        [SerializeField] private AudioClip _bgmMainMenu;
        [Tooltip("Tıklama sesi yedeği (SFX_ButtonClick)")]
        [SerializeField] private AudioClip _sfxClickFallback;
        [SerializeField] private string _gameSceneName = SceneIds.WorldMap;

        private bool _isMuted = false;
        private AudioSource _localAudioSource;

        private void Start()
        {
            RegisterButtons();
            InitVolumeState();
            PlayMainMenuMusic();
            CheckSaveFileStatus();
            InitVersionText();
        }

        private void OnDestroy()
        {
            UnregisterButtons();
        }

        private void Update()
        {
            HandleBackButton();
        }

        /// <summary>
        /// Android Geri tuşu (veya klavye Escape) basıldığında açık popupları veya çekmeceyi kapatır.
        /// Hiçbir pencere açık değilse çıkış onayını tetikler.
        /// </summary>
        private void HandleBackButton()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

            if (_settingsPopup != null && _settingsPopup.gameObject.activeSelf)
            {
                _settingsPopup.Hide();
            }
            else if (_newGameConfirmPopup != null && _newGameConfirmPopup.gameObject.activeSelf)
            {
                _newGameConfirmPopup.Hide();
            }
            else if (_quitConfirmPopup != null && _quitConfirmPopup.gameObject.activeSelf)
            {
                _quitConfirmPopup.Hide();
            }
            else if (_menuDrawer != null && _menuDrawer.IsOpen)
            {
                _menuDrawer.CloseDrawer();
            }
            else
            {
                OnExitClicked();
            }
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

        private void InitVolumeState()
        {
            _isMuted = AudioManager.Instance != null && AudioManager.Instance.IsMuted;
            UpdateVolumeIcon();
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
                _localAudioSource.mute = _isMuted;
                _localAudioSource.Play();
            }
        }

        private void CheckSaveFileStatus()
        {
            if (_continueButton == null) return;

            bool hasSaveFile = Services.TryGet<SaveService>(out var saveService) && saveService.HasSaveFile();
            _continueButton.interactable = _forceEnableContinueForTesting || hasSaveFile;
        }

        private void InitVersionText()
        {
            if (_versionText != null)
            {
                _versionText.text = _versionString;
            }
        }

        #region Buton Olayları (Event Handlers)

        private void OnNewGameClicked()
        {
            PlayClickSound();

            bool hasSaveFile = Services.TryGet<SaveService>(out var saveService) && saveService.HasSaveFile();
            if (hasSaveFile)
            {
                if (_newGameConfirmPopup != null)
                {
                    _newGameConfirmPopup.ShowCustomConfirm(ExecuteStartNewGame);
                    return;
                }

                if (_quitConfirmPopup != null)
                {
                    _quitConfirmPopup.ShowCustomConfirm(
                        ExecuteStartNewGame,
                        "YENİ OYUN",
                        "Mevcut kaydınız silinecek ve yeni bir kervan seferi başlayacak. Onaylıyor musunuz?"
                    );
                    return;
                }
            }

            ExecuteStartNewGame();
        }

        private void ExecuteStartNewGame()
        {
            Debug.Log("[MainMenuUI] Yeni Oyun başlatılıyor. Hedef sahne: " + _gameSceneName);

            var session = GetOrCreateSession();
            session.StartNewGame(GameSession.DefaultStartNodeId);

            if (Services.TryGet<SaveService>(out var saveService))
            {
                saveService.SaveSession(session);
            }

            TransitionToGameScene();
        }

        private void OnContinueClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] Devam Et (Kayıt Yükle) tıklandı. Hedef sahne: " + _gameSceneName);

            var session = GetOrCreateSession();

            if (Services.TryGet<SaveService>(out var saveService))
            {
                if (!saveService.LoadIntoSession(session))
                {
                    Debug.LogWarning("[MainMenuUI] Kayıt dosyası okunamadı veya bozuk! Bozuk kayıt temizlenip yeni oyun başlatılıyor.");
                    saveService.DeleteSaveFile();
                    session.StartNewGame(GameSession.DefaultStartNodeId);
                    saveService.SaveSession(session);
                }
            }

            TransitionToGameScene();
        }

        private static GameSession GetOrCreateSession()
        {
            if (!Services.TryGet<GameSession>(out var session))
            {
                session = new GameSession();
                Services.Register(session);
            }
            return session;
        }

        private void TransitionToGameScene()
        {
            string targetScene = !string.IsNullOrEmpty(_gameSceneName) ? _gameSceneName : SceneIds.WorldMap;
            SceneLoader.Load(targetScene, _fadeDuration);
        }

        private void OnGoogleLoginClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] Google Play Girişi seçildi. (Geliştirme aşamasında)");
        }

        private void OnAppleLoginClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] Apple Game Center Girişi seçildi. (Geliştirme aşamasında)");
        }

        private void OnEmailLoginClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenuUI] E-posta / Hesap Girişi seçildi. (Geliştirme aşamasında)");
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
            if (AudioManager.Instance != null)
            {
                _isMuted = AudioManager.Instance.ToggleMute();
            }
            else
            {
                _isMuted = !_isMuted;
                if (_localAudioSource != null)
                {
                    _localAudioSource.mute = _isMuted;
                }
            }

            if (!_isMuted)
            {
                PlayClickSound();
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
            if (_quitConfirmPopup != null)
            {
                _quitConfirmPopup.Show();
            }
            else
            {
                Debug.Log("[MainMenuUI] Oyundan çıkılıyor...");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
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
