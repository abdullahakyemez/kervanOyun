using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Combat;

namespace Kervan.UI
{
    /// <summary>
    /// EKRAN.md standardinda Sol Basparmak Bolgesi: Aktif Birim Bilgi Karti (Unit HUD).
    /// Portre, HP/Zirh barlari, 5'li Stamina hucreleri ve Moral hilalini yonetir.
    /// </summary>
    public class UnitHUD : MonoBehaviour
    {
        [Header("Kimlik")]
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _roleText;

        [Header("Can (HP) Bari - Koyu Al #8B261E")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TextMeshProUGUI _healthValueText;

        [Header("Zirh Bari - Celik Gri #4A4E51")]
        [SerializeField] private Slider _armorSlider;
        [SerializeField] private TextMeshProUGUI _armorValueText;

        [Header("Stamina Hucreleri - Kehribar Sarisi #DAA520")]
        [SerializeField] private List<Image> _staminaDots = new List<Image>();

        [Header("Moral Hilal Gostergesi")]
        [SerializeField] private Image _moraleIcon;

        [Header("Yoneticiler")]
        [SerializeField] private CombatTurnManager _turnManager;

        private CombatUnit _boundUnit;

        // TASARİM.md Renk Kodlari
        private static readonly Color ColorMoraleHigh = new Color(0.165f, 0.294f, 0.216f); // #2A4B37 Yesil
        private static readonly Color ColorMoraleNormal = Color.white;
        private static readonly Color ColorMoralePanic = new Color(0.545f, 0.149f, 0.118f); // #8B261E Koyu Al
        private static readonly Color ColorStaminaActive = new Color(0.855f, 0.647f, 0.125f); // #DAA520 Kehribar
        private static readonly Color ColorStaminaSpent = new Color(0.29f, 0.30f, 0.32f, 0.40f); // Sonuk Gri

        private void Start()
        {
            if (_turnManager == null) _turnManager = FindAnyObjectByType<CombatTurnManager>();
            if (_turnManager != null)
            {
                _turnManager.OnUnitSelected += BindUnit;
            }
        }

        public void BindUnit(CombatUnit unit)
        {
            if (_boundUnit != null)
            {
                _boundUnit.OnStatsChanged -= UpdateDisplay;
            }

            _boundUnit = unit;

            if (_boundUnit != null)
            {
                _boundUnit.OnStatsChanged += UpdateDisplay;
                gameObject.SetActive(true);
                UpdateDisplay(_boundUnit);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnUnitSelected -= BindUnit;
            }

            if (_boundUnit != null)
            {
                _boundUnit.OnStatsChanged -= UpdateDisplay;
            }
        }

        private void UpdateDisplay(CombatUnit unit)
        {
            if (unit == null || unit.Data == null) return;

            // 1. Isim ve Rol
            if (_unitNameText != null) _unitNameText.text = unit.Data.UnitName;
            if (_roleText != null) _roleText.text = unit.Data.CombatRole.ToString();

            // 2. Portre
            if (_portraitImage != null)
            {
                _portraitImage.sprite = unit.Data.Portrait != null ? unit.Data.Portrait : unit.Data.DefaultSprite;
            }

            // 3. Can (HP) Bari
            if (_healthSlider != null)
            {
                _healthSlider.maxValue = unit.Data.MaxHealth;
                _healthSlider.value = unit.CurrentHealth;
            }
            if (_healthValueText != null)
            {
                _healthValueText.text = $"{unit.CurrentHealth} / {unit.Data.MaxHealth}";
            }

            // 4. Zirh Bari
            if (_armorSlider != null)
            {
                _armorSlider.maxValue = Mathf.Max(1, unit.Data.MaxArmor);
                _armorSlider.value = unit.CurrentArmor;
            }
            if (_armorValueText != null)
            {
                _armorValueText.text = $"{unit.CurrentArmor} / {unit.Data.MaxArmor}";
            }

            // 5. Stamina Hucreleri (5 adet)
            for (int i = 0; i < _staminaDots.Count; i++)
            {
                if (_staminaDots[i] != null)
                {
                    bool isActive = (i < unit.CurrentStamina);
                    _staminaDots[i].color = isActive ? ColorStaminaActive : ColorStaminaSpent;
                }
            }

            // 6. Moral Hilal Gostergesi
            if (_moraleIcon != null)
            {
                if (unit.IsRouting || unit.CurrentMorale < 30)
                {
                    _moraleIcon.color = ColorMoralePanic;
                }
                else if (unit.CurrentMorale > 100)
                {
                    _moraleIcon.color = ColorMoraleHigh;
                }
                else
                {
                    _moraleIcon.color = ColorMoraleNormal;
                }
            }
        }
    }
}
