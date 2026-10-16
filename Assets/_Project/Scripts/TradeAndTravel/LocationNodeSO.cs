using UnityEngine;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    public enum NodeType
    {
        City,           // Şehir: Pazar, idareciler, casusluk ve görevler
        Caravanserai,   // Kervansaray: Güvenli dinlenme, iyileşme ve erzak ikmali
        Outpost         // Uç Karakolu: Muhafız kiralama ve sınır güvenliği
    }

    /// <summary>
    /// Harita üzerindeki bir coğrafi düğümü (Şehir veya Kervansaray)
    /// tanımlayan ScriptableObject veri modeli.
    /// </summary>
    [CreateAssetMenu(fileName = "Node_New", menuName = "Kervan/Travel/Location Node", order = 1)]
    public class LocationNodeSO : ScriptableObject
    {
        [Header("Temel Kimlik")]
        [Tooltip("Benzersiz ASCII küçük harfli düğüm kimliği (ör. node_sogut)")]
        [SerializeField] private string _nodeId;
        [Tooltip("Oyuncuya gösterilecek Türkçe şehir/han adı (ör. Söğüt)")]
        [SerializeField] private string _nodeName;
        [SerializeField] private NodeType _nodeType = NodeType.City;
        [SerializeField] private FactionType _rulingFaction = FactionType.Ottoman;
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("Tesisler & Ekonomi")]
        [SerializeField] private bool _hasMarket = true;
        [SerializeField] private bool _hasInn = true;
        [Tooltip("Bilgi kartında gösterilecek başlıca ticaret malları özeti (ör. 'Pazar: İpek, Çelik, Kürk')")]
        [SerializeField] private string _marketGoodsSummary = "Pazar: Çeşitli Emtia";
        [Tooltip("Kervansarayda günlük konaklama/iyileşme masrafı (Akçe)")]
        [SerializeField] private int _dailyRestCost = 15;
        [Tooltip("Günlük iyileşme oranı (Can puanı)")]
        [SerializeField] private int _dailyHealAmount = 20;
        [Tooltip("Günlük moral tazeleme puanı")]
        [SerializeField] private int _dailyMoraleRestore = 15;

        [Header("Harita Konumu (2D)")]
        [SerializeField] private Vector2 _mapCoordinates;
        [SerializeField] private Sprite _nodeIcon;

        // Özellikler (Getters)
        public string NodeId => _nodeId;
        public string NodeName => _nodeName;
        public NodeType NodeType => _nodeType;
        public FactionType RulingFaction => _rulingFaction;
        public string Description => _description;

        public bool HasMarket => _hasMarket;
        public bool HasInn => _hasInn;
        public string MarketGoodsSummary => !string.IsNullOrWhiteSpace(_marketGoodsSummary) ? _marketGoodsSummary : "Pazar: Çeşitli Emtia";
        public int DailyRestCost => _dailyRestCost;
        public int DailyHealAmount => _dailyHealAmount;
        public int DailyMoraleRestore => _dailyMoraleRestore;

        public Vector2 MapCoordinates => _mapCoordinates;
        public Sprite NodeIcon => _nodeIcon;

        /// <summary>
        /// Düğüme hâkim olan fraksiyonun kullanıcı arayüzünde gösterilecek Türkçe adını döndürür.
        /// </summary>
        public string GetFactionDisplayName()
        {
            switch (_rulingFaction)
            {
                case FactionType.Ottoman:
                    return "Osmanlı";
                case FactionType.Byzantine:
                    return "Bizans";
                case FactionType.Bandit:
                    return "Eşkıya";
                case FactionType.Steppe:
                case FactionType.SeljukLegacy:
                    return "Anadolu Beyliği";
                default:
                    return "Osmanlı";
            }
        }
    }
}
