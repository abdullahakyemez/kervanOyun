using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kervan.Audio;
using Kervan.Core.Save;
using Kervan.TradeAndTravel;
using Kervan.UI;

namespace Kervan.Core
{
    /// <summary>
    /// Oyun açılışında (00_Boot sahnesinde veya Editör'de herhangi bir sahneden Play'e basıldığında)
    /// temel servisleri (EventBus temizliği, Services, SaveService, GameSession, AudioManager) bir kez kuran
    /// ve uygulama yaşam döngüsü boyunca otomatik kayıtları yöneten kalıcı başlatıcı.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        private const string BootSceneName = SceneIds.Boot;

        [Header("Başlangıç Sahnesi Yönlendirmesi")]
        [Tooltip("00_Boot sahnesi tamamlandığında yüklenecek ilk sahne adı")]
        [SerializeField] private string _initialSceneName = SceneIds.MainMenu;

        private static bool _isInitialized;
        private static Bootstrapper _instance;

        /// <summary>
        /// Domain Reload kapalı olsa dahi her Play girişinde bayrağı ve statik kayıtları sıfırlar.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _isInitialized = false;
            _instance = null;
            EventBus.ClearAll();
            Services.Clear();
        }

        /// <summary>
        /// Hangi sahneden başlatılırsa başlatılsın sahne Awake'lerinden önce çekirdek servisleri kurar.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureCoreServices()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            if (!Services.IsRegistered<SaveService>())
            {
                var saveService = new SaveService();
                Services.Register(saveService);
            }

            if (!Services.IsRegistered<GameSession>())
            {
                var session = new GameSession();
                Services.Register(session);
            }

            // Kalıcı ScreenFader ve SceneLoader servislerini güvenceye al
            ScreenFader.EnsureInstance();
            SceneLoader.EnsureInstance();

            Debug.Log("[Bootstrapper] Çekirdek servisler (SaveService, GameSession, ScreenFader, SceneLoader) hazır.");
        }

        /// <summary>
        /// Editörde 00_Boot dışındaki bir sahneden Play'e basıldığında da yaşam döngüsü olaylarını
        /// (OnApplicationPause / OnApplicationQuit) dinleyebilmek için kalıcı Bootstrapper objesini güvenceye alır.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsurePersistentRunner()
        {
            if (_instance != null) return;

            var existing = FindAnyObjectByType<Bootstrapper>();
            if (existing != null)
            {
                _instance = existing;
                return;
            }

            var go = new GameObject("[Bootstrapper]");
            _instance = go.AddComponent<Bootstrapper>();
        }

        private void Awake()
        {
            EnsureCoreServices();

            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            // Sahnedeki kalıcı AudioManager varsa Services'e kaydet
            if (AudioManager.Instance != null && !Services.IsRegistered<AudioManager>())
            {
                Services.Register(AudioManager.Instance);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LocationChangedEvent>(OnLocationChanged);
            EventBus.Subscribe<DayPassedEvent>(OnDayPassed);
            EventBus.Subscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocationChangedEvent>(OnLocationChanged);
            EventBus.Unsubscribe<DayPassedEvent>(OnDayPassed);
            EventBus.Unsubscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
        }

        private void Start()
        {
            if (AudioManager.Instance != null && !Services.IsRegistered<AudioManager>())
            {
                Services.Register(AudioManager.Instance);
            }

            // Yalnızca 00_Boot sahnesindeysek otomatik olarak başlangıç sahnesine (MainMenuScene) geç
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (string.Equals(activeSceneName, BootSceneName, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(_initialSceneName))
            {
                Debug.Log($"[Bootstrapper] {activeSceneName} tamamlandı → {_initialSceneName} yükleniyor.");
                SceneLoader.Load(_initialSceneName, 0.35f);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                TryAutoSave("OnApplicationPause");
            }
        }

        private void OnApplicationQuit()
        {
            TryAutoSave("OnApplicationQuit");
        }

        private void OnLocationChanged(LocationChangedEvent evt)
        {
            TryAutoSave("LocationChanged");
        }

        private void OnDayPassed(DayPassedEvent evt)
        {
            TryAutoSave("DayPassed");
        }

        private void OnCaravanStatsChanged(CaravanStatsChangedEvent evt)
        {
            TryAutoSave("CaravanStatsChanged");
        }

        private static void TryAutoSave(string reason)
        {
            if (!Services.TryGet<GameSession>(out var session) || !session.IsSessionActive)
            {
                return;
            }

            if (!Services.TryGet<SaveService>(out var saveService))
            {
                return;
            }

            saveService.SaveSession(session);
        }
    }
}
