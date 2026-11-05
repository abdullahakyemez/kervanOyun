using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Combat;
using Kervan.Core;

namespace Kervan.UI
{
    /// <summary>
    /// GDD §2.B standardında Sağ Başparmak Bölgesi: Eylem Komuta Kümesi.
    /// "Turu Bitir", "Kılıç/Yay Mod Değiştir", "Siper Al" ve "Nara At (Özel Yetenek)" düğmelerini yönetir.
    /// </summary>
    public class ActionCluster : MonoBehaviour
    {
        [Header("Yöneticiler")]
        [SerializeField] private CombatTurnManager _turnManager;

        [Header("Birincil Eylem")]
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private TextMeshProUGUI _endTurnText;

        [Header("İkincil Eylemler (3 Madalyon Düğme)")]
        [SerializeField] private Button _attackModeButton;
        [SerializeField] private TextMeshProUGUI _attackModeText;
        [SerializeField] private Button _defendButton;
        [SerializeField] private TextMeshProUGUI _defendText;
        [SerializeField] private Button _skillButton;
        [SerializeField] private TextMeshProUGUI _skillText;

        private CombatUnit _currentUnit;

        private void Awake()
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

        private void Start()
        {
            if (_turnManager != null && _turnManager.SelectedUnit != null)
            {
                HandleUnitSelected(_turnManager.SelectedUnit);
            }
        }

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnUnitSelected -= HandleUnitSelected;
                _turnManager.OnTurnChanged -= HandleTurnChanged;
            }

            if (_currentUnit != null)
            {
                _currentUnit.OnStatsChanged -= RefreshButtons;
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
            if (_endTurnText != null)
            {
                _endTurnText.text = isPlayerPhase ? "TURU BİTİR" : "DÜŞMAN TURU";
            }

            if (isPlayerPhase)
            {
                RefreshButtons(_currentUnit);
            }
            else
            {
                SetActionButtonsInteractable(false);
            }
        }

        private void RefreshButtons(CombatUnit unit)
        {
            if (unit == null || unit.IsDead || _turnManager == null || _turnManager.CurrentState != CombatState.PlayerTurn)
            {
                SetActionButtonsInteractable(false);
                return;
            }

            bool isHybrid = unit.Data != null && unit.Data.CombatRole == UnitCombatRole.Hybrid;
            int defendCost = unit.Data != null ? unit.Data.StaminaCostDefend : 1;
            const int skillCost = 3;

            // 1. Kılıç <-> Yay Mod Değiştirme (Yalnızca Hibrit Alp için aktif)
            if (_attackModeButton != null)
            {
                _attackModeButton.interactable = isHybrid && !unit.IsRouting;
            }
            if (_attackModeText != null)
            {
                if (isHybrid)
                {
                    _attackModeText.text = unit.IsUsingRangedWeapon ? "Yay" : "Kılıç";
                }
                else
                {
                    _attackModeText.text = "Sabit";
                }
            }

            // 2. Siper Al (1 Stamina)
            if (_defendButton != null)
            {
                _defendButton.interactable = (unit.CurrentStamina >= defendCost && !unit.IsDefending && !unit.IsRouting);
            }
            if (_defendText != null)
            {
                _defendText.text = unit.IsDefending ? "Siperde" : "Siper";
            }

            // 3. Nara At / Moral Hamlesi (3 Stamina)
            if (_skillButton != null)
            {
                _skillButton.interactable = (unit.CurrentStamina >= skillCost && !unit.IsRouting);
            }
            if (_skillText != null)
            {
                _skillText.text = "Nara";
            }
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
            if (_currentUnit != null && _currentUnit.IsPlayerControlled && _turnManager != null)
            {
                _currentUnit.Defend();
                // Kalan stamina ile menzil halkalarını yenile
                _turnManager.SelectUnit(_currentUnit);
            }
        }

        private void OnAttackModeClicked()
        {
            if (_turnManager != null)
            {
                _turnManager.ToggleSelectedUnitWeaponMode();
                RefreshButtons(_currentUnit);
            }
        }

        private void OnSkillClicked()
        {
            if (_turnManager != null)
            {
                _turnManager.ExecuteSelectedUnitSkill();
                RefreshButtons(_currentUnit);
            }
        }
    }
}
