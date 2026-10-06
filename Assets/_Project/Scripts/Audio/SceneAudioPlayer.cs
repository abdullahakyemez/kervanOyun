using UnityEngine;

namespace Kervan.Audio
{
    public enum SceneMusicType
    {
        Combat,
        Travel,
        Market,
        TownHub
    }

    /// <summary>
    /// Sahne yüklendiğinde AudioManager üzerinden ilgili sahne müziğini başlatan yardımcı bileşen.
    /// </summary>
    public class SceneAudioPlayer : MonoBehaviour
    {
        [SerializeField] private SceneMusicType _musicType;
        [SerializeField] private float _crossFadeDuration = 1.0f;

        private void Start()
        {
            if (AudioManager.Instance == null) return;

            var library = AudioManager.Instance.Library;
            if (library == null) return;

            AudioClip clipToPlay = null;
            switch (_musicType)
            {
                case SceneMusicType.Combat: clipToPlay = library.BgmCombat; break;
                case SceneMusicType.Travel: clipToPlay = library.BgmTravel; break;
                case SceneMusicType.Market: clipToPlay = library.BgmMarket; break;
                case SceneMusicType.TownHub: clipToPlay = library.BgmTownHub; break;
            }

            if (clipToPlay != null)
            {
                AudioManager.Instance.PlayMusic(clipToPlay, _crossFadeDuration);
            }
        }
    }
}
