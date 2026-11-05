using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Combat;
using Kervan.Core;

namespace Kervan.UI
{
    /// <summary>
    /// GDD §2.A standardında Sol Başparmak Bölgesi: Aktif Birim Bilgi Kartı (Unit HUD).
    /// Portre, Türkçe sınıf/mod bilgisi, Can/Zırh barları, 5'li Stamina yıldızları ve Moral hilalini yönetir.
    /// </summary>
    public class UnitHUD : MonoBehaviour
    {
        [Header("Kimlik")]
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _roleText;

        [Header("Can (HP) Barı - Koyu Al #8B261E")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Image _healthFillImage;
        [SerializeField] private TextMeshProUGUI _healthValueText;

        [Header("Zırh Barı - Çelik Gri #4A4E51")]
        [SerializeField] private Slider _armorSlider;
        [SerializeField] private Image _armorFillImage;
        [SerializeField] private TextMeshProUGUI _armorValueText;

        [Header("Stamina Hücreleri - Kehribar Sarısı #DAA520")]
        [SerializeField] private List<Image> _staminaDots = new List<Image>();

        [Header("Moral Hilal Göstergesi")]
        [SerializeField] private Image _moraleIcon;
        [SerializeField] private TextMeshProUGUI _moraleValueText;

        [Header("Yöneticiler")]
        [SerializeField] private CombatTurnManager _turnManager;

        private CombatUnit _boundUnit;

        // GDD §2 Renk Kodları
        private static readonly Color ColorMoraleHigh = new Color(0.18f, 0.49f, 0.20f, 1f);     // #2E7D32 Yüksek Moral Yeşili
        private static readonly Color ColorMoraleNormal = new Color(0.95f, 0.90f, 0.81f, 1f);   // #F2E6CE Parşömen Kremi
        private static readonly Color ColorMoralePanic = new Color(0.72f, 0.11f, 0.11f, 1f);    // #B71C1C Panik Kırmızısı
        private static readonly Color ColorStaminaActive = new Color(0.855f, 0.647f, 0.125f, 1f); // #DAA520 Kehribar Altını
        private static readonly Color ColorStaminaSpent = new Color(0.29f, 0.30f, 0.32f, 0.38f);  // Sönük Gri

        private void Awake()
        {
            if (_turnManager == null)
            {
                _turnManager = FindAnyObjectByType<CombatTurnManager>();
            }

            if (_turnManager != null)
            {
                _turnManager.OnUnitSelected += BindUnit;
            }
        }

        private void Start()
        {
            if (_turnManager != null && _turnManager.SelectedUnit != null)
            {
                BindUnit(_turnManager.SelectedUnit);
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

            // 1. İsim ve Türkçe Rol / Aktif Silah Modu
            if (_unitNameText != null)
            {
                _unitNameText.text = unit.Data.UnitName;
            }

            if (_roleText != null)
            {
                string modeLabel = GetRoleAndWeaponLabel(unit);
                if (unit.IsDefending)
                {
                    modeLabel += " · <color=#008B9B>[Siperde]</color>";
                }
                else if (unit.IsRouting)
                {
                    modeLabel += " · <color=#B71C1C>[Panik!]</color>";
                }
                _roleText.text = modeLabel;
            }

            // 2. Portre
            if (_portraitImage != null)
            {
                _portraitImage.sprite = unit.Data.Portrait != null ? unit.Data.Portrait : unit.Data.DefaultSprite;
                _portraitImage.preserveAspect = true;
            }

            // 3. Can (HP) Barı
            int maxHp = Mathf.Max(1, unit.Data.MaxHealth);
            if (_healthSlider != null)
            {
                _healthSlider.maxValue = maxHp;
                _healthSlider.value = unit.CurrentHealth;
            }
            if (_healthFillImage != null)
            {
                _healthFillImage.fillAmount = Mathf.Clamp01((float)unit.CurrentHealth / maxHp);
            }
            if (_healthValueText != null)
            {
                _healthValueText.text = $"{unit.CurrentHealth} / {maxHp}";
            }

            // 4. Zırh Barı
            int maxArmor = Mathf.Max(1, unit.Data.MaxArmor);
            if (_armorSlider != null)
            {
                _armorSlider.maxValue = maxArmor;
                _armorSlider.value = unit.CurrentArmor;
            }
            if (_armorFillImage != null)
            {
                _armorFillImage.fillAmount = unit.Data.MaxArmor > 0
                    ? Mathf.Clamp01((float)unit.CurrentArmor / maxArmor)
                    : 0f;
            }
            if (_armorValueText != null)
            {
                _armorValueText.text = $"{unit.CurrentArmor} / {unit.Data.MaxArmor}";
            }

            // 5. Stamina Hücreleri (5 adet Selçuklu Yıldızı)
            for (int i = 0; i < _staminaDots.Count; i++)
            {
                if (_staminaDots[i] != null)
                {
                    bool isActive = (i < unit.CurrentStamina);
                    _staminaDots[i].color = isActive ? ColorStaminaActive : ColorStaminaSpent;
                }
            }

            // 6. Moral Hilal Göstergesi
            if (_moraleIcon != null)
            {
                if (unit.IsRouting || unit.CurrentMorale < 35)
                {
                    _moraleIcon.color = ColorMoralePanic;
                }
                else if (unit.CurrentMorale >= 90)
                {
                    _moraleIcon.color = ColorMoraleHigh;
                }
                else
                {
                    _moraleIcon.color = ColorMoraleNormal;
                }
            }

            if (_moraleValueText != null)
            {
                _moraleValueText.text = $"Moral: {unit.CurrentMorale}";
            }
        }

        private static string GetRoleAndWeaponLabel(CombatUnit unit)
        {
            switch (unit.Data.CombatRole)
            {
                case UnitCombatRole.Hybrid:
                    return unit.IsUsingRangedWeapon
                        ? $"Yay Modu (Menzil: {unit.ActiveAttackRange})"
                        : "Kılınç Modu (Yakın Dövüş)";
                case UnitCombatRole.Ranged:
                    return $"Okçu · Menzil: {unit.ActiveAttackRange}";
                default:
                    return "Ağır Piyade · Kalkan & Kılıç";
            }
        }
    }
}
