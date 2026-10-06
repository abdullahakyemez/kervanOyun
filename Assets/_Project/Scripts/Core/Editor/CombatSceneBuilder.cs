using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Kervan.Combat;
using Kervan.UI;

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
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Tiles/HexTile_Steppe.prefab");
            SetSerializedRef(hexGrid, "_tilePrefab", tilePrefab);

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
            var banditArcherData = AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/_Project/ScriptableObjects/Units/Unit_Haydut_Okcu.asset");

            var alpUnit = CreateUnitObject("Unit_Alp_Player", alpData, new HexCoordinates(1, 2), true, new Color(0.16f, 0.45f, 0.25f)); // Sancak Yesili
            var guardUnit = CreateUnitObject("Unit_Muhafiz_Player", guardData, new HexCoordinates(1, 3), true, new Color(0.25f, 0.35f, 0.45f)); // Zirh Grisi

            var bandit1 = CreateUnitObject("Unit_Haydut_1", banditData, new HexCoordinates(5, 2), false, new Color(0.55f, 0.15f, 0.12f)); // Koyu Al
            var bandit2 = CreateUnitObject("Unit_Haydut_2", banditArcherData != null ? banditArcherData : banditData, new HexCoordinates(5, 3), false, new Color(0.55f, 0.15f, 0.12f));

            turnManager.RegisterUnit(alpUnit);
            turnManager.RegisterUnit(guardUnit);
            turnManager.RegisterUnit(bandit1);
            turnManager.RegisterUnit(bandit2);

            // 5. Mobil UI Katmani (Canvas, SafeArea, HUD, ActionCluster, Header, Prediction)
            BuildCombatUI(turnManager);

            // Sahneyi kaydet
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AddSceneToBuildSettings(scenePath);

            Debug.Log($"[Kervan] Savas sahnesi ve HUD basariyla olusturuldu: {scenePath}");
        }

        private static void BuildCombatUI(CombatTurnManager turnManager)
        {
            // EventSystem
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Canvas (1920x1080 Landscape)
            var canvasObj = new GameObject("CombatCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingLayerName = "ScreenUI";

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Safe Area
            var safeAreaObj = new GameObject("SafeArea");
            safeAreaObj.transform.SetParent(canvasObj.transform, false);
            var safeAreaRect = safeAreaObj.AddComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;
            safeAreaObj.AddComponent<SafeAreaFitter>();

            // 1. Sol Alt: UnitHUD (420x180 px)
            BuildUnitHUD(safeAreaObj.transform, turnManager);

            // 2. Sag Alt: ActionCluster (Eylem Komuta Kumesi)
            BuildActionCluster(safeAreaObj.transform, turnManager);

            // 3. Ust Orta: CombatHeaderUI (Tur / Faz / Geri Cekil)
            BuildHeaderUI(safeAreaObj.transform, turnManager);

            // 4. Hedef Odak Balonu: CombatPredictionUI
            BuildPredictionUI(safeAreaObj.transform, turnManager);

            // 5. Savas Sonu: CombatResultUI
            BuildResultUI(safeAreaObj.transform, turnManager);
        }

        private static void BuildUnitHUD(Transform parent, CombatTurnManager turnManager)
        {
            var hudObj = new GameObject("UnitHUD");
            hudObj.transform.SetParent(parent, false);
            var rect = hudObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(32f, 32f);
            rect.sizeDelta = new Vector2(420f, 180f);

            var bg = hudObj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.10f, 0.08f, 0.85f); // Koyu murekkep arka plan

            var unitHud = hudObj.AddComponent<UnitHUD>();
            SetSerializedRef(unitHud, "_turnManager", turnManager);

            // Isim Text
            var nameObj = CreateTextObj("UnitName", hudObj.transform, new Vector2(130f, 140f), new Vector2(270f, 30f), "Kıdemli Alp", 20, FontStyles.Bold, new Color(0.95f, 0.90f, 0.80f));
            SetSerializedRef(unitHud, "_unitNameText", nameObj.GetComponent<TextMeshProUGUI>());

            // Rol Text
            var roleObj = CreateTextObj("UnitRole", hudObj.transform, new Vector2(130f, 115f), new Vector2(270f, 24f), "Gazi-Tüccar (Hibrit)", 14, FontStyles.Italic, new Color(0.70f, 0.65f, 0.55f));
            SetSerializedRef(unitHud, "_roleText", roleObj.GetComponent<TextMeshProUGUI>());

            // Portre
            var portraitObj = new GameObject("PortraitFrame");
            portraitObj.transform.SetParent(hudObj.transform, false);
            var pRect = portraitObj.AddComponent<RectTransform>();
            pRect.anchoredPosition = new Vector2(65f, 90f);
            pRect.sizeDelta = new Vector2(100f, 100f);
            var pImg = portraitObj.AddComponent<Image>();
            pImg.color = new Color(0.35f, 0.25f, 0.18f); // Ahsap cerceve
            SetSerializedRef(unitHud, "_portraitImage", pImg);

            // HP Bari (Koyu Al #8B261E)
            var hpSlider = CreateBar("HealthBar", hudObj.transform, new Vector2(265f, 75f), new Vector2(260f, 18f), new Color(0.545f, 0.149f, 0.118f));
            SetSerializedRef(unitHud, "_healthSlider", hpSlider);

            // Zirh Bari (Celik Gri #4A4E51)
            var armorSlider = CreateBar("ArmorBar", hudObj.transform, new Vector2(265f, 52f), new Vector2(260f, 14f), new Color(0.29f, 0.31f, 0.32f));
            SetSerializedRef(unitHud, "_armorSlider", armorSlider);
        }

        private static void BuildActionCluster(Transform parent, CombatTurnManager turnManager)
        {
            var clusterObj = new GameObject("ActionCluster");
            clusterObj.transform.SetParent(parent, false);
            var rect = clusterObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-32f, 32f);
            rect.sizeDelta = new Vector2(200f, 220f);

            var cluster = clusterObj.AddComponent<ActionCluster>();
            SetSerializedRef(cluster, "_turnManager", turnManager);

            // Turu Bitir Butonu (180x64 px)
            var endTurnBtn = CreateButton("EndTurnButton", clusterObj.transform, new Vector2(100f, 32f), new Vector2(180f, 64f), "TURU BİTİR", new Color(0.855f, 0.647f, 0.125f)); // Altin
            SetSerializedRef(cluster, "_endTurnButton", endTurnBtn);

            // Siper Al (Defend - 72x72)
            var defendBtn = CreateButton("DefendButton", clusterObj.transform, new Vector2(50f, 110f), new Vector2(72f, 72f), "SİPER", new Color(0.29f, 0.31f, 0.32f));
            SetSerializedRef(cluster, "_defendButton", defendBtn);

            // Saldir / Mod (72x72)
            var attackBtn = CreateButton("AttackButton", clusterObj.transform, new Vector2(135f, 110f), new Vector2(72f, 72f), "VUR", new Color(0.55f, 0.15f, 0.12f));
            SetSerializedRef(cluster, "_attackModeButton", attackBtn);
        }

        private static void BuildHeaderUI(Transform parent, CombatTurnManager turnManager)
        {
            var headerObj = new GameObject("CombatHeaderUI");
            headerObj.transform.SetParent(parent, false);
            var rect = headerObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            rect.sizeDelta = new Vector2(0f, 80f);

            var headerUI = headerObj.AddComponent<CombatHeaderUI>();
            SetSerializedRef(headerUI, "_turnManager", turnManager);

            var titleObj = CreateTextObj("TurnTitle", headerObj.transform, new Vector2(960f, -20f), new Vector2(400f, 36f), "1. TUR", 26, FontStyles.Bold, Color.white);
            SetSerializedRef(headerUI, "_turnTitleText", titleObj.GetComponent<TextMeshProUGUI>());

            var subObj = CreateTextObj("PhaseSub", headerObj.transform, new Vector2(960f, -50f), new Vector2(400f, 24f), "Alp & Muhafız Fazı", 16, FontStyles.Normal, new Color(0.20f, 0.65f, 0.35f));
            SetSerializedRef(headerUI, "_phaseSubText", subObj.GetComponent<TextMeshProUGUI>());

            // Geri Cekil Butonu (Sag Ust)
            var retreatBtnObj = new GameObject("RetreatButton");
            retreatBtnObj.transform.SetParent(headerObj.transform, false);
            var rRect = retreatBtnObj.AddComponent<RectTransform>();
            rRect.anchorMin = new Vector2(1f, 1f);
            rRect.anchorMax = new Vector2(1f, 1f);
            rRect.pivot = new Vector2(1f, 1f);
            rRect.anchoredPosition = new Vector2(-32f, -10f);
            rRect.sizeDelta = new Vector2(160f, 48f);
            var rImg = retreatBtnObj.AddComponent<Image>();
            rImg.color = new Color(0.20f, 0.15f, 0.15f, 0.90f);
            var holdBtn = retreatBtnObj.AddComponent<HoldButton>();
            SetSerializedRef(headerUI, "_retreatButton", holdBtn);

            CreateTextObj("RetreatText", retreatBtnObj.transform, new Vector2(80f, 24f), new Vector2(150f, 30f), "Geri Çekil (Basılı Tut)", 12, FontStyles.Normal, Color.white);
        }

        private static void BuildPredictionUI(Transform parent, CombatTurnManager turnManager)
        {
            var predObj = new GameObject("CombatPredictionUI");
            predObj.transform.SetParent(parent, false);
            var rect = predObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220f, 60f);

            var bg = predObj.AddComponent<Image>();
            bg.color = new Color(0.95f, 0.90f, 0.81f, 0.95f); // Parsomen zemin

            var pred = predObj.AddComponent<CombatPredictionUI>();
            SetSerializedRef(pred, "_panelRoot", predObj);
            SetSerializedRef(pred, "_turnManager", turnManager);

            var hitObj = CreateTextObj("HitText", predObj.transform, new Vector2(110f, 42f), new Vector2(200f, 22f), "İsabet: %85", 14, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));
            SetSerializedRef(pred, "_hitChanceText", hitObj.GetComponent<TextMeshProUGUI>());

            var dmgObj = CreateTextObj("DmgText", predObj.transform, new Vector2(110f, 18f), new Vector2(200f, 22f), "Hasar: 14 - 20", 13, FontStyles.Normal, new Color(0.45f, 0.15f, 0.10f));
            SetSerializedRef(pred, "_damageEstimateText", dmgObj.GetComponent<TextMeshProUGUI>());

            predObj.SetActive(false);
        }

        private static void BuildResultUI(Transform parent, CombatTurnManager turnManager)
        {
            var resultObj = new GameObject("CombatResultUI");
            resultObj.transform.SetParent(parent, false);
            var rect = resultObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(500f, 300f);

            var bg = resultObj.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.08f, 0.07f, 0.95f); // Koyu parsomen/tas zemin

            var result = resultObj.AddComponent<CombatResultUI>();
            SetSerializedRef(result, "_panelRoot", resultObj);
            SetSerializedRef(result, "_turnManager", turnManager);

            var titleObj = CreateTextObj("ResultTitle", resultObj.transform, new Vector2(250f, 240f), new Vector2(450f, 40f), "ZAFER!", 32, FontStyles.Bold, new Color(0.20f, 0.65f, 0.35f));
            SetSerializedRef(result, "_titleText", titleObj.GetComponent<TextMeshProUGUI>());

            var descObj = CreateTextObj("ResultDesc", resultObj.transform, new Vector2(250f, 160f), new Vector2(440f, 80f), "Kervan basariyla savundu.", 16, FontStyles.Normal, new Color(0.90f, 0.85f, 0.75f));
            SetSerializedRef(result, "_descriptionText", descObj.GetComponent<TextMeshProUGUI>());

            var btn = CreateButton("ContinueButton", resultObj.transform, new Vector2(250f, 50f), new Vector2(200f, 50f), "DEVAM ET", new Color(0.855f, 0.647f, 0.125f));
            SetSerializedRef(result, "_continueButton", btn);

            resultObj.SetActive(false);
        }

        #region UI Olusturucu Yardimcilari

        private static GameObject CreateTextObj(string name, Transform parent, Vector2 pos, Vector2 size, string text, float fontSize, FontStyles style, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;

            return obj;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 pos, Vector2 size, string label, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;

            var img = obj.AddComponent<Image>();
            img.color = color;

            var btn = obj.AddComponent<Button>();

            CreateTextObj("Label", obj.transform, new Vector2(size.x * 0.5f, size.y * 0.5f), size, label, 16, FontStyles.Bold, Color.white);

            return btn;
        }

        private static Slider CreateBar(string name, Transform parent, Vector2 pos, Vector2 size, Color fillColor)
        {
            var sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);
            var rect = sliderObj.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;

            var slider = sliderObj.AddComponent<Slider>();

            // Arka plan
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // Dolgu Alani
            var fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            var fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = fillColor;

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImg;

            return slider;
        }

        private static void SetSerializedRef(Object target, string fieldName, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        #endregion

        private static CombatUnit CreateUnitObject(string name, UnitDataSO data, HexCoordinates coords, bool isPlayer, Color placeholderColor)
        {
            var unitObj = new GameObject(name);
            var sr = unitObj.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Entities";

            if (data != null && data.DefaultSprite != null)
            {
                sr.sprite = data.DefaultSprite;
                sr.color = Color.white;
                // Düşmanlar oyuncuya (sola/güneybatıya) baksın diye x ekseninde çevrilir
                if (!isPlayer)
                {
                    sr.flipX = true;
                }
            }
            else
            {
                sr.sprite = CreatePlaceholderSprite();
                sr.color = placeholderColor;
            }

            var unit = unitObj.AddComponent<CombatUnit>();
            unit.Initialize(data, coords, isPlayer);

            return unit;
        }

        private static Sprite CreatePlaceholderSprite()
        {
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
                            tex.SetPixel(x, y, new Color(0.12f, 0.10f, 0.08f, 1f));
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
