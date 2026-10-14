using UnityEngine;

namespace Kervan.Audio
{
    /// <summary>
    /// Oyundaki tüm tarihsel müzik ve ses efekti referanslarını tutan merkezi kütüphane.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "Kervan/Audio/Audio Library", order = 1)]
    public class AudioDataSO : ScriptableObject
    {
        [Header("Arka Plan Müzikleri (BGM)")]
        [SerializeField] private AudioClip _bgmCombat;
        [SerializeField] private AudioClip _bgmTravel;
        [SerializeField] private AudioClip _bgmMarket;
        [SerializeField] private AudioClip _bgmTownHub;

        [Header("Savaş Ses Efektleri (Combat SFX)")]
        [SerializeField] private AudioClip _sfxSwordHit;
        [SerializeField] private AudioClip _sfxArrowShot;
        [SerializeField] private AudioClip _sfxShieldBlock;
        [SerializeField] private AudioClip _sfxVictory;
        [SerializeField] private AudioClip _sfxDefeat;

        [Header("Ticaret & Arayüz Ses Efektleri (Trade & UI SFX)")]
        [SerializeField] private AudioClip _sfxCoins;
        [Tooltip("Eski parşömen klibi yuvası (Geriye dönük uyumluluk için)")]
        [SerializeField] private AudioClip _sfxParchment;
        [Tooltip("Sayfa, çekmece ve kart açılışlarında kullanılan parşömen hışırtısı sesi (SFX_PaperRustle)")]
        [SerializeField] private AudioClip _sfxPaperRustle;
        [SerializeField] private AudioClip _sfxButtonClick;
        [SerializeField] private AudioClip _sfxTravelStep;

        // Getters
        public AudioClip BgmCombat => _bgmCombat;
        public AudioClip BgmTravel => _bgmTravel;
        public AudioClip BgmMarket => _bgmMarket;
        public AudioClip BgmTownHub => _bgmTownHub;

        public AudioClip SfxSwordHit => _sfxSwordHit;
        public AudioClip SfxArrowShot => _sfxArrowShot;
        public AudioClip SfxShieldBlock => _sfxShieldBlock;
        public AudioClip SfxVictory => _sfxVictory;
        public AudioClip SfxDefeat => _sfxDefeat;

        public AudioClip SfxCoins => _sfxCoins;
        public AudioClip SfxPaperRustle => _sfxPaperRustle != null ? _sfxPaperRustle : _sfxParchment;
        public AudioClip SfxParchment => SfxPaperRustle;
        public AudioClip SfxButtonClick => _sfxButtonClick;
        public AudioClip SfxTravelStep => _sfxTravelStep;

        public void SetAudioClips(
            AudioClip combat, AudioClip travel, AudioClip market, AudioClip townHub,
            AudioClip sword, AudioClip arrow, AudioClip shield, AudioClip victory, AudioClip defeat,
            AudioClip coins, AudioClip parchment, AudioClip buttonClick, AudioClip travelStep,
            AudioClip paperRustle = null)
        {
            _bgmCombat = combat;
            _bgmTravel = travel;
            _bgmMarket = market;
            _bgmTownHub = townHub;

            _sfxSwordHit = sword;
            _sfxArrowShot = arrow;
            _sfxShieldBlock = shield;
            _sfxVictory = victory;
            _sfxDefeat = defeat;

            _sfxCoins = coins;
            _sfxParchment = parchment;
            _sfxPaperRustle = paperRustle != null ? paperRustle : parchment;
            _sfxButtonClick = buttonClick;
            _sfxTravelStep = travelStep;
        }
    }
}
