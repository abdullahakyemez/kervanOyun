using UnityEngine;
using TMPro;
using Kervan.Combat;

namespace Kervan.UI
{
    /// <summary>
    /// EKRAN.md standardinda Hedef Odak Balonu (Floating Combat Prediction).
    /// Hedeflenen dusmanin basinin uzerinde isabet ve tahmini hasar gosterir.
    /// </summary>
    public class CombatPredictionUI : MonoBehaviour
    {
        [Header("Bilesenler")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _hitChanceText;
        [SerializeField] private TextMeshProUGUI _damageEstimateText;
        [SerializeField] private CombatTurnManager _turnManager;

        private Camera _mainCamera;
        private Transform _targetTransform;
        private Vector3 _worldOffset = new Vector3(0f, 0.75f, 0f);

        private void Awake()
        {
            _mainCamera = Camera.main;
            Hide();
        }

        private void Start()
        {
            if (_turnManager == null) _turnManager = FindAnyObjectByType<CombatTurnManager>();
            if (_turnManager != null)
            {
                _turnManager.OnTargetEnemyFocused += Show;
                _turnManager.OnTargetEnemyUnfocused += Hide;
            }
        }

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnTargetEnemyFocused -= Show;
                _turnManager.OnTargetEnemyUnfocused -= Hide;
            }
        }

        private void LateUpdate()
        {
            if (_panelRoot != null && _panelRoot.activeSelf && _targetTransform != null && _mainCamera != null)
            {
                Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetTransform.position + _worldOffset);
                transform.position = screenPos;
            }
        }

        public void Show(CombatUnit attacker, CombatUnit defender)
        {
            if (attacker == null || defender == null)
            {
                Hide();
                return;
            }

            _targetTransform = defender.transform;
            int dist = HexCoordinates.Distance(attacker.Coordinates, defender.Coordinates);
            var (hitChance, minDmg, maxDmg) = DamageCalculator.PredictCombat(attacker, defender, dist);

            if (_hitChanceText != null)
            {
                _hitChanceText.text = $"İsabet: %{hitChance}";
            }

            if (_damageEstimateText != null)
            {
                _damageEstimateText.text = $"Hasar: {minDmg} - {maxDmg}";
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
        }

        public void Hide()
        {
            _targetTransform = null;
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }
    }
}
