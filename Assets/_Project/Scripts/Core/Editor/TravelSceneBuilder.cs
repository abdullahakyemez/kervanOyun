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
    public static class TravelSceneBuilder
    {
        [MenuItem("Kervan/Olustur/Seyahat Sahnesini Kur")]
        public static void BuildTravelScene()
        {
            string scenePath = "Assets/_Project/Scenes/TravelScene.unity";

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Kamera
            var cameraObj = new GameObject("Main Camera");
            var cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.60f, 0.50f, 0.38f); // Bozkir sarimsi toprak tonu #998061
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            cameraObj.tag = "MainCamera";

            // 2. Seyahat Yoneticisi
            var managerObj = new GameObject("TravelManager");
            var travelManager = managerObj.AddComponent<TravelManager>();

            var sogutNode = AssetDatabase.LoadAssetAtPath<LocationNodeSO>("Assets/_Project/ScriptableObjects/Nodes/Node_Sogut.asset");
            var routeSogutBilecik = AssetDatabase.LoadAssetAtPath<TravelRouteSO>("Assets/_Project/ScriptableObjects/Routes/Route_Sogut_Bilecik.asset");

            SetSerializedRef(travelManager, "_currentLocation", sogutNode);
            SetSerializedRef(travelManager, "_activeRoute", routeSogutBilecik);
            SetSerializedField(travelManager, "_isTraveling", true);

            // 3. 2D Side-View Kervan Gorseli (Placeholder)
            BuildCaravanVisual(new Vector3(0f, -1.5f, 0f));

            // 4. Mobil UI Katmani
            BuildTravelUI(travelManager);

            // Sahneyi kaydet
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AddSceneToBuildSettings(scenePath);

            Debug.Log($"[Kervan] Seyahat sahnesi (TravelScene.unity) basariyla olusturuldu: {scenePath}");
        }

        private static void BuildCaravanVisual(Vector3 position)
        {
            var caravanObj = new GameObject("CaravanVisual");
            caravanObj.transform.position = position;

            // Zemin Cizgisi
            var groundObj = new GameObject("GroundLine");
            groundObj.transform.SetParent(caravanObj.transform, false);
            var gSr = groundObj.AddComponent<SpriteRenderer>();
            gSr.sortingLayerName = "Background";
            gSr.color = new Color(0.40f, 0.30f, 0.20f);
            gSr.sprite = CreateBoxSprite(100, 10);
            groundObj.transform.localScale = new Vector3(15f, 0.2f, 1f);

            // Kervan Ati / Devesi
            var horseObj = new GameObject("Horse");
            horseObj.transform.SetParent(caravanObj.transform, false);
            var hSr = horseObj.AddComponent<SpriteRenderer>();
            hSr.sortingLayerName = "Entities";
            hSr.color = new Color(0.35f, 0.22f, 0.15f); // Yagiz At kahvesi
            hSr.sprite = CreateBoxSprite(48, 48);
            horseObj.transform.localPosition = new Vector3(1.2f, 0.8f, 0f);

            // Yuk Arabasi
            var wagonObj = new GameObject("Wagon");
            wagonObj.transform.SetParent(caravanObj.transform, false);
            var wSr = wagonObj.AddComponent<SpriteRenderer>();
            wSr.sortingLayerName = "Entities";
            wSr.color = new Color(0.50f, 0.38f, 0.25f); // Ahsap araba
            wSr.sprite = CreateBoxSprite(64, 40);
            wagonObj.transform.localPosition = new Vector3(-0.8f, 0.7f, 0f);
        }

        private static void BuildTravelUI(TravelManager travelManager)
        {
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            var canvasObj = new GameObject("TravelCanvas");
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

            var travelUI = canvasObj.AddComponent<TravelUI>();
            SetSerializedRef(travelUI, "_travelManager", travelManager);

            // --- 1. UST BOLGE (Top Bar: 1920x100) ---
            var topBarObj = new GameObject("TopBar");
            topBarObj.transform.SetParent(safeAreaObj.transform, false);
            var topRect = topBarObj.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.sizeDelta = new Vector2(0f, 90f);
            var topBg = topBarObj.AddComponent<Image>();
            topBg.color = new Color(0.12f, 0.10f, 0.08f, 0.90f);

            // Erzak Gostergesi
            var foodSlider = CreateBar("FoodBar", topBarObj.transform, new Vector2(200f, -45f), new Vector2(220f, 22f), new Color(0.20f, 0.55f, 0.30f));
            SetSerializedRef(travelUI, "_foodSlider", foodSlider);
            var foodText = CreateText("FoodText", topBarObj.transform, new Vector2(200f, -45f), new Vector2(200f, 24f), "Erzak: 20 Gün", 15, Color.white);
            SetSerializedRef(travelUI, "_foodText", foodText);

            // Akce Gostergesi
            var akceText = CreateText("AkceText", topBarObj.transform, new Vector2(500f, -45f), new Vector2(200f, 30f), "500 Akçe", 20, new Color(0.855f, 0.647f, 0.125f));
            SetSerializedRef(travelUI, "_akceText", akceText);

            // Tarih / Takvim
            var calText = CreateText("CalendarText", topBarObj.transform, new Vector2(960f, -35f), new Vector2(300f, 32f), "1. Gün", 24, Color.white);
            SetSerializedRef(travelUI, "_calendarText", calText);

            // Rota Sartlari
            var routeText = CreateText("RouteInfoText", topBarObj.transform, new Vector2(960f, -65f), new Vector2(400f, 22f), "Bozkır Yolu - Açık", 14, new Color(0.75f, 0.70f, 0.60f));
            SetSerializedRef(travelUI, "_routeInfoText", routeText);

            // --- 2. ALT BOLGE (Bottom Bar: Ilerleme ve Mola) ---
            var bottomBarObj = new GameObject("BottomBar");
            bottomBarObj.transform.SetParent(safeAreaObj.transform, false);
            var botRect = bottomBarObj.AddComponent<RectTransform>();
            botRect.anchorMin = new Vector2(0f, 0f);
            botRect.anchorMax = new Vector2(1f, 0f);
            botRect.pivot = new Vector2(0.5f, 0f);
            botRect.sizeDelta = new Vector2(0f, 130f);
            var botBg = bottomBarObj.AddComponent<Image>();
            botBg.color = new Color(0.12f, 0.10f, 0.08f, 0.90f);

            // Rota Ilerleme Slider
            var progSlider = CreateBar("ProgressSlider", bottomBarObj.transform, new Vector2(960f, 75f), new Vector2(700f, 20f), new Color(0.855f, 0.647f, 0.125f));
            SetSerializedRef(travelUI, "_progressSlider", progSlider);

            var originText = CreateText("OriginText", bottomBarObj.transform, new Vector2(550f, 105f), new Vector2(200f, 25f), "Söğüt", 18, Color.white);
            SetSerializedRef(travelUI, "_originNodeText", originText);

            var destText = CreateText("DestText", bottomBarObj.transform, new Vector2(1370f, 105f), new Vector2(200f, 25f), "Bilecik", 18, Color.white);
            SetSerializedRef(travelUI, "_destNodeText", destText);

            // Mola / Devam Butonu
            var pauseBtn = CreateButton("PauseButton", bottomBarObj.transform, new Vector2(960f, 35f), new Vector2(220f, 44f), "MOLA VER", new Color(0.35f, 0.25f, 0.18f));
            SetSerializedRef(travelUI, "_pauseResumeButton", pauseBtn);
            SetSerializedRef(travelUI, "_pauseResumeText", pauseBtn.GetComponentInChildren<TextMeshProUGUI>());

            // --- 3. OLAY MODAL PENCERESI (Pop-up) ---
            var modalObj = new GameObject("EventModal");
            modalObj.transform.SetParent(safeAreaObj.transform, false);
            var mRect = modalObj.AddComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.5f, 0.5f);
            mRect.anchorMax = new Vector2(0.5f, 0.5f);
            mRect.pivot = new Vector2(0.5f, 0.5f);
            mRect.sizeDelta = new Vector2(550f, 280f);
            var mBg = modalObj.AddComponent<Image>();
            mBg.color = new Color(0.95f, 0.90f, 0.81f, 0.98f); // Parsomen
            SetSerializedRef(travelUI, "_eventModalRoot", modalObj);

            var mTitle = CreateText("ModalTitle", modalObj.transform, new Vector2(275f, 235f), new Vector2(500f, 35f), "Yol Olayı", 24, new Color(0.12f, 0.10f, 0.08f));
            SetSerializedRef(travelUI, "_eventTitleText", mTitle);

            var mDesc = CreateText("ModalDesc", modalObj.transform, new Vector2(275f, 150f), new Vector2(500f, 100f), "Hadise aciklamasi burada gorunecek.", 16, new Color(0.25f, 0.20f, 0.15f));
            SetSerializedRef(travelUI, "_eventDescText", mDesc);

            var mCloseBtn = CreateButton("ModalCloseButton", modalObj.transform, new Vector2(275f, 45f), new Vector2(180f, 44f), "DEVAM ET", new Color(0.165f, 0.45f, 0.25f));
            SetSerializedRef(travelUI, "_eventCloseButton", mCloseBtn);

            modalObj.SetActive(false);

            // --- 4. PUSU UYARI PANELI ---
            var ambushObj = new GameObject("AmbushPanel");
            ambushObj.transform.SetParent(safeAreaObj.transform, false);
            var aRect = ambushObj.AddComponent<RectTransform>();
            aRect.anchorMin = new Vector2(0.5f, 0.5f);
            aRect.anchorMax = new Vector2(0.5f, 0.5f);
            aRect.sizeDelta = new Vector2(550f, 260f);
            var aBg = ambushObj.AddComponent<Image>();
            aBg.color = new Color(0.15f, 0.10f, 0.08f, 0.98f);
            SetSerializedRef(travelUI, "_ambushPanelRoot", ambushObj);

            CreateText("AmbushTitle", ambushObj.transform, new Vector2(275f, 210f), new Vector2(500f, 40f), "PUSUYA DÜŞÜLDÜ!", 28, new Color(0.75f, 0.15f, 0.10f));
            CreateText("AmbushDesc", ambushObj.transform, new Vector2(275f, 135f), new Vector2(500f, 70f), "Haydutlar kervanın önünü kesti!\nMuhafızlar savaş düzeni alıyor...", 17, Color.white);

            var fightBtn = CreateButton("FightButton", ambushObj.transform, new Vector2(275f, 45f), new Vector2(240f, 48f), "SAVAŞ DÜZENİ AL", new Color(0.75f, 0.15f, 0.10f));
            SetSerializedRef(travelUI, "_enterCombatButton", fightBtn);

            ambushObj.SetActive(false);
        }

        #region Yardimci Metotlar

        private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 pos, Vector2 size, string text, float fontSize, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
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
            CreateText("Label", obj.transform, new Vector2(size.x * 0.5f, size.y * 0.5f), size, label, 16, Color.white);
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
            bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

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

        private static Sprite CreateBoxSprite(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
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

        private static void SetSerializedField(Object target, string fieldName, object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                if (value is bool b) prop.boolValue = b;
                else if (value is int i) prop.intValue = i;
                else if (value is float f) prop.floatValue = f;
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
