using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kervan.UI
{
    /// <summary>
    /// EKRAN.md standardinda kazara basmayi onleyen basili tutmali dugme mekanigi.
    /// Belirtilen sure boyunca basili tutuldugunda eylemi tetikler.
    /// </summary>
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Ayarlar")]
        [Tooltip("Eylemin tetiklenmesi icin gereken basili tutma suresi (Saniye)")]
        [SerializeField] private float _holdDuration = 1.5f;

        [Header("Gorsel Geri Bildirim")]
        [SerializeField] private Image _fillProgressImage;

        public event Action OnHoldComplete;

        private bool _isPointerDown;
        private float _currentHoldTimer;

        private void Update()
        {
            if (_isPointerDown)
            {
                _currentHoldTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(_currentHoldTimer / _holdDuration);

                if (_fillProgressImage != null)
                {
                    _fillProgressImage.fillAmount = progress;
                }

                if (_currentHoldTimer >= _holdDuration)
                {
                    _isPointerDown = false;
                    _currentHoldTimer = 0f;
                    if (_fillProgressImage != null) _fillProgressImage.fillAmount = 0f;
                    OnHoldComplete?.Invoke();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            _currentHoldTimer = 0f;
            if (_fillProgressImage != null) _fillProgressImage.fillAmount = 0f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetHold();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetHold();
        }

        private void ResetHold()
        {
            _isPointerDown = false;
            _currentHoldTimer = 0f;
            if (_fillProgressImage != null) _fillProgressImage.fillAmount = 0f;
        }
    }
}
