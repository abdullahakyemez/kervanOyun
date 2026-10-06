#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Kervan.Combat;
using Kervan.Core;

namespace Kervan.EditorTools
{
    public static class UnitAssetConfigurator
    {
        [MenuItem("Kervan/Varlıklar/Birim Karakterlerini Yapılandır")]
        public static void ConfigureUnitSprites()
        {
            string[] spriteNames = new string[]
            {
                "Unit_Alp",
                "Unit_Muhafiz",
                "Unit_Okcu",
                "Unit_Haydut",
                "Unit_Haydut_Okcu"
            };

            foreach (var name in spriteNames)
            {
                string path = $"Assets/_Project/Art/Sprites/Units/{name}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"[UnitConfig] Görsel bulunamadı: {path}");
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                
                // 384 px boy / 256 PPU = 1.5 Unit boyu (Altıgen zeminle mükemmel minyatür orantısı)
                importer.spritePixelsPerUnit = 256f;

                // Ayak tabanı pivotu (Karonun üzerine basması için)
                importer.spritePivot = new Vector2(0.5f, 0.05f);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0.05f);
                importer.SetTextureSettings(settings);

                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                Debug.Log($"[UnitConfig] Birim sprite'ı yapılandırıldı: {name} (PPU: 256, Pivot: 0.5, 0.05)");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // ScriptableObject'lere bağla
            LinkSpritesToUnitData();
        }

        private static void LinkSpritesToUnitData()
        {
            AssignSpriteToSO("Assets/_Project/ScriptableObjects/Units/Unit_Alp.asset", "Assets/_Project/Art/Sprites/Units/Unit_Alp.png");
            AssignSpriteToSO("Assets/_Project/ScriptableObjects/Units/Unit_Muhafiz.asset", "Assets/_Project/Art/Sprites/Units/Unit_Muhafiz.png");
            AssignSpriteToSO("Assets/_Project/ScriptableObjects/Units/Unit_Okcu.asset", "Assets/_Project/Art/Sprites/Units/Unit_Okcu.png");
            AssignSpriteToSO("Assets/_Project/ScriptableObjects/Units/Unit_Haydut.asset", "Assets/_Project/Art/Sprites/Units/Unit_Haydut.png");

            // Haydut Okçusu için UnitDataSO oluştur / güncelle
            string banditArcherPath = "Assets/_Project/ScriptableObjects/Units/Unit_Haydut_Okcu.asset";
            var banditArcherSO = AssetDatabase.LoadAssetAtPath<UnitDataSO>(banditArcherPath);
            if (banditArcherSO == null)
            {
                banditArcherSO = ScriptableObject.CreateInstance<UnitDataSO>();
                AssetDatabase.CreateAsset(banditArcherSO, banditArcherPath);
            }

            var banditArcherSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Units/Unit_Haydut_Okcu.png");
            SetSerializedRef(banditArcherSO, "_unitId", "unit_haydut_okcu");
            SetSerializedRef(banditArcherSO, "_unitName", "Haydut Nişancısı");
            SetSerializedRef(banditArcherSO, "_combatRole", (int)UnitCombatRole.Ranged);
            SetSerializedRef(banditArcherSO, "_faction", (int)FactionType.Bandit);
            SetSerializedRef(banditArcherSO, "_description", "Pusu kurmuş yaylı eşkıya.");
            SetSerializedRef(banditArcherSO, "_maxHealth", 70);
            SetSerializedRef(banditArcherSO, "_maxArmor", 10);
            SetSerializedRef(banditArcherSO, "_maxStamina", 5);
            SetSerializedRef(banditArcherSO, "_baseMorale", 80);
            SetSerializedRef(banditArcherSO, "_minDamage", 14);
            SetSerializedRef(banditArcherSO, "_maxDamage", 20);
            SetSerializedRef(banditArcherSO, "_meleeRange", 1);
            SetSerializedRef(banditArcherSO, "_rangedRange", 3);
            SetSerializedRef(banditArcherSO, "_baseAccuracy", 0.80f);
            SetSerializedRef(banditArcherSO, "_defaultSprite", banditArcherSprite);
            SetSerializedRef(banditArcherSO, "_portrait", banditArcherSprite);
            EditorUtility.SetDirty(banditArcherSO);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UnitConfig] Tüm UnitDataSO varlıklarına yeni sprite'lar başarıyla bağlandı!");
        }

        private static void AssignSpriteToSO(string soPath, string spritePath)
        {
            var so = AssetDatabase.LoadAssetAtPath<UnitDataSO>(soPath);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (so != null && sprite != null)
            {
                SetSerializedRef(so, "_defaultSprite", sprite);
                SetSerializedRef(so, "_portrait", sprite);
                EditorUtility.SetDirty(so);
                Debug.Log($"[UnitConfig] {so.name} -> {sprite.name} bağlandı.");
            }
        }

        private static void SetSerializedRef(Object target, string propName, object val)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(propName);
            if (prop != null)
            {
                if (val is string s) prop.stringValue = s;
                else if (val is int i) prop.intValue = i;
                else if (val is float f) prop.floatValue = f;
                else if (val is Object obj) prop.objectReferenceValue = obj;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
