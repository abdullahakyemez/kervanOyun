#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kervan.Core;
using Kervan.TradeAndTravel;
using Kervan.Combat;

namespace Kervan.EditorTools
{
    public static class TravelDataGenerator
    {
        [MenuItem("Kervan/Olustur/Seyahat ve Dugum Verilerini Olustur")]
        public static void GenerateTravelData()
        {
            string nodesPath = "Assets/_Project/ScriptableObjects/Nodes";
            string routesPath = "Assets/_Project/ScriptableObjects/Routes";

            EnsureDirectoryExists(nodesPath);
            EnsureDirectoryExists(routesPath);

            // 1. Dugumler
            var sogut = CreateNode(nodesPath + "/Node_Sogut.asset", "node_sogut", "Söğüt", NodeType.City, FactionType.Ottoman,
                "Osmanlı Uç Beyliği'nin kutlu başkenti ve kervanımızın ana ocağı.", new Vector2(0f, 0f), 10, 15, 20);

            var bilecik = CreateNode(nodesPath + "/Node_Bilecik.asset", "node_bilecik", "Bilecik", NodeType.City, FactionType.Ottoman,
                "Demir madenleri ve kılıç dövüm ocaklarıyla ünlü müstahkem beylik kalesi.", new Vector2(40f, 20f), 12, 15, 15);

            var domanic = CreateNode(nodesPath + "/Node_Domanic.asset", "node_domanic", "Domaniç", NodeType.Caravanserai, FactionType.Ottoman,
                "Yaz aylarında konaklanan serin yayla kampı ve at yetiştirme merkezi.", new Vector2(-30f, -40f), 8, 25, 25);

            var iznik = CreateNode(nodesPath + "/Node_Iznik.asset", "node_iznik", "İznik (Nikaia)", NodeType.City, FactionType.Byzantine,
                "Göl kıyısında antik surları ve Bizans atölyeleriyle kadim bir ticaret şehri.", new Vector2(30f, 70f), 18, 15, 15);

            var bursa = CreateNode(nodesPath + "/Node_Bursa.asset", "node_bursa", "Bursa (Prusa)", NodeType.City, FactionType.Ottoman,
                "İpek Yolu'nun Anadolu'daki en zengin pazarlarına ve dev kervansaraylarına ev sahipliği yapar.", new Vector2(80f, 50f), 20, 20, 20);

            var karatayHan = CreateNode(nodesPath + "/Node_KaratayHan.asset", "node_karatay_han", "Karatay Kervansarayı", NodeType.Caravanserai, FactionType.SeljukLegacy,
                "Selçuklu mirası kesme taş duvarları ve çinileriyle korunaklı sığınak.", new Vector2(20f, -10f), 15, 30, 25);

            // 2. Rotalar
            var banditData = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/_Project/ScriptableObjects/Units/Unit_Haydut.asset");

            CreateRoute(routesPath + "/Route_Sogut_Bilecik.asset", "route_sogut_bilecik", sogut, bilecik, 2, TerrainType.Steppe, RoadQuality.DirtRoad, WeatherCondition.Clear, 0.20f, 1.0f, banditData);
            CreateRoute(routesPath + "/Route_Bilecik_Iznik.asset", "route_bilecik_iznik", bilecik, iznik, 3, TerrainType.Forest, RoadQuality.RoughPass, WeatherCondition.Clear, 0.40f, 1.2f, banditData);
            CreateRoute(routesPath + "/Route_Iznik_Bursa.asset", "route_iznik_bursa", iznik, bursa, 4, TerrainType.Steppe, RoadQuality.PavedRomanRoad, WeatherCondition.Clear, 0.25f, 1.0f, banditData);
            CreateRoute(routesPath + "/Route_Sogut_Domanic.asset", "route_sogut_domanic", sogut, domanic, 2, TerrainType.MountainPass, RoadQuality.RoughPass, WeatherCondition.Clear, 0.30f, 1.3f, banditData);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kervan] Tum harita dugumleri ve rotalari basariyla olusturuldu!");
        }

        private static LocationNodeSO CreateNode(string path, string id, string name, NodeType type, FactionType faction, string desc, Vector2 coords, int restCost, int heal, int morale)
        {
            var node = AssetDatabase.LoadAssetAtPath<LocationNodeSO>(path);
            if (node == null)
            {
                node = ScriptableObject.CreateInstance<LocationNodeSO>();
                AssetDatabase.CreateAsset(node, path);
            }

            SerializedObject so = new SerializedObject(node);
            so.FindProperty("_nodeId").stringValue = id;
            so.FindProperty("_nodeName").stringValue = name;
            so.FindProperty("_nodeType").enumValueIndex = (int)type;
            so.FindProperty("_rulingFaction").enumValueIndex = (int)faction;
            so.FindProperty("_description").stringValue = desc;
            so.FindProperty("_mapCoordinates").vector2Value = coords;
            so.FindProperty("_dailyRestCost").intValue = restCost;
            so.FindProperty("_dailyHealAmount").intValue = heal;
            so.FindProperty("_dailyMoraleRestore").intValue = morale;
            so.ApplyModifiedPropertiesWithoutUndo();

            return node;
        }

        private static TravelRouteSO CreateRoute(string path, string id, LocationNodeSO from, LocationNodeSO to, int days, TerrainType terrain, RoadQuality road, WeatherCondition weather, float danger, float foodMult, UnitDataSO bandit)
        {
            var route = AssetDatabase.LoadAssetAtPath<TravelRouteSO>(path);
            if (route == null)
            {
                route = ScriptableObject.CreateInstance<TravelRouteSO>();
                AssetDatabase.CreateAsset(route, path);
            }

            SerializedObject so = new SerializedObject(route);
            so.FindProperty("_routeId").stringValue = id;
            so.FindProperty("_fromNode").objectReferenceValue = from;
            so.FindProperty("_toNode").objectReferenceValue = to;
            so.FindProperty("_distanceDays").intValue = days;
            so.FindProperty("_terrain").enumValueIndex = (int)terrain;
            so.FindProperty("_roadQuality").enumValueIndex = (int)road;
            so.FindProperty("_weather").enumValueIndex = (int)weather;
            so.FindProperty("_dangerLevel").floatValue = danger;
            so.FindProperty("_foodConsumptionMultiplier").floatValue = foodMult;

            if (bandit != null)
            {
                var enemiesProp = so.FindProperty("_potentialEnemies");
                enemiesProp.ClearArray();
                enemiesProp.InsertArrayElementAtIndex(0);
                enemiesProp.GetArrayElementAtIndex(0).objectReferenceValue = bandit;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return route;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, parts[i]);
                    }
                    current = next;
                }
            }
        }
    }
}
#endif
