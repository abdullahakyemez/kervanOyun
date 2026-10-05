#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kervan.Core;
using Kervan.Combat;
using Kervan.TradeAndTravel;

namespace Kervan.EditorTools
{
    public static class InitialDataGenerator
    {
        [MenuItem("Kervan/Olustur/Baslangic Verilerini Olustur")]
        public static void GenerateAllInitialData()
        {
            string unitsPath = "Assets/_Project/ScriptableObjects/Units";
            string itemsPath = "Assets/_Project/ScriptableObjects/Items";

            EnsureDirectoryExists(unitsPath);
            EnsureDirectoryExists(itemsPath);

            CreateUnits(unitsPath);
            CreateItems(itemsPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kervan] Tum baslangic ScriptableObject verileri basariyla olusturuldu!");
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

        private static void CreateUnits(string path)
        {
            // 1. Alp (Ana Karakter - Hibrit)
            CreateOrUpdateUnit(path + "/Unit_Alp.asset", unit =>
            {
                SetSerializedField(unit, "_unitId", "alp_hero");
                SetSerializedField(unit, "_unitName", "Kıdemli Alp (Tüccar-Gazi)");
                SetSerializedField(unit, "_combatRole", UnitCombatRole.Hybrid);
                SetSerializedField(unit, "_faction", FactionType.Ottoman);
                SetSerializedField(unit, "_description", "Osmanlı idaresine gizli istihbarat ağı ile bağlı kıdemli gazi tüccar. Hem kılıç hem kompozit yay ustası.");
                SetSerializedField(unit, "_maxHealth", 120);
                SetSerializedField(unit, "_maxArmor", 35);
                SetSerializedField(unit, "_maxStamina", 5);
                SetSerializedField(unit, "_baseMorale", 120);
                SetSerializedField(unit, "_minDamage", 16);
                SetSerializedField(unit, "_maxDamage", 24);
                SetSerializedField(unit, "_meleeRange", 1);
                SetSerializedField(unit, "_rangedRange", 3);
                SetSerializedField(unit, "_baseAccuracy", 0.90f);
            });

            // 2. Bozkır Okçusu
            CreateOrUpdateUnit(path + "/Unit_Okcu.asset", unit =>
            {
                SetSerializedField(unit, "_unitId", "steppe_archer");
                SetSerializedField(unit, "_unitName", "Bozkır Okçusu");
                SetSerializedField(unit, "_combatRole", UnitCombatRole.Ranged);
                SetSerializedField(unit, "_faction", FactionType.Steppe);
                SetSerializedField(unit, "_description", "Çevik, uzun menzilli Türkmen avcısı. Yüksek isabet ve menzil kabiliyeti.");
                SetSerializedField(unit, "_maxHealth", 75);
                SetSerializedField(unit, "_maxArmor", 15);
                SetSerializedField(unit, "_maxStamina", 5);
                SetSerializedField(unit, "_baseMorale", 90);
                SetSerializedField(unit, "_minDamage", 12);
                SetSerializedField(unit, "_maxDamage", 18);
                SetSerializedField(unit, "_meleeRange", 1);
                SetSerializedField(unit, "_rangedRange", 4);
                SetSerializedField(unit, "_baseAccuracy", 0.85f);
            });

            // 3. Kervan Muhafızı (Kalkanlı)
            CreateOrUpdateUnit(path + "/Unit_Muhafiz.asset", unit =>
            {
                SetSerializedField(unit, "_unitId", "caravan_guard");
                SetSerializedField(unit, "_unitName", "Kervan Muhafızı");
                SetSerializedField(unit, "_combatRole", UnitCombatRole.Melee);
                SetSerializedField(unit, "_faction", FactionType.SeljukLegacy);
                SetSerializedField(unit, "_description", "Ağır kalkanlı ve zincir zırhlı tecrübeli muhafız. Ön safta darbe sönümleyici.");
                SetSerializedField(unit, "_maxHealth", 110);
                SetSerializedField(unit, "_maxArmor", 50);
                SetSerializedField(unit, "_maxStamina", 5);
                SetSerializedField(unit, "_baseMorale", 100);
                SetSerializedField(unit, "_minDamage", 10);
                SetSerializedField(unit, "_maxDamage", 15);
                SetSerializedField(unit, "_meleeRange", 1);
                SetSerializedField(unit, "_rangedRange", 1);
                SetSerializedField(unit, "_baseAccuracy", 0.80f);
            });

            // 4. Dağ Haydudu (Düşman)
            CreateOrUpdateUnit(path + "/Unit_Haydut.asset", unit =>
            {
                SetSerializedField(unit, "_unitId", "mountain_bandit");
                SetSerializedField(unit, "_unitName", "Dağ Haydudu");
                SetSerializedField(unit, "_combatRole", UnitCombatRole.Melee);
                SetSerializedField(unit, "_faction", FactionType.Bandit);
                SetSerializedField(unit, "_description", "Geçitleri tutan acımasız dağ eşkıyası. Düşük zırh, kontrolsüz ve saldırgan vuruşlar.");
                SetSerializedField(unit, "_maxHealth", 80);
                SetSerializedField(unit, "_maxArmor", 10);
                SetSerializedField(unit, "_maxStamina", 5);
                SetSerializedField(unit, "_baseMorale", 70);
                SetSerializedField(unit, "_minDamage", 14);
                SetSerializedField(unit, "_maxDamage", 22);
                SetSerializedField(unit, "_meleeRange", 1);
                SetSerializedField(unit, "_rangedRange", 1);
                SetSerializedField(unit, "_baseAccuracy", 0.75f);
            });
        }

        private static void CreateItems(string path)
        {
            CreateOrUpdateItem(path + "/Item_Ipek.asset", item =>
            {
                SetSerializedField(item, "_itemId", "item_silk");
                SetSerializedField(item, "_itemName", "Bursa İpeği");
                SetSerializedField(item, "_category", ItemCategory.Luxury);
                SetSerializedField(item, "_description", "Doğu Roma ve Bursa tezgahlarının yüksek kıymetli kumaşı. İç bölgelerde yüksek kâr sağlar.");
                SetSerializedField(item, "_basePrice", 180);
                SetSerializedField(item, "_unitWeightKg", 0.5f);
            });

            CreateOrUpdateItem(path + "/Item_Demir.asset", item =>
            {
                SetSerializedField(item, "_itemId", "item_iron");
                SetSerializedField(item, "_itemName", "Bilecik Demiri");
                SetSerializedField(item, "_category", ItemCategory.RawMaterial);
                SetSerializedField(item, "_description", "Uç beyliklerinin kılıç ve zırh dövümünde kullandığı kaliteli maden.");
                SetSerializedField(item, "_basePrice", 45);
                SetSerializedField(item, "_unitWeightKg", 2.5f);
            });

            CreateOrUpdateItem(path + "/Item_Bugday.asset", item =>
            {
                SetSerializedField(item, "_itemId", "item_wheat");
                SetSerializedField(item, "_itemName", "Konya Buğdayı");
                SetSerializedField(item, "_category", ItemCategory.Food);
                SetSerializedField(item, "_description", "Bozkırın temel besin kaynağı. Kıtlık çeken şehirlerde ve seferlerde altın değerindedir.");
                SetSerializedField(item, "_basePrice", 15);
                SetSerializedField(item, "_unitWeightKg", 1.0f);
            });

            CreateOrUpdateItem(path + "/Item_Baharat.asset", item =>
            {
                SetSerializedField(item, "_itemId", "item_spice");
                SetSerializedField(item, "_itemName", "Doğu Baharatı");
                SetSerializedField(item, "_category", ItemCategory.Luxury);
                SetSerializedField(item, "_description", "İpek Yolu kervanlarıyla uzak diyarlardan gelen kıymetli baharatlar.");
                SetSerializedField(item, "_basePrice", 120);
                SetSerializedField(item, "_unitWeightKg", 0.3f);
            });
        }

        private static void CreateOrUpdateUnit(string assetPath, System.Action<UnitDataSO> configure)
        {
            var unit = AssetDatabase.LoadAssetAtPath<UnitDataSO>(assetPath);
            if (unit == null)
            {
                unit = ScriptableObject.CreateInstance<UnitDataSO>();
                configure(unit);
                AssetDatabase.CreateAsset(unit, assetPath);
            }
            else
            {
                configure(unit);
                EditorUtility.SetDirty(unit);
            }
        }

        private static void CreateOrUpdateItem(string assetPath, System.Action<ItemDataSO> configure)
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemDataSO>();
                configure(item);
                AssetDatabase.CreateAsset(item, assetPath);
            }
            else
            {
                configure(item);
                EditorUtility.SetDirty(item);
            }
        }

        private static void SetSerializedField(Object target, string fieldName, object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                if (value is string s) prop.stringValue = s;
                else if (value is int i) prop.intValue = i;
                else if (value is float f) prop.floatValue = f;
                else if (value is System.Enum e) prop.enumValueIndex = System.Convert.ToInt32(e);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
