using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;
using Kervan.Combat;

namespace Kervan.TradeAndTravel
{
    public enum RoadQuality
    {
        PavedRomanRoad, // Roma Yolu: Hizli, arabalara zarar vermez
        DirtRoad,       // Toprak Yol: Standart bozkir yolu
        RoughPass,      // Engebeli Gecit: Araba aksı kirilma veya hayvan sakatlanma riski
        SwampTrail      // Bataklik Iz: Erzak tuketimini katlar
    }

    /// <summary>
    /// Iki cografi dugum arasindaki rotayi, yol sartlarini ve pusu riskini
    /// tanimlayan ScriptableObject veri modeli.
    /// </summary>
    [CreateAssetMenu(fileName = "Route_New", menuName = "Kervan/Travel/Travel Route", order = 2)]
    public class TravelRouteSO : ScriptableObject
    {
        [Header("Baglanti Dugumleri")]
        [SerializeField] private string _routeId;
        [SerializeField] private LocationNodeSO _fromNode;
        [SerializeField] private LocationNodeSO _toNode;

        [Header("Fiziksel Yol Kosullari")]
        [Tooltip("Standart seyahat suresi (Oyun Gunu)")]
        [SerializeField] private int _distanceDays = 3;
        [SerializeField] private TerrainType _terrain = TerrainType.Steppe;
        [SerializeField] private RoadQuality _roadQuality = RoadQuality.DirtRoad;
        [SerializeField] private WeatherCondition _weather = WeatherCondition.Clear;

        [Header("Tehlike ve Lojistik")]
        [Range(0f, 1f)]
        [Tooltip("Pusuya dusme olasiligi (0.0 = Guvenli, 1.0 = Kesin pusu)")]
        [SerializeField] private float _dangerLevel = 0.25f;
        [Tooltip("Zorlu sartlarda erzak harcama carpani (1.0 = normal)")]
        [SerializeField] private float _foodConsumptionMultiplier = 1.0f;

        [Header("Karşılaşma Düşmanları")]
        [SerializeField] private List<UnitDataSO> _potentialEnemies = new List<UnitDataSO>();

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
    }
}
