using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;
using Kervan.Combat;

namespace Kervan.TradeAndTravel
{
    public enum RoadQuality
    {
        PavedRomanRoad, // Roma Yolu: Hızlı, arabalara zarar vermez
        DirtRoad,       // Toprak Yol: Standart bozkır yolu
        RoughPass,      // Engebeli Geçit: Araba aksı kırılma veya hayvan sakatlanma riski
        SwampTrail      // Bataklık İz: Erzak tüketimini katlar
    }

    /// <summary>
    /// İki coğrafi düğüm arasındaki rotayı, yol şartlarını, pusu riskini ve
    /// rotaya özgü yol olaylarını tanımlayan ScriptableObject veri modeli.
    /// </summary>
    [CreateAssetMenu(fileName = "Route_New", menuName = "Kervan/Travel/Travel Route", order = 2)]
    public class TravelRouteSO : ScriptableObject
    {
        [Header("Bağlantı Düğümleri")]
        [SerializeField] private string _routeId;
        [SerializeField] private LocationNodeSO _fromNode;
        [SerializeField] private LocationNodeSO _toNode;

        [Header("Fiziksel Yol Koşulları")]
        [Tooltip("Standart seyahat süresi (Oyun Günü)")]
        [SerializeField] private int _distanceDays = 3;
        [SerializeField] private TerrainType _terrain = TerrainType.Steppe;
        [SerializeField] private RoadQuality _roadQuality = RoadQuality.DirtRoad;
        [SerializeField] private WeatherCondition _weather = WeatherCondition.Clear;

        [Header("Tehlike ve Lojistik")]
        [Range(0f, 1f)]
        [Tooltip("Pusuya düşme olasılığı (0.0 = Güvenli, 1.0 = Kesin pusu)")]
        [SerializeField] private float _dangerLevel = 0.25f;
        [Tooltip("Zorlu şartlarda erzak harcama çarpanı (1.0 = normal)")]
        [SerializeField] private float _foodConsumptionMultiplier = 1.0f;

        [Header("Karşılaşma Düşmanları & Yol Olayları")]
        [Tooltip("Bu rotada pusuya düşüldüğünde çıkabilecek düşman birimleri (Haydut, Kurt vb.)")]
        [SerializeField] private List<UnitDataSO> _potentialEnemies = new List<UnitDataSO>();

        [Tooltip("Bu rotada tetiklenebilecek özel yol olayları (Boş bırakılırsa TravelManager genel havuzunu kullanır)")]
        [SerializeField] private List<RoadEventSO> _roadEvents = new List<RoadEventSO>();

        // Getters
        public string RouteId => _routeId;
        public LocationNodeSO FromNode => _fromNode;
        public LocationNodeSO ToNode => _toNode;
        public int DistanceDays => _distanceDays;
        public TerrainType Terrain => _terrain;
        public RoadQuality RoadQuality => _roadQuality;
        public WeatherCondition Weather => _weather;
        public float DangerLevel => _dangerLevel;
        public float FoodConsumptionMultiplier => _foodConsumptionMultiplier;
        public IReadOnlyList<UnitDataSO> PotentialEnemies => _potentialEnemies;
        public IReadOnlyList<RoadEventSO> RoadEvents => _roadEvents;
    }
}
