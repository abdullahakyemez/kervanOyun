using UnityEngine;
using UnityEngine.SceneManagement;
using Kervan.Audio;

namespace Kervan.Core
{
    /// <summary>
    /// Oyun açılışında (00_Boot sahnesinde veya Editör'de herhangi bir sahneden Play'e basıldığında)
    /// temel servisleri (EventBus temizliği, Services, GameSession, AudioManager) bir kez kuran başlatıcı.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Başlangıç Sahnesi Yönlendirmesi")]
        [Tooltip("00_Boot sahnesi tamamlandığında yüklenecek ilk sahne adı")]
        [SerializeField] private string _initialSceneName = "MainMenuScene";

        private static bool _isInitialized;

        /// <summary>
        /// Domain Reload kapalı olsa dahi her Play girişinde bayrağı ve statik kayıtları sıfırlar.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _isInitialized = false;
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

            if (!Services.IsRegistered<GameSession>())
            {
                var session = new GameSession();
                Services.Register(session);
            }

            Debug.Log("[Bootstrapper] Çekirdek servisler (GameSession) hazır.");
        }

        private void Awake()
        {
            EnsureCoreServices();

            // Sahnedeki kalıcı AudioManager varsa Services'e kaydet
            if (AudioManager.Instance != null && !Services.IsRegistered<AudioManager>())
            {
                Services.Register(AudioManager.Instance);
            }
        }

        private void Start()
        {
            if (AudioManager.Instance != null && !Services.IsRegistered<AudioManager>())
            {
                Services.Register(AudioManager.Instance);
            }

            // Eğer bu bileşen 00_Boot sahnesinde çalışıyorsa otomatik olarak ilk sahneye geç
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrWhiteSpace(_initialSceneName) &&
                !string.Equals(activeSceneName, _initialSceneName, System.StringComparison.Ordinal))
            {
                Debug.Log($"[Bootstrapper] {activeSceneName} tamamlandı → {_initialSceneName} yükleniyor.");
                SceneManager.LoadScene(_initialSceneName);
            }
        }
    }
}
