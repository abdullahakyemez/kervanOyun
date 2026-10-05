using UnityEngine;
using UnityEngine.InputSystem;

namespace Kervan.Combat
{
    /// <summary>
    /// Mobil dokunmatik ve masaustu fare tiklamalarini algilayip
    /// altigen koordinata ceviren ve CombatTurnManager'a ileten bilesen.
    /// </summary>
    public class CombatInputHandler : MonoBehaviour
    {
        [SerializeField] private CombatTurnManager _turnManager;
        [SerializeField] private Camera _mainCamera;

        private void Awake()
        {
            if (_turnManager == null) _turnManager = GetComponent<CombatTurnManager>();
            if (_mainCamera == null) _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_turnManager == null || _mainCamera == null) return;

            // Dokunmatik veya Fare Sol Tiklamasi
            bool pointerDown = false;
            Vector2 screenPos = Vector2.zero;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                pointerDown = true;
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                pointerDown = true;
                screenPos = Mouse.current.position.ReadValue();
            }

            if (pointerDown)
            {
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                HexCoordinates clickedCoords = HexCoordinates.FromWorldPosition(worldPos);

                _turnManager.HandleTileClicked(clickedCoords);
            }
        }
    }
}
