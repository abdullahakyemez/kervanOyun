using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Combat;

namespace Kervan.UI
{
    /// <summary>
    /// Savasin zafer veya yenilgiyle sonuclanmasi durumunda
    /// acilan sonuc ve ganimet paneli.
    /// </summary>
    public class CombatResultUI : MonoBehaviour
    {
        [Header("Yoneticiler")]
        [SerializeField] private CombatTurnManager _turnManager;

        [Header("UI Paneli")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _continueButton;

        private void Start()
        {
            if (_turnManager == null)
            {
                _turnManager = FindAnyObjectByType<CombatTurnManager>();
            }

            if (_turnManager != null)
            {
                _turnManager.OnCombatEnded += HandleCombatEnded;
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnCombatEnded -= HandleCombatEnded;
            }
        }

        private void HandleCombatEnded(bool isVictory)
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            if (isVictory)
            {
                if (_titleText != null)
                {
                    _titleText.text = "ZAFER!";
                    _titleText.color = new Color(0.165f, 0.45f, 0.25f); // Sancak Yesili
                }

                if (_descriptionText != null)
                {
                    _descriptionText.text = "Eşkıyalar püskürtüldü! Kervan ve yükler emniyete alındı.\nKazanılan Ganimet: +80 Akçe, +5 Günlük Erzak.";
                }
            }
            else
            {
                if (_titleText != null)
                {
                    _titleText.text = "YENİLGİ!";
                    _titleText.color = new Color(0.55f, 0.15f, 0.12f); // Koyu Al
                }

                if (_descriptionText != null)
                {
                    _descriptionText.text = "Kervan muhafızları savunmayı kaybetti!\nYüklerin bir kısmı yağmalandı ve kervan en yakın han veya kervansaraya sığınmak zorunda kaldı.";
                }
            }
        }

        private void OnContinueClicked()
        {
            Debug.Log("[CombatResultUI] Savas tamamlandi, harita/seyahat ekranina donuluyor.");
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }
    }
}
