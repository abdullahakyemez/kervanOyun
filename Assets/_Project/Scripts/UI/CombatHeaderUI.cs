using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Kervan.Combat;
using Kervan.Core;

namespace Kervan.UI
{
    /// <summary>
    /// GDD §2.C standardında Üst Güvenli Bölge: Tur/Faz Bilgisi ve 1.5s Basılı Tutmalı Geri Çekilme Paneli.
    /// </summary>
    public class CombatHeaderUI : MonoBehaviour
    {
        [Header("Yöneticiler")]
        [SerializeField] private CombatTurnManager _turnManager;

        [Header("Göstergeler")]
        [SerializeField] private TextMeshProUGUI _turnTitleText;
        [SerializeField] private TextMeshProUGUI _phaseSubText;

        [Header("Geri Çekil (Basılı Tut)")]
        [SerializeField] private HoldButton _retreatButton;

        private static readonly Color ColorPlayerPhase = new Color(0.85f, 0.70f, 0.35f, 1f); // #D9B359 Antik Altın
        private static readonly Color ColorEnemyPhase = new Color(0.85f, 0.32f, 0.28f, 1f);  // Tehlike Kırmızısı
        private static readonly Color ColorVictoryPhase = new Color(0.28f, 0.78f, 0.42f, 1f); // Zafer Yeşili

        private void Awake()
        {
            if (_turnManager == null)
            {
                _turnManager = FindAnyObjectByType<CombatTurnManager>();
            }

            if (_turnManager != null)
            {
                _turnManager.OnTurnChanged += HandleTurnChanged;
                _turnManager.OnCombatEnded += HandleCombatEnded;
            }

            if (_retreatButton != null)
            {
                _retreatButton.OnHoldComplete += HandleRetreatTriggered;
            }
        }

        private void Start()
        {
            int round = _turnManager != null ? _turnManager.RoundNumber : 1;
            CombatTurnPhase phase = (_turnManager != null && _turnManager.CurrentState == CombatState.EnemyTurn)
                ? CombatTurnPhase.EnemyTurn
                : CombatTurnPhase.PlayerTurn;
            UpdateDisplay(phase, round);
        }

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnTurnChanged -= HandleTurnChanged;
                _turnManager.OnCombatEnded -= HandleCombatEnded;
            }

            if (_retreatButton != null)
            {
                _retreatButton.OnHoldComplete -= HandleRetreatTriggered;
            }
        }

        private void HandleTurnChanged(CombatTurnPhase phase, int round)
        {
            UpdateDisplay(phase, round);
        }

        private void HandleCombatEnded(bool isVictory)
        {
            if (_turnTitleText != null)
            {
                _turnTitleText.text = isVictory ? "ZAFER!" : "BOZGUN!";
            }

            if (_phaseSubText != null)
            {
                _phaseSubText.text = isVictory ? "Eşkıyalar Püskürtüldü" : "Kervan Savunması Yarıldı";
                _phaseSubText.color = isVictory ? ColorVictoryPhase : ColorEnemyPhase;
            }

            if (_retreatButton != null)
            {
                _retreatButton.gameObject.SetActive(false);
            }
        }

        private void UpdateDisplay(CombatTurnPhase phase, int round)
        {
            if (_turnTitleText != null)
            {
                _turnTitleText.text = $"{round}. TUR";
            }

            if (_phaseSubText != null)
            {
                if (phase == CombatTurnPhase.PlayerTurn)
                {
                    _phaseSubText.text = "Alp & Muhafız Fazı";
                    _phaseSubText.color = ColorPlayerPhase;
                }
                else
                {
                    _phaseSubText.text = "Düşman Hamlesi...";
                    _phaseSubText.color = ColorEnemyPhase;
                }
            }
        }

        private void HandleRetreatTriggered()
        {
            Debug.Log("[CombatHeaderUI] Kervan savaş alanından geri çekildi!");
            if (_turnManager != null)
            {
                _turnManager.ApplyRetreatPenaltyToSession();
            }
            SceneLoader.Load(SceneIds.Travel);
        }
    }
}
