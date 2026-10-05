#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kervan.TradeAndTravel;

namespace Kervan.EditorTools
{
    public static class MarketDataGenerator
    {
        [MenuItem("Kervan/Olustur/Sehir Pazarlari Verilerini Olustur")]
        public static void GenerateMarkets()
        {
            string marketsPath = "Assets/_Project/ScriptableObjects/Markets";
            EnsureDirectoryExists(marketsPath);

            var sogutNode = AssetDatabase.LoadAssetAtPath<LocationNodeSO>("Assets/_Project/ScriptableObjects/Nodes/Node_Sogut.asset");
            var bilecikNode = AssetDatabase.LoadAssetAtPath<LocationNodeSO>("Assets/_Project/ScriptableObjects/Nodes/Node_Bilecik.asset");
            var bursaNode = AssetDatabase.LoadAssetAtPath<LocationNodeSO>("Assets/_Project/ScriptableObjects/Nodes/Node_Bursa.asset");
            var iznikNode = AssetDatabase.LoadAssetAtPath<LocationNodeSO>("Assets/_Project/ScriptableObjects/Nodes/Node_Iznik.asset");

            var ipek = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Ipek.asset");
            var demir = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Demir.asset");
            var bugday = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Bugday.asset");
            var baharat = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Baharat.asset");

            // 1. Bursa Pazari (Ipek uretim merkezi, Demir ve Tahil kit)
            CreateMarket(marketsPath + "/Market_Bursa.asset", bursaNode, 0.08f,
                local: new[] { (ipek, 80, 0.65f) },
                demanded: new[] { (demir, 40, 1.70f), (bugday, 60, 1.45f) },
                standard: new[] { (baharat, 20, 1.10f) });

            // 2. Bilecik Pazari (Demir madeni merkezi, Ipek ve Baharat kit)
            CreateMarket(marketsPath + "/Market_Bilecik.asset", bilecikNode, 0.06f,
                local: new[] { (demir, 100, 0.60f) },
                demanded: new[] { (ipek, 30, 1.85f), (baharat, 25, 1.60f) },
                standard: new[] { (bugday, 40, 1.05f) });

            // 3. Sogut Pazari (Beylik merkezi)
            CreateMarket(marketsPath + "/Market_Sogut.asset", sogutNode, 0.05f,
                local: new[] { (bugday, 70, 0.75f) },
                demanded: new[] { (ipek, 25, 1.60f), (demir, 30, 1.30f) },
                standard: new[] { (baharat, 15, 1.15f) });

            // 4. Iznik Pazari (Kultur & Baharat limani)
            CreateMarket(marketsPath + "/Market_Iznik.asset", iznikNode, 0.10f,
                local: new[] { (baharat, 50, 0.70f) },
                demanded: new[] { (demir, 35, 1.50f) },
                standard: new[] { (ipek, 30, 1.15f), (bugday, 40, 1.10f) });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kervan] Tum sehir pazarlari verileri (Arbitraj kurgusuyla) basariyla olusturuldu!");
        }

        private static void CreateMarket(string path, LocationNodeSO node, float tax,
            (ItemDataSO item, int stock, float mult)[] local,
            (ItemDataSO item, int stock, float mult)[] demanded,
            (ItemDataSO item, int stock, float mult)[] standard)
        {
            var market = AssetDatabase.LoadAssetAtPath<CityMarketDataSO>(path);
            if (market == null)
            {
                market = ScriptableObject.CreateInstance<CityMarketDataSO>();
                AssetDatabase.CreateAsset(market, path);
            }

            SerializedObject so = new SerializedObject(market);
            so.FindProperty("_cityNode").objectReferenceValue = node;
            so.FindProperty("_baseTaxRate").floatValue = tax;

            PopulateEntries(so.FindProperty("_localGoods"), local);
            PopulateEntries(so.FindProperty("_demandedGoods"), demanded);
            PopulateEntries(so.FindProperty("_standardGoods"), standard);

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PopulateEntries(SerializedProperty listProp, (ItemDataSO item, int stock, float mult)[] items)
        {
            listProp.ClearArray();
            for (int i = 0; i < items.Length; i++)
            {
                listProp.InsertArrayElementAtIndex(i);
                var elem = listProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("Item").objectReferenceValue = items[i].item;
                elem.FindPropertyRelative("BaseStock").intValue = items[i].stock;
                elem.FindPropertyRelative("PriceMultiplier").floatValue = items[i].mult;
            }
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
