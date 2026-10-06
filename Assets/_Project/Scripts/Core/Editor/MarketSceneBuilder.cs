#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Kervan.TradeAndTravel;
using Kervan.UI;

namespace Kervan.EditorTools
{
    public static class MarketSceneBuilder
    {
        [MenuItem("Kervan/Olustur/Pazar Sahnesini Kur")]
        public static void BuildMarketScene()
        {
            string scenePath = "Assets/_Project/Scenes/MarketScene.unity";

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Kamera (Kalker Tasi / Han zemini rengi #D4B886)
            var cameraObj = new GameObject("Main Camera");
            var cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.83f, 0.72f, 0.52f); // #D4B886
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            cameraObj.tag = "MainCamera";

            // 2. Pazar Yoneticisi
            var marketObj = new GameObject("CityMarket");
            var cityMarket = marketObj.AddComponent<CityMarket>();

            var bursaMarketData = AssetDatabase.LoadAssetAtPath<CityMarketDataSO>("Assets/_Project/ScriptableObjects/Markets/Market_Bursa.asset");
            SetSerializedRef(cityMarket, "_marketData", bursaMarketData);

            // 3. UI Katmani
            BuildMarketUI(cityMarket);

            // Sahneyi kaydet
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AddSceneToBuildSettings(scenePath);

            Debug.Log($"[Kervan] Pazar sahnesi (MarketScene.unity) basariyla olusturuldu: {scenePath}");
        }

        private static void BuildMarketUI(CityMarket cityMarket)
        {
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            var canvasObj = new GameObject("MarketCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingLayerName = "ScreenUI";

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            var safeAreaObj = new GameObject("SafeArea");
            safeAreaObj.transform.SetParent(canvasObj.transform, false);
            var safeAreaRect = safeAreaObj.AddComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.sizeDelta = Vector2.zero;
            safeAreaObj.AddComponent<SafeAreaFitter>();

            var marketUI = canvasObj.AddComponent<MarketUI>();
            SetSerializedRef(marketUI, "_cityMarket", cityMarket);

            var marketFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/UI/UI_Frame_Market.png");

            // --- 1. SOL PANEL: KERVAN ENVANTERI (500x700 px) ---
            var leftPanel = CreatePanel("LeftPanel_Caravan", safeAreaObj.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(60f, 40f), new Vector2(500f, 720f),
                Color.white, marketFrame);

            CreateText("CaravanTitle", leftPanel.transform, new Vector2(250f, 680f), new Vector2(460f, 40f), "KERVAN YÜKÜ & KASA", 22, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));

            var akceText = CreateText("CaravanAkce", leftPanel.transform, new Vector2(250f, 640f), new Vector2(460f, 32f), "500 Akçe", 20, FontStyles.Bold, new Color(0.855f, 0.647f, 0.125f));
            SetSerializedRef(marketUI, "_caravanAkceText", akceText);

            var weightSlider = CreateBar("WeightSlider", leftPanel.transform, new Vector2(250f, 600f), new Vector2(400f, 20f), new Color(0.35f, 0.25f, 0.18f));
            SetSerializedRef(marketUI, "_weightSlider", weightSlider);

            var weightText = CreateText("WeightText", leftPanel.transform, new Vector2(250f, 570f), new Vector2(400f, 24f), "120 / 300 Kg", 15, FontStyles.Normal, new Color(0.30f, 0.25f, 0.20f));
            SetSerializedRef(marketUI, "_weightText", weightText);

            // --- 2. SAG PANEL: SEHIR TEZGAHI (1100x700 px) ---
            var rightPanel = CreatePanel("RightPanel_CityMarket", safeAreaObj.transform,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-60f, 40f), new Vector2(1100f, 720f),
                Color.white, marketFrame);

            var marketTitle = CreateText("MarketTitle", rightPanel.transform, new Vector2(550f, 680f), new Vector2(1000f, 40f), "BURSA PAZARI (Vergi: %8)", 24, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));
            SetSerializedRef(marketUI, "_marketTitleText", marketTitle);

            // Ornek Mal Secim Butonlari (Ipek, Demir, Celik, Kurk, Bugday, Baharat)
            var ipekItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Ipek.asset");
            var demirItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Demir.asset");
            var celikItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Celik.asset");
            var kurkItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Kurk.asset");
            var bugdayItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Bugday.asset");
            var baharatItem = AssetDatabase.LoadAssetAtPath<ItemDataSO>("Assets/_Project/ScriptableObjects/Items/Item_Baharat.asset");

            CreateItemRow("Row_Ipek", rightPanel.transform, new Vector2(550f, 610f), "Bursa İpeği", "Stok: 80 | Alış: 126 Akçe | Satış: 108 Akçe [BOL / UCUZ]", marketUI, ipekItem);
            CreateItemRow("Row_Demir", rightPanel.transform, new Vector2(550f, 545f), "Bilecik Demiri", "Stok: 40 | Alış: 82 Akçe | Satış: 70 Akçe [KIT / PAHALI]", marketUI, demirItem);
            CreateItemRow("Row_Celik", rightPanel.transform, new Vector2(550f, 480f), "Şam Çeliği", "Stok: 20 | Alış: 145 Akçe | Satış: 130 Akçe [TALEP YÜKSEK]", marketUI, celikItem);
            CreateItemRow("Row_Kurk", rightPanel.transform, new Vector2(550f, 415f), "Bozkır Kürkü", "Stok: 30 | Alış: 105 Akçe | Satış: 92 Akçe [LÜKS EMKİA]", marketUI, kurkItem);
            CreateItemRow("Row_Bugday", rightPanel.transform, new Vector2(550f, 350f), "Konya Buğdayı", "Stok: 60 | Alış: 24 Akçe | Satış: 20 Akçe [STANDART]", marketUI, bugdayItem);
            CreateItemRow("Row_Baharat", rightPanel.transform, new Vector2(550f, 285f), "Doğu Baharatı", "Stok: 20 | Alış: 142 Akçe | Satış: 122 Akçe [STANDART]", marketUI, baharatItem);

            // --- 3. ALT ISLEM PANELI (1700x160 px) ---
            var bottomPanel = CreatePanel("BottomPanel_Trade", safeAreaObj.transform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(1700f, 160f),
                new Color(0.12f, 0.10f, 0.08f, 0.95f));

            var selItemName = CreateText("SelectedItemName", bottomPanel.transform, new Vector2(250f, 120f), new Vector2(400f, 32f), "Seçili: Bursa İpeği", 20, FontStyles.Bold, Color.white);
            SetSerializedRef(marketUI, "_selectedItemNameText", selItemName);

            var qSlider = CreateBar("QuantitySlider", bottomPanel.transform, new Vector2(300f, 60f), new Vector2(450f, 22f), new Color(0.855f, 0.647f, 0.125f));
            qSlider.minValue = 1;
            qSlider.maxValue = 50;
            qSlider.value = 1;
            SetSerializedRef(marketUI, "_quantitySlider", qSlider);

            var qText = CreateText("QuantityText", bottomPanel.transform, new Vector2(570f, 60f), new Vector2(120f, 28f), "1 Adet", 18, FontStyles.Bold, Color.white);
            SetSerializedRef(marketUI, "_quantityValueText", qText);

            var totalPrText = CreateText("TotalPriceText", bottomPanel.transform, new Vector2(900f, 85f), new Vector2(450f, 30f), "Alış: 126 Akçe | Satış: 108 Akçe", 18, FontStyles.Bold, new Color(0.855f, 0.647f, 0.125f));
            SetSerializedRef(marketUI, "_totalPriceText", totalPrText);

            var totalWText = CreateText("TotalWeightText", bottomPanel.transform, new Vector2(900f, 45f), new Vector2(400f, 24f), "Ağırlık: ±0.5 Kg", 15, FontStyles.Normal, new Color(0.70f, 0.65f, 0.55f));
            SetSerializedRef(marketUI, "_totalWeightChangeText", totalWText);

            // Satin Al Butonu (Yesil)
            var buyBtn = CreateButton("BuyButton", bottomPanel.transform, new Vector2(1250f, 80f), new Vector2(180f, 54f), "SATIN AL", new Color(0.165f, 0.45f, 0.25f));
            SetSerializedRef(marketUI, "_buyButton", buyBtn);

            // Sat Butonu (Koyu Al)
            var sellBtn = CreateButton("SellButton", bottomPanel.transform, new Vector2(1460f, 80f), new Vector2(180f, 54f), "SAT", new Color(0.55f, 0.15f, 0.12f));
            SetSerializedRef(marketUI, "_sellButton", sellBtn);

            // Pazardan Ayril Butonu
            var leaveBtn = CreateButton("LeaveButton", bottomPanel.transform, new Vector2(1630f, 80f), new Vector2(120f, 54f), "AYRIL", new Color(0.35f, 0.25f, 0.18f));
            SetSerializedRef(marketUI, "_leaveMarketButton", leaveBtn);
        }

        private static void CreateItemRow(string name, Transform parent, Vector2 pos, string title, string sub, MarketUI marketUI, ItemDataSO item)
        {
            var rowObj = new GameObject(name);
            rowObj.transform.SetParent(parent, false);
            var rect = rowObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1000f, 65f);
            rect.anchoredPosition = pos;

            var img = rowObj.AddComponent<Image>();
            img.color = new Color(0.88f, 0.82f, 0.70f, 0.90f);

            var btn = rowObj.AddComponent<Button>();
            btn.onClick.AddListener(() => marketUI.SelectItem(item));

            CreateText("Title", rowObj.transform, new Vector2(250f, 40f), new Vector2(450f, 26f), title, 18, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));
            CreateText("Sub", rowObj.transform, new Vector2(500f, 18f), new Vector2(950f, 22f), sub, 14, FontStyles.Normal, new Color(0.30f, 0.25f, 0.20f));
        }

        #region UI Yardimcilari

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color, Sprite sprite = null)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = obj.AddComponent<Image>();
            img.color = color;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Simple;
            }
            return obj;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 pos, Vector2 size, string text, float fontSize, FontStyles style, Color color)
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
            return tmp;
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
            CreateText("Label", obj.transform, new Vector2(size.x * 0.5f, size.y * 0.5f), size, label, 15, FontStyles.Bold, Color.white);
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

            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);

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

        #endregion
    }
}
#endif
