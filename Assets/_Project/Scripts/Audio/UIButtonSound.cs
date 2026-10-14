using UnityEngine;
using UnityEngine.UI;

namespace Kervan.Audio
{
    /// <summary>
    /// UI butonuna eklendiğinde tıklama esnasında AudioManager üzerinden
    /// ahşap/parşömen tıklama ses efektini (SFX_ButtonClick) çalar.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [Header("Özel Ses Ayarları (Opsiyonel)")]
        [Tooltip("Özel bir tıklama sesi efekti çalmak isterseniz sürükleyin. Boş bırakılırsa AudioManager'daki varsayılan SFX_ButtonClick çalar.")]
        [SerializeField] private AudioClip _customClickClip;

        [Range(0f, 1f)]
        [Tooltip("Tıklama ses düzeyi.")]
        [SerializeField] private float _volumeScale = 1.0f;

        [Tooltip("Tıklama sesinde gerçekçi hafif perde (pitch) değişkenliği uygula.")]
        [SerializeField] private bool _usePitchVariation = true;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(OnClick);
            }
        }

        private void Start()
        {
            // Eğer sahne başlangıcında başka bir script dinleyicileri temizlediyse kendini yeniden güvenceye al
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
                _button.onClick.AddListener(OnClick);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            // 1. Durum: Butona özel bir ses efekti atanmışsa
            if (_customClickClip != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(_customClickClip, _volumeScale, _usePitchVariation ? 0.04f : 0f);
                }
                else
                {
                    // Fallback: Sahnede AudioManager yoksa doğrudan kameradan çal
                    AudioSource.PlayClipAtPoint(_customClickClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, _volumeScale);
                }
                return;
            }

            // 2. Durum: Varsayılan buton tıklama sesi (SFX_ButtonClick)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
            else
            {
                // Fallback: Doğrudan harita sahnesinden başlatılan testlerde sessiz kalmayı önle
                AudioClip fallbackClip = Resources.Load<AudioClip>("Audio/SFX/SFX_ButtonClick");
                if (fallbackClip != null)
                {
                    AudioSource.PlayClipAtPoint(fallbackClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, _volumeScale * 0.7f);
                }
                else
                {
                    Debug.LogWarning("[UIButtonSound] Sahnede 'AudioManager' bulunamadı! Tıklama seslerinin sorunsuz çalışması için sahneye 'AudioManager.prefab' ekleyin veya testi MainMenuScene üzerinden başlatın.", this);
                }
            }
        }
    }
}
