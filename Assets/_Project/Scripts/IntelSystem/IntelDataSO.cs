using UnityEngine;
using Kervan.Core;

namespace Kervan.IntelSystem
{
    /// <summary>
    /// Şehirlerde, hanlarda ve pazarlarda toplanan tarihsel istihbarat / casusluk verisi.
    /// </summary>
    [CreateAssetMenu(fileName = "Intel_", menuName = "Kervan/İstihbarat/İstihbarat Verisi")]
    public class IntelDataSO : ScriptableObject
    {
        [Header("Kimlik ve Tanım")]
        [SerializeField] private string _intelId;
        [SerializeField] private string _title;
        [TextArea(3, 6)]
        [SerializeField] private string _description;
        [SerializeField] private IntelType _intelType;

        [Header("Coğrafya ve Hedef")]
        [SerializeField] private string _sourceCity;          // İstihbaratın toplandığı şehir (Örn: Bilecik)
        [SerializeField] private FactionType _targetFaction;   // Bu istihbaratı ödüllendirecek beylik (Örn: Ottoman)

        [Header("Risk ve Zorluk")]
        [Range(0, 100)]
        [SerializeField] private int _discoveryRisk = 25;     // Bilgi toplarken yakalanma riski %

        [Header("Teslimat Ödülleri (Hand-in Rewards)")]
        [SerializeField] private int _rewardAkce = 150;
        [SerializeField] private int _rewardReputation = 15;
        [SerializeField] private float _taxDiscountPercent = 2f; // Vergi indirimi / berat katkısı

        public string IntelId => _intelId;
        public string Title => _title;
        public string Description => _description;
        public IntelType IntelType => _intelType;
        public string SourceCity => _sourceCity;
        public FactionType TargetFaction => _targetFaction;
        public int DiscoveryRisk => _discoveryRisk;
        public int RewardAkce => _rewardAkce;
        public int RewardReputation => _rewardReputation;
        public float TaxDiscountPercent => _taxDiscountPercent;

        public void Init(string id, string title, string desc, IntelType type, string sourceCity, FactionType targetFaction, int risk, int rewardAkce, int rewardRep, float taxDiscount)
        {
            _intelId = id;
            _title = title;
            _description = desc;
            _intelType = type;
            _sourceCity = sourceCity;
            _targetFaction = targetFaction;
            _discoveryRisk = risk;
            _rewardAkce = rewardAkce;
            _rewardReputation = rewardRep;
            _taxDiscountPercent = taxDiscount;
        }
    }
}
