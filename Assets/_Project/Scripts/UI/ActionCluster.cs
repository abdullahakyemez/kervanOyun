using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Combat;
using Kervan.Core;

namespace Kervan.UI
{
    /// <summary>
    /// EKRAN.md standardinda Sag Basparmak Bolgesi: Eylem Komuta Kumesi.
    /// "Turu Bitir", "Saldir/Mod Degistir", "Siper Al" ve "Ozel Yetenek" dugmelerini yonetir.
    /// </summary>
    public class ActionCluster : MonoBehaviour
    {
        [Header("Yoneticiler")]
        [SerializeField] private CombatTurnManager _turnManager;

        [Header("Birincil Eylem")]
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private TextMeshProUGUI _endTurnText;

        [Header("Ikincil Eylemler")]
        [SerializeField] private Button _attackModeButton;
        [SerializeField] private Button _defendButton;
        [SerializeField] private Button _skillButton;

        private CombatUnit _currentUnit;

        private void Start()
        {
            if (_turnManager == null)
            {
                _turnManager = FindAnyObjectByType<CombatTurnManager>();
            }

            if (_turnManager != null)
            {
                _turnManager.OnUnitSelected += HandleUnitSelected;
                _turnManager.OnTurnChanged += HandleTurnChanged;
            }

            if (_endTurnButton != null) _endTurnButton.onClick.AddListener(OnEndTurnClicked);
            if (_defendButton != null) _defendButton.onClick.AddListener(OnDefendClicked);
            if (_attackModeButton != null) _attackModeButton.onClick.AddListener(OnAttackModeClicked);
            if (_skillButton != null) _skillButton.onClick.AddListener(OnSkillClicked);
        }

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnUnitSelected -= HandleUnitSelected;
                _turnManager.OnTurnChanged -= HandleTurnChanged;
            }
        }

        private void HandleUnitSelected(CombatUnit unit)
        {
            if (_currentUnit != null)
            {
                _currentUnit.OnStatsChanged -= RefreshButtons;
            }

            _currentUnit = unit;

            if (_currentUnit != null)
            {
                _currentUnit.OnStatsChanged += RefreshButtons;
            }

            RefreshButtons(_currentUnit);
        }

        private void HandleTurnChanged(CombatTurnPhase phase, int round)
        {
            bool isPlayerPhase = (phase == CombatTurnPhase.PlayerTurn);

            if (_endTurnButton != null) _endTurnButton.interactable = isPlayerPhase;
            if (_defendButton != null) _defendButton.interactable = isPlayerPhase;
            if (_attackModeButton != null) _attackModeButton.interactable = isPlayerPhase;
            if (_skillButton != null) _skillButton.interactable = isPlayerPhase;

            if (isPlayerPhase)
            {
                RefreshButtons(_currentUnit);
            }
        }

        private void RefreshButtons(CombatUnit unit)
        {
            if (unit == null || unit.IsDead || _turnManager.CurrentState != CombatState.PlayerTurn)
            {
                SetActionButtonsInteractable(false);
                return;
            }

            // Stamina maliyeti kontrolleri
            int attackCost = unit.Data != null ? unit.Data.StaminaCostAttack : 2;
            int defendCost = unit.Data != null ? unit.Data.StaminaCostDefend : 1;

            if (_attackModeButton != null) _attackModeButton.interactable = (unit.CurrentStamina >= attackCost);
            if (_defendButton != null) _defendButton.interactable = (unit.CurrentStamina >= defendCost && !unit.IsDefending);
            if (_skillButton != null) _skillButton.interactable = (unit.CurrentStamina >= 3);
        }

        private void SetActionButtonsInteractable(bool interactable)
        {
            if (_attackModeButton != null) _attackModeButton.interactable = interactable;
            if (_defendButton != null) _defendButton.interactable = interactable;
            if (_skillButton != null) _skillButton.interactable = interactable;
        }

        private void OnEndTurnClicked()
        {
            _turnManager?.EndPlayerTurn();
        }

        private void OnDefendClicked()
        {
            if (_currentUnit != null && _currentUnit.IsPlayerControlled)
            {
                _currentUnit.Defend();
            }
        }

        private void OnAttackModeClicked()
        {
            // Hibrit Alp icin Kilic <-> Yay gecisi veya saldiri hazirligi
            Debug.Log("[ActionCluster] Saldiri modu / Silah degistirildi.");
        }

        private void OnSkillClicked()
        {
            // Ozel Taktik Hamle / Nara At (Morale Boost)
            Debug.Log("[ActionCluster] Ozel Yetenek kullanildi!");
        }
    }
}
