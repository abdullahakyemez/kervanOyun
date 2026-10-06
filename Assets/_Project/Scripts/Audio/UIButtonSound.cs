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
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}
