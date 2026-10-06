#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kervan.EditorTools
{
    public static class TileAssetConfigurator
    {
        [MenuItem("Kervan/Varlıklar/Zemin Karolarını Yapılandır")]
        public static void ConfigureTileTextures()
        {
            string[] tilePaths = new string[]
            {
                "Assets/_Project/Art/Sprites/Tiles/Tile_Steppe.png",
                "Assets/_Project/Art/Sprites/Tiles/Tile_Cobblestone.png"
            };

            foreach (var path in tilePaths)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogWarning($"[TileConfig] Dosya bulunamadı: {path}");
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                
                // 762 px genişlik, Dünya genişliği W: 2.0f -> PPU = 381
                importer.spritePixelsPerUnit = 381f;

                // Kaideli / derinlikli altıgenler için üst yüzey pivot noktası
                importer.spritePivot = new Vector2(0.5f, 0.58f);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0.58f);
                importer.SetTextureSettings(settings);

                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                Debug.Log($"[TileConfig] Karo sprite olarak başarıyla yapılandırıldı: {path} (PPU: 381, Pivot: 0.5, 0.58)");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
