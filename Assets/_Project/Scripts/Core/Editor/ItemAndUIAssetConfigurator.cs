#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Kervan.Core;
using Kervan.TradeAndTravel;

namespace Kervan.EditorTools
{
    public static class ItemAndUIAssetConfigurator
    {
        [MenuItem("Kervan/Varlıklar/Malları ve UI Çerçevelerini Yapılandır")]
        public static void ConfigureAll()
        {
            ConfigureSprites();
            CreateAndLinkItems();
            UpdateCityMarkets();
            RebuildScenesWithArt();
        }

        private static void ConfigureSprites()
        {
            // 1. Mal İkonları
            string[] items = new string[] { "Item_Ipek", "Item_Demir", "Item_Bugday", "Item_Baharat", "Item_Celik", "Item_Kurk" };
            foreach (var item in items)
            {
                string path = $"Assets/_Project/Art/Sprites/Items/{item}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100f;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.alphaIsTransparency = true;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    Debug.Log($"[ItemConfig] Mal ikonu yapılandırıldı: {item}");
                }
            }

            // 2. UI Çerçeveleri
            string[] uiFrames = new string[] { "UI_Frame_Market", "UI_Frame_Divan" };
            foreach (var frame in uiFrames)
            {
                string path = $"Assets/_Project/Art/Sprites/UI/{frame}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100f;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.alphaIsTransparency = true;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    Debug.Log($"[UIConfig] UI Çerçevesi yapılandırıldı: {frame}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateAndLinkItems()
        {
            string itemsFolder = "Assets/_Project/ScriptableObjects/Items";

            // 1. Yeni Mallar: Çelik ve Kürk
            CreateOrUpdateItem($"{itemsFolder}/Item_Celik.asset", "item_celik", "Şam Çeliği", ItemCategory.RawMaterial,
                "Bilecik ocaklarında dövülmüş, menevişli birinci sınıf kılıç ve zırh çeliği.", 110, 2.5f, "Item_Celik");

            CreateOrUpdateItem($"{itemsFolder}/Item_Kurk.asset", "item_kurk", "Bozkır Kürkü", ItemCategory.Luxury,
                "Kuzey ve bozkır yaylalarından kervanlarla getirilen kıymetli kışlık kürk postu.", 85, 1.2f, "Item_Kurk");

            // 2. Mevcut Mallara İkon Bağla
            LinkIconToExistingItem($"{itemsFolder}/Item_Ipek.asset", "Item_Ipek");
            LinkIconToExistingItem($"{itemsFolder}/Item_Demir.asset", "Item_Demir");
            LinkIconToExistingItem($"{itemsFolder}/Item_Bugday.asset", "Item_Bugday");
            LinkIconToExistingItem($"{itemsFolder}/Item_Baharat.asset", "Item_Baharat");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ItemConfig] Tüm emtialar ve ikonlar başarıyla bağlandı!");
        }

        private static void CreateOrUpdateItem(string path, string id, string name, ItemCategory cat, string desc, int price, float weight, string spriteName)
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemDataSO>();
                AssetDatabase.CreateAsset(item, path);
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/Sprites/Items/{spriteName}.png");

            SetSerializedRef(item, "_itemId", id);
            SetSerializedRef(item, "_itemName", name);
            SetSerializedRef(item, "_category", (int)cat);
            SetSerializedRef(item, "_description", desc);
            SetSerializedRef(item, "_basePrice", price);
            SetSerializedRef(item, "_unitWeightKg", weight);
            SetSerializedRef(item, "_icon", sprite);

            EditorUtility.SetDirty(item);
        }

        private static void LinkIconToExistingItem(string path, string spriteName)
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/Sprites/Items/{spriteName}.png");
            if (item != null && sprite != null)
            {
                SetSerializedRef(item, "_icon", sprite);
                EditorUtility.SetDirty(item);
            }
        }

        private static void UpdateCityMarkets()
        {
            var celikItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Celik.asset");
            var kurkItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Kurk.asset");

            if (celikItem == null || kurkItem == null) return;

            // Bilecik: Çelik madeni (Bol/ucuz: x0.8), Kürk talebi (x1.4)
            AddOrUpdateMarketItem("Assets/_Project/ScriptableObjects/Markets/Market_Bilecik.asset", celikItem, 50, 0.8f);
            AddOrUpdateMarketItem("Assets/_Project/ScriptableObjects/Markets/Market_Bilecik.asset", kurkItem, 15, 1.4f);

            // Bursa: Çelik talebi (x1.3), Kürk lüks tüketimi (x1.2)
            AddOrUpdateMarketItem("Assets/_Project/ScriptableObjects/Markets/Market_Bursa.asset", celikItem, 20, 1.3f);
            AddOrUpdateMarketItem("Assets/_Project/ScriptableObjects/Markets/Market_Bursa.asset", kurkItem, 30, 1.2f);

            // Söğüt: Çelik ordugâh ihtiyacı (x1.25), Kürk yayla üretimi (x0.85)
            AddOrUpdateMarketItem("Assets/_Project/ScriptableObjects/Markets/Market_Sogut.asset", celikItem, 25, 1.25f);
            AddOrUpdateMarketItem("Assets/_Project/ScriptableObjects/Markets/Market_Sogut.asset", kurkItem, 45, 0.85f);

            // İznik: Dengeli talep
            AddOrUpdateMarketItem("Assets/_Project/ScriptableObjects/Markets/Market_Iznik.asset", celikItem, 25, 1.1f);
            AddOrUpdateMarketItem("Assets/_Project/ScriptableObjects/Markets/Market_Iznik.asset", kurkItem, 25, 1.1f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MarketConfig] Şehir pazarlarına Çelik ve Kürk arbitraj kotaları eklendi!");
        }

        private static void AddOrUpdateMarketItem(string marketPath, ItemDataSO item, int stock, float mult)
        {
            var marketSO = AssetDatabase.LoadAssetAtPath<CityMarketDataSO>(marketPath);
            if (marketSO == null) return;

            SerializedObject so = new SerializedObject(marketSO);
            SerializedProperty listProp = so.FindProperty("_marketItems");
            if (listProp == null) return;

            // Zaten var mı kontrol et
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                var itemProp = elem.FindPropertyRelative("item");
                if (itemProp != null && itemProp.objectReferenceValue == item)
                {
                    elem.FindPropertyRelative("initialStock").intValue = stock;
                    elem.FindPropertyRelative("basePriceMultiplier").floatValue = mult;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(marketSO);
                    return;
                }
            }

            // Yeni ekle
            int newIdx = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(newIdx);
            var newElem = listProp.GetArrayElementAtIndex(newIdx);
            newElem.FindPropertyRelative("item").objectReferenceValue = item;
            newElem.FindPropertyRelative("initialStock").intValue = stock;
            newElem.FindPropertyRelative("basePriceMultiplier").floatValue = mult;

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(marketSO);
        }

        private static void RebuildScenesWithArt()
        {
            MarketSceneBuilder.BuildMarketScene();
            TownHubSceneBuilder.BuildTownHubScene();
            Debug.Log("[ItemConfig] Pazar ve Şehir sahneleri yeni görseller ve çerçevelerle yeniden inşa edildi!");
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
