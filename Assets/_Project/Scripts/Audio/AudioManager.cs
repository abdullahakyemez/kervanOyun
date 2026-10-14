using System.Collections;
using UnityEngine;
using Kervan.Core;
using Kervan.Combat;
using Kervan.IntelSystem;
using Kervan.TradeAndTravel;

namespace Kervan.Audio
{
    /// <summary>
    /// Savaş, Seyahat, Pazar ve Han sahnelerinde tarihsel müzik ve ses efektlerini
    /// EventBus üzerinden yöneten tekil (Singleton) ses motoru.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<AudioManager>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Ses Kütüphanesi")]
        [SerializeField] private AudioDataSO _library;

        [Header("Ses Kanalları")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Ses Düzeyleri (0.0 - 1.0)")]
        [Range(0f, 1f)] [SerializeField] private float _masterVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float _musicVolume = 0.75f;
        [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 1.0f;

        private Coroutine _fadeCoroutine;

        public AudioDataSO Library => _library;
        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            RegisterEventBus();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            UnregisterEventBus();
        }

        private void InitializeAudioSources()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
                _bgmSource.loop = true;
                _bgmSource.playOnAwake = false;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        private void RegisterEventBus()
        {
            EventBus.Subscribe<CombatHitEvent>(OnCombatHit);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Subscribe<ShopProfitCollectedEvent>(OnShopProfitCollected);
            EventBus.Subscribe<MarketTransactionEvent>(OnMarketTransaction);
            EventBus.Subscribe<IntelGatheredEvent>(OnIntelGathered);
            EventBus.Subscribe<IntelSubmittedEvent>(OnIntelSubmitted);
            EventBus.Subscribe<TravelCompletedEvent>(OnTravelCompleted);
        }

        private void UnregisterEventBus()
        {
            EventBus.Unsubscribe<CombatHitEvent>(OnCombatHit);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Unsubscribe<ShopProfitCollectedEvent>(OnShopProfitCollected);
            EventBus.Unsubscribe<MarketTransactionEvent>(OnMarketTransaction);
            EventBus.Unsubscribe<IntelGatheredEvent>(OnIntelGathered);
            EventBus.Unsubscribe<IntelSubmittedEvent>(OnIntelSubmitted);
            EventBus.Unsubscribe<TravelCompletedEvent>(OnTravelCompleted);
        }

        public void SetAudioLibrary(AudioDataSO library)
        {
            _library = library;
        }

        #region Müzik Kontrolleri (BGM)

        public void PlayMusic(AudioClip newClip, float fadeDuration = 1.0f)
        {
            if (newClip == null || _bgmSource.clip == newClip) return;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeMusicRoutine(newClip, fadeDuration));
        }

        private IEnumerator FadeMusicRoutine(AudioClip newClip, float duration)
        {
            float targetVol = _masterVolume * _musicVolume;
            float halfDuration = Mathf.Max(0.1f, duration * 0.5f);

            // Ses kısılma (Fade out)
            if (_bgmSource.isPlaying)
            {
                float startVol = _bgmSource.volume;
                for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
                {
                    _bgmSource.volume = Mathf.Lerp(startVol, 0f, t / halfDuration);
                    yield return null;
                }
            }

            _bgmSource.clip = newClip;
            _bgmSource.Play();

            // Ses açılma (Fade in)
            for (float t = 0; t < halfDuration; t += Time.unscaledDeltaTime)
            {
                _bgmSource.volume = Mathf.Lerp(0f, targetVol, t / halfDuration);
                yield return null;
            }

            _bgmSource.volume = targetVol;
            _fadeCoroutine = null;
        }

        #endregion

        #region Ses Efekti Kontrolleri (SFX)

        public void PlaySFX(AudioClip clip, float volumeScale = 1.0f, float pitchVariation = 0.05f)
        {
            if (clip == null || _sfxSource == null) return;

            float originalPitch = 1.0f;
            float randomPitch = originalPitch + Random.Range(-pitchVariation, pitchVariation);
            _sfxSource.pitch = randomPitch;

            float finalVolume = _masterVolume * _sfxVolume * volumeScale;
            _sfxSource.PlayOneShot(clip, finalVolume);
        }

        public void PlaySwordHit() => PlaySFX(_library != null ? _library.SfxSwordHit : null);
        public void PlayArrowShot() => PlaySFX(_library != null ? _library.SfxArrowShot : null);
        public void PlayShieldBlock() => PlaySFX(_library != null ? _library.SfxShieldBlock : null);
        public void PlayCoins() => PlaySFX(_library != null ? _library.SfxCoins : null);
        public void PlayPaperRustle()
        {
            AudioClip clip = _library != null ? _library.SfxPaperRustle : null;
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>("Audio/SFX/SFX_PaperRustle");
            }
            PlaySFX(clip, 0.85f, 0.03f);
        }

        public void PlayParchment() => PlayPaperRustle();
        public void PlayButtonClick()
        {
            AudioClip clip = _library != null ? _library.SfxButtonClick : null;
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>("Audio/SFX/SFX_ButtonClick");
            }
            PlaySFX(clip, 0.7f, 0.02f);
        }
        public void PlayVictory() => PlaySFX(_library != null ? _library.SfxVictory : null);
        public void PlayDefeat() => PlaySFX(_library != null ? _library.SfxDefeat : null);

        #endregion

        #region Ses Ayarları

        public void SetMasterVolume(float vol)
        {
            _masterVolume = Mathf.Clamp01(vol);
            UpdateVolumes();
        }

        public void SetMusicVolume(float vol)
        {
            _musicVolume = Mathf.Clamp01(vol);
            UpdateVolumes();
        }

        public void SetSFXVolume(float vol)
        {
            _sfxVolume = Mathf.Clamp01(vol);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (_bgmSource != null) _bgmSource.volume = _masterVolume * _musicVolume;
        }

        #endregion

        #region EventBus Dinleyicileri

        private void OnCombatHit(CombatHitEvent evt)
        {
            if (evt.DamageAbsorbed > 0 && evt.DamageToHealth == 0)
            {
                PlayShieldBlock();
            }
            else
            {
                PlaySwordHit();
            }
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            if (evt.IsVictory)
            {
                PlayVictory();
            }
            else
            {
                PlayDefeat();
            }
        }

        private void OnMarketTransaction(MarketTransactionEvent evt)
        {
            PlayCoins();
        }

        private void OnShopProfitCollected(ShopProfitCollectedEvent evt)
        {
            PlayCoins();
        }

        private void OnIntelGathered(IntelGatheredEvent evt)
        {
            PlayParchment();
        }

        private void OnIntelSubmitted(IntelSubmittedEvent evt)
        {
            PlayCoins();
        }

        private void OnTravelCompleted(TravelCompletedEvent evt)
        {
            PlayParchment();
        }

        #endregion
    }
}
