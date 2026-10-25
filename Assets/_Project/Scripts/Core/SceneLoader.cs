using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kervan.UI;

namespace Kervan.Core
{
    /// <summary>
    /// Sahneler arası asenkron geçişleri, kararma (ScreenFader) entegrasyonunu
    /// ve çift dokunma/yükleme korumasını yöneten merkezi sahne yükleyici servisi.
    /// TD-07 (tutarsız sahne geçişleri) ve TD-14 (dağınık string sahne adları) çözümlerini sunar.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public bool IsLoading { get; private set; }

        /// <summary>
        /// Sahne geçişi ve kararma başladığında tetiklenir (hedef sahne adı).
        /// </summary>
        public event Action<string> OnSceneTransitionStarted;

        /// <summary>
        /// Yeni sahne asenkron yüklenip aydınlanma tamamlandığında tetiklenir (yüklenen sahne adı).
        /// </summary>
        public event Action<string> OnSceneTransitionCompleted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        /// <summary>
        /// Sahne veya Boot hiyerarşisinde SceneLoader yoksa dahi çalışma anında bağımsız,
        /// kalıcı (DontDestroyOnLoad) bir SceneLoader servisi oluşturarak hazırlar.
        /// </summary>
        public static SceneLoader EnsureInstance()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var existing = FindAnyObjectByType<SceneLoader>();
            if (existing != null)
            {
                Instance = existing;
                if (existing.transform.parent == null)
                {
                    DontDestroyOnLoad(existing.gameObject);
                }

                if (!Services.IsRegistered<SceneLoader>())
                {
                    Services.Register(existing);
                }

                return existing;
            }

            var rootGo = new GameObject("[SceneLoader]");
            DontDestroyOnLoad(rootGo);

            var loader = rootGo.AddComponent<SceneLoader>();
            Instance = loader;

            if (!Services.IsRegistered<SceneLoader>())
            {
                Services.Register(loader);
            }

            return loader;
        }

        /// <summary>
        /// Statik kolaylık çağrısı: SceneLoader.Load(SceneIds.WorldMap);
        /// </summary>
        public static void Load(string sceneName, float fadeDuration = 0.4f, Action onComplete = null)
        {
            var loader = EnsureInstance();
            loader.LoadScene(sceneName, fadeDuration, onComplete);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (!Services.IsRegistered<SceneLoader>())
            {
                Services.Register(this);
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
        /// Hedef sahneyi kararma (fade-out), asenkron yükleme ve aydınlanma (fade-in) adımlarıyla yükler.
        /// Çift tıklama ve eşzamanlı çoklu yükleme taleplerine karşı korumalıdır.
        /// </summary>
        public void LoadScene(string sceneName, float fadeDuration = 0.4f, Action onComplete = null)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneLoader] Zaten bir sahne geçişi devam ederken '{sceneName}' yükleme isteği geldi, yoksayılıyor.");
                return;
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[SceneLoader] Yüklenecek sahne adı boş olamaz!");
                return;
            }

            StartCoroutine(TransitionRoutine(sceneName, fadeDuration, onComplete));
        }

        private IEnumerator TransitionRoutine(string sceneName, float fadeDuration, Action onComplete)
        {
            IsLoading = true;
            OnSceneTransitionStarted?.Invoke(sceneName);

            // ScreenFader servisini al veya güvenceye al
            ScreenFader fader = null;
            if (Services.TryGet<ScreenFader>(out var registeredFader))
            {
                fader = registeredFader;
            }
            else
            {
                fader = ScreenFader.EnsureInstance();
            }

            // 1. Fade Out — Ekranı karart ve dokunmaları engelle
            if (fader != null)
            {
                yield return fader.FadeOutRoutine(fadeDuration);
            }

            // 2. Asenkron sahne yükleme
            AsyncOperation asyncOp = null;
            try
            {
                asyncOp = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneLoader] '{sceneName}' sahnesi yüklenirken hata oluştu: {ex.Message}");
            }

            if (asyncOp != null)
            {
                while (!asyncOp.isDone)
                {
                    yield return null;
                }
            }

            // Yeni sahnenin Awake ve Start yaşam döngüleri için 1 kare bekle
            yield return null;

            // 3. Fade In — Ekranı aydınlat ve dokunma kilidini kaldır
            if (fader != null)
            {
                yield return fader.FadeInRoutine(fadeDuration);
            }

            IsLoading = false;
            OnSceneTransitionCompleted?.Invoke(sceneName);
            onComplete?.Invoke();
        }
    }
}
