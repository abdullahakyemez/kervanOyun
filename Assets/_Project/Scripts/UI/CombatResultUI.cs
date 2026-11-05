using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Combat;
using Kervan.Core;

namespace Kervan.UI
{
    /// <summary>
    /// Savaşın zafer veya yenilgiyle sonuçlanması durumunda
    /// açılan sonuç, zayiat ve ganimet paneli (A5.4).
    /// </summary>
    public class CombatResultUI : MonoBehaviour
    {
        [Header("Yöneticiler")]
        [SerializeField] private CombatTurnManager _turnManager;

        [Header("UI Paneli")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _continueButton;

        private bool _isSubscribed;
        private bool _isResultShown;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Start()
        {
            EnsureInitialized();

            if (!_isResultShown && _panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void EnsureInitialized()
        {
            if (_panelRoot == null)
            {
                _panelRoot = gameObject;
            }

            if (_turnManager == null)
            {
                _turnManager = FindAnyObjectByType<CombatTurnManager>();
            }

            if (_turnManager != null && !_isSubscribed)
            {
                _turnManager.OnCombatEnded += HandleCombatEnded;
                _isSubscribed = true;
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(OnContinueClicked);
                _continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void OnDestroy()
        {
            if (_turnManager != null && _isSubscribed)
            {
                _turnManager.OnCombatEnded -= HandleCombatEnded;
                _isSubscribed = false;
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(OnContinueClicked);
            }
        }

        private void HandleCombatEnded(bool isVictory)
        {
            string summary = _turnManager != null ? _turnManager.LastOutcomeSummary : null;
            ShowCombatResult(isVictory, summary);
        }

        /// <summary>
        /// Savaş sonuç ekranını görünür kılar ve gerçek ganimet/kayıp özetini doldurur.
        /// </summary>
        public void ShowCombatResult(bool isVictory, string customSummary = null)
        {
            EnsureInitialized();
            _isResultShown = true;

            gameObject.SetActive(true);
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            transform.SetAsLastSibling();

            if (isVictory)
            {
                if (_titleText != null)
                {
                    _titleText.text = "ZAFER!";
                    _titleText.color = new Color(0.85f, 0.70f, 0.35f, 1f); // Antik Altın (#D9B359)
                }

                if (_descriptionText != null)
                {
                    _descriptionText.text = !string.IsNullOrWhiteSpace(customSummary)
                        ? customSummary
                        : "Eşkıyalar püskürtüldü! Kervan ve yükler emniyete alındı.";
                }
            }
            else
            {
                if (_titleText != null)
                {
                    _titleText.text = "BOZGUN!";
                    _titleText.color = new Color(0.85f, 0.28f, 0.24f, 1f); // Koyu Al
                }

                if (_descriptionText != null)
                {
                    _descriptionText.text = !string.IsNullOrWhiteSpace(customSummary)
                        ? customSummary
                        : "Kervan muhafızları savunmayı kaybetti! Yüklerin bir kısmı yağmalandı.";
                }
            }
        }

        private void OnContinueClicked()
        {
            Debug.Log("[CombatResultUI] Savaş tamamlandı, seyahat ekranına dönülüyor.");
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
            SceneLoader.Load(SceneIds.Travel);
        }
    }
}
