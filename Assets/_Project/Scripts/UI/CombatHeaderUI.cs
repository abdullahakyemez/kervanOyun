using UnityEngine;
using TMPro;
using Kervan.Combat;
using Kervan.Core;

namespace Kervan.UI
{
    /// <summary>
    /// EKRAN.md standardinda Ust Guvenli Bolge: Tur/Faz Bilgisi ve Geri Cekilme Paneli.
    /// </summary>
    public class CombatHeaderUI : MonoBehaviour
    {
        [Header("Yoneticiler")]
        [SerializeField] private CombatTurnManager _turnManager;

        [Header("Gostergeler")]
        [SerializeField] private TextMeshProUGUI _turnTitleText;
        [SerializeField] private TextMeshProUGUI _phaseSubText;

        [Header("Geri Cekil")]
        [SerializeField] private HoldButton _retreatButton;

        private void Start()
        {
            if (_turnManager == null)
            {
                _turnManager = FindAnyObjectByType<CombatTurnManager>();
            }

            if (_turnManager != null)
            {
                _turnManager.OnTurnChanged += HandleTurnChanged;
            }

            if (_retreatButton != null)
            {
                _retreatButton.OnHoldComplete += HandleRetreatTriggered;
            }

            UpdateDisplay(CombatTurnPhase.PlayerTurn, 1);
        }

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnTurnChanged -= HandleTurnChanged;
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
                    _phaseSubText.color = new Color(0.165f, 0.45f, 0.25f); // Yesil
                }
                else
                {
                    _phaseSubText.text = "Düşman Hamlesi...";
                    _phaseSubText.color = new Color(0.70f, 0.20f, 0.15f); // Kirmizi
                }
            }
        }

        private void HandleRetreatTriggered()
        {
            Debug.Log("[CombatHeaderUI] Kervan savas alanindan geri cekildi / teslim oldu!");
            // Kervan geri cekilme / kacak durumunu tetikle
        }
    }
}
