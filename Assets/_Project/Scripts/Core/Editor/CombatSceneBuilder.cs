using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Kervan.Combat;

namespace Kervan.EditorTools
{
    public static class CombatSceneBuilder
    {
        [MenuItem("Kervan/Olustur/Savas Test Sahnesini Kur")]
        public static void BuildCombatTestScene()
        {
            string scenePath = "Assets/_Project/Scenes/CombatScene.unity";

            // Yeni sahne ac
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Ana Kamera (2D Orthographic)
            var cameraObj = new GameObject("Main Camera");
            var cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.45f, 0.33f); // Bozkir topragi tonu #8C7355
            cameraObj.tag = "MainCamera";

            // 2. Izgara Yoneticisi
            var gridObj = new GameObject("HexGrid");
            var hexGrid = gridObj.AddComponent<HexGrid>();

            // Kamerayi izgaranin ortasina ortala
            cameraObj.transform.position = new Vector3(5.5f, 1.8f, -10f);

            // 3. Savas Kontrolculeri
            var managerObj = new GameObject("CombatManager");
            var turnManager = managerObj.AddComponent<CombatTurnManager>();
            managerObj.AddComponent<CombatInputHandler>();

            // 4. Test Birimleri
            var alpData = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/_Project/ScriptableObjects/Units/Unit_Alp.asset");
            var guardData = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/_Project/ScriptableObjects/Units/Unit_Muhafiz.asset");
            var banditData = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/_Project/ScriptableObjects/Units/Unit_Haydut.asset");

            // Birim prototip olusturucu yardimcisi
            var alpUnit = CreateUnitObject("Unit_Alp_Player", alpData, new HexCoordinates(1, 2), true, new Color(0.16f, 0.45f, 0.25f)); // Sancak Yesili
            var guardUnit = CreateUnitObject("Unit_Muhafiz_Player", guardData, new HexCoordinates(1, 3), true, new Color(0.25f, 0.35f, 0.45f)); // Zirh Grisi

            var bandit1 = CreateUnitObject("Unit_Haydut_1", banditData, new HexCoordinates(5, 2), false, new Color(0.55f, 0.15f, 0.12f)); // Koyu Al
            var bandit2 = CreateUnitObject("Unit_Haydut_2", banditData, new HexCoordinates(5, 3), false, new Color(0.55f, 0.15f, 0.12f));

            turnManager.RegisterUnit(alpUnit);
            turnManager.RegisterUnit(guardUnit);
            turnManager.RegisterUnit(bandit1);
            turnManager.RegisterUnit(bandit2);

            // Sahneyi kaydet
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Sahneyi Build Settings'e ekle
            AddSceneToBuildSettings(scenePath);

            Debug.Log($"[Kervan] Savas sahnesi basariyla olusturuldu ve kaydedildi: {scenePath}");
        }

        private static CombatUnit CreateUnitObject(string name, UnitDataSO data, HexCoordinates coords, bool isPlayer, Color placeholderColor)
        {
            var unitObj = new GameObject(name);
            var sr = unitObj.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Entities";
            sr.color = placeholderColor;

            // Gecici bir 2D daire/elips sprite'i olustur (Placeholder)
            sr.sprite = CreatePlaceholderSprite();

            var unit = unitObj.AddComponent<CombatUnit>();
            unit.Initialize(data, coords, isPlayer);

            return unit;
        }

        private static Sprite CreatePlaceholderSprite()
        {
            // 64x64 gecici prototip daire sprite'i
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        float borderDist = radius - dist;
                        if (borderDist < 2.5f)
                            tex.SetPixel(x, y, new Color(0.12f, 0.10f, 0.08f, 1f)); // Koyu kontur
                        else
                            tex.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.2f), 64f);
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }

            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            for (int i = 0; i < scenes.Length; i++) newScenes[i] = scenes[i];
            newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}

