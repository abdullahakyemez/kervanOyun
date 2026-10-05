using UnityEngine;
using Kervan.Core;

namespace Kervan.Combat
{
    /// <summary>
    /// Savas birimlerinin (Alp, Okcu, Muhafiz, Haydut) temel stat ve gorsel verilerini
    /// Unity Editor uzerinden duzenlemeyi saglayan ScriptableObject sablonu.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitData_New", menuName = "Kervan/Combat/Unit Data", order = 1)]
    public class UnitDataSO : ScriptableObject
    {
        [Header("Kimlik & Sinif")]
        [SerializeField] private string _unitId;
        [SerializeField] private string _unitName;
        [SerializeField] private UnitCombatRole _combatRole = UnitCombatRole.Melee;
        [SerializeField] private FactionType _faction = FactionType.Ottoman;
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("Temel Savunma ve Hayatta Kalma")]
        [Tooltip("Azami Can Degeri")]
        [SerializeField] private int _maxHealth = 100;
        [Tooltip("Azami Zirh Degeri (Darbeleri sonumler)")]
        [SerializeField] private int _maxArmor = 30;
        [Tooltip("Maksimum Dayaniklilik Puani (Hucresel gosterge, standart 5)")]
        [SerializeField] private int _maxStamina = 5;
        [Tooltip("Temel Moral Puani (Kritik altina duserse panik/kacis baslar)")]
        [SerializeField] private int _baseMorale = 100;

        [Header("Saldiri Nitelikleri")]
        [SerializeField] private int _minDamage = 12;
        [SerializeField] private int _maxDamage = 18;
        [Tooltip("Yakin dovus altigen erisimi (standart 1)")]
        [SerializeField] private int _meleeRange = 1;
        [Tooltip("Menzilli saldiri erisimi (Okcular ve yay modundaki Alp icin)")]
        [SerializeField] private int _rangedRange = 3;
        [Range(0f, 1f)]
        [Tooltip("Temel vurus isabet sansi (0.0 - 1.0)")]
        [SerializeField] private float _baseAccuracy = 0.85f;

        [Header("Stamina Harcama Maliyetleri")]
        [SerializeField] private int _staminaCostMovePerTile = 1;
        [SerializeField] private int _staminaCostAttack = 2;
        [SerializeField] private int _staminaCostDefend = 1;

        [Header("Gorsel & Arayuz Varliklari")]
        [SerializeField] private Sprite _portrait;
        [SerializeField] private Sprite _defaultSprite;
        [SerializeField] private GameObject _unitPrefab;

        // Getters
        public string UnitId => _unitId;
        public string UnitName => _unitName;
        public UnitCombatRole CombatRole => _combatRole;
        public FactionType Faction => _faction;
        public string Description => _description;

        public int MaxHealth => _maxHealth;
        public int MaxArmor => _maxArmor;
        public int MaxStamina => _maxStamina;
        public int BaseMorale => _baseMorale;

        public int MinDamage => _minDamage;
        public int MaxDamage => _maxDamage;
        public int MeleeRange => _meleeRange;
        public int RangedRange => _rangedRange;
        public float BaseAccuracy => _baseAccuracy;

        public int StaminaCostMovePerTile => _staminaCostMovePerTile;
        public int StaminaCostAttack => _staminaCostAttack;
        public int StaminaCostDefend => _staminaCostDefend;

        public Sprite Portrait => _portrait;
        public Sprite DefaultSprite => _defaultSprite;
        public GameObject UnitPrefab => _unitPrefab;
    }
}
