using UnityEngine;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    public enum NodeType
    {
        City,           // Sehir: Pazar, idareciler, casusluk ve gorevler
        Caravanserai,   // Kervansaray: Guvenli dinlenme, iyilesme ve erzak ikmali
        Outpost         // Uc Karakolu: Muhafiz kiralama ve sinir guvenligi
    }

    /// <summary>
    /// Harita uzerindeki bir cografi dugumu (Sehir veya Kervansaray)
    /// tanimlayan ScriptableObject veri modeli.
    /// </summary>
    [CreateAssetMenu(fileName = "Node_New", menuName = "Kervan/Travel/Location Node", order = 1)]
    public class LocationNodeSO : ScriptableObject
    {
        [Header("Temel Kimlik")]
        [SerializeField] private string _nodeId;
        [SerializeField] private string _nodeName;
        [SerializeField] private NodeType _nodeType = NodeType.City;
        [SerializeField] private FactionType _rulingFaction = FactionType.Ottoman;
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("Tesisler & Ekonomi")]
        [SerializeField] private bool _hasMarket = true;
        [SerializeField] private bool _hasInn = true;
        [Tooltip("Kervansarayda gunluk konaklama/iyilesme masrafi (Akce)")]
        [SerializeField] private int _dailyRestCost = 15;
        [Tooltip("Gunluk iyilesme orani (Can puani)")]
        [SerializeField] private int _dailyHealAmount = 20;
        [Tooltip("Gunluk moral tazeleme puani")]
        [SerializeField] private int _dailyMoraleRestore = 15;

        [Header("Harita Konumu (2D)")]
        [SerializeField] private Vector2 _mapCoordinates;
        [SerializeField] private Sprite _nodeIcon;

        // Getters
        public string NodeId => _nodeId;
        public string NodeName => _nodeName;
        public NodeType NodeType => _nodeType;
        public FactionType RulingFaction => _rulingFaction;
        public string Description => _description;

        public bool HasMarket => _hasMarket;
        public bool HasInn => _hasInn;
        public int DailyRestCost => _dailyRestCost;
        public int DailyHealAmount => _dailyHealAmount;
        public int DailyMoraleRestore => _dailyMoraleRestore;

        public Vector2 MapCoordinates => _mapCoordinates;
        public Sprite NodeIcon => _nodeIcon;
    }
}
