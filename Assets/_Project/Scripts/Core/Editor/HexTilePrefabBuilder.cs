#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using Kervan.Combat;

namespace Kervan.EditorTools
{
    public static class HexTilePrefabBuilder
    {
        [MenuItem("Kervan/Varlıklar/Karo Prefablarını Oluştur")]
        public static void CreateTilePrefabs()
        {
            string folder = "Assets/_Project/Prefabs/Tiles";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var steppeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Tiles/Tile_Steppe.png");
            var cobbleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Tiles/Tile_Cobblestone.png");

            CreatePrefab(folder, "HexTile_Steppe", steppeSprite);
            CreatePrefab(folder, "HexTile_Cobblestone", cobbleSprite);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TilePrefab] Karo prefabları başarıyla oluşturuldu.");
        }

        private static void CreatePrefab(string folder, string name, Sprite sprite)
        {
            var obj = new GameObject(name);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = "HexGrid";

            // Vurgu katmanı (Highlights)
            var hlObj = new GameObject("Highlight");
            hlObj.transform.SetParent(obj.transform, false);
            var hlSr = hlObj.AddComponent<SpriteRenderer>();
            hlSr.sprite = sprite;
            hlSr.sortingLayerName = "GridHighlights";
            hlSr.enabled = false;

            var tile = obj.AddComponent<HexTile>();

            string prefabPath = $"{folder}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
            Object.DestroyImmediate(obj);

            Debug.Log($"[TilePrefab] Prefab kaydedildi: {prefabPath}");
        }
    }
}
#endif
