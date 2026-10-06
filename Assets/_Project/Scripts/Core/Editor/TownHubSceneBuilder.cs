#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Kervan.Core;
using Kervan.IntelSystem;
using Kervan.TradeAndTravel;
using Kervan.UI;

namespace Kervan.EditorTools
{
    public static class TownHubSceneBuilder
    {
        [MenuItem("Kervan/Olustur/Sehir ve Han Sahnesini Kur")]
        public static void BuildTownHubScene()
        {
            string scenePath = "Assets/_Project/Scenes/TownHubScene.unity";

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Kamera (Sıcak Han / Kalker tonu #C2A676)
            var cameraObj = new GameObject("Main Camera");
            var cam = cameraObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.76f, 0.65f, 0.46f);
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            cameraObj.tag = "MainCamera";

            // 2. Sistem Yöneticileri
            var managersObj = new GameObject("MetaManagers");
            var repManager = managersObj.AddComponent<ReputationManager>();
            var questManager = managersObj.AddComponent<QuestManager>();
            var shopManager = managersObj.AddComponent<ShopManager>();

            var caravanObj = new GameObject("CaravanManager");
            var caravanManager = caravanObj.AddComponent<CaravanManager>();

            // 3. UI Katmanı
            BuildHubUI(repManager, questManager, shopManager, caravanManager);

            // Sahneyi kaydet
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AddSceneToBuildSettings(scenePath);

            Debug.Log($"[Kervan] Şehir ve Han sahnesi (TownHubScene.unity) başarıyla oluşturuldu: {scenePath}");
        }

        private static void BuildHubUI(ReputationManager repMgr, QuestManager qMgr, ShopManager sMgr, CaravanManager caravan)
        {
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            var canvasObj = new GameObject("TownHubCanvas");
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

            var hubUI = canvasObj.AddComponent<TownHubUI>();
            SetSerializedRef(hubUI, "_reputationManager", repMgr);
            SetSerializedRef(hubUI, "_questManager", qMgr);
            SetSerializedRef(hubUI, "_shopManager", sMgr);

            // --- 1. ÜST ÇUBUK (1800x90 px) ---
            var headerPanel = CreatePanel("HeaderPanel", safeAreaObj.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(1800f, 90f),
                new Color(0.12f, 0.10f, 0.08f, 0.95f));

            var titleText = CreateText("CityTitle", headerPanel.transform, new Vector2(400f, 45f), new Vector2(700f, 40f), "SÖĞÜT HANI & BEYLİK DİVANI", 26, FontStyles.Bold, new Color(0.855f, 0.647f, 0.125f));
            SetSerializedRef(hubUI, "_cityTitleText", titleText);

            var akceText = CreateText("AkceText", headerPanel.transform, new Vector2(1500f, 45f), new Vector2(400f, 36f), "Kese: 500 Akçe", 22, FontStyles.Bold, new Color(0.855f, 0.647f, 0.125f));
            SetSerializedRef(hubUI, "_caravanAkceText", akceText);

            var notifText = CreateText("NotifText", headerPanel.transform, new Vector2(950f, 45f), new Vector2(600f, 32f), "Uç beyliği ordugâhına hoş geldiniz.", 16, FontStyles.Italic, Color.white);
            SetSerializedRef(hubUI, "_statusNotificationText", notifText);

            // --- 2. SOL SEKME MENÜSÜ (320x800 px) ---
            var navPanel = CreatePanel("NavPanel", safeAreaObj.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(60f, -40f), new Vector2(320f, 800f),
                new Color(0.18f, 0.14f, 0.10f, 0.95f));

            var tab1Btn = CreateButton("Tab_Divan", navPanel.transform, new Vector2(160f, 700f), new Vector2(280f, 70f), "BEYLİK DİVANI", new Color(0.45f, 0.32f, 0.18f));
            var tab2Btn = CreateButton("Tab_Intel", navPanel.transform, new Vector2(160f, 600f), new Vector2(280f, 70f), "HAN FISILTILARI", new Color(0.35f, 0.25f, 0.18f));
            var tab3Btn = CreateButton("Tab_Shop", navPanel.transform, new Vector2(160f, 500f), new Vector2(280f, 70f), "SABİT DÜKKÂN", new Color(0.25f, 0.20f, 0.15f));

            SetSerializedRef(hubUI, "_tabDivanButton", tab1Btn);
            SetSerializedRef(hubUI, "_tabIntelButton", tab2Btn);
            SetSerializedRef(hubUI, "_tabShopButton", tab3Btn);

            var divanFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/UI/UI_Frame_Divan.png");

            // --- 3. İÇERİK ALANI (1420x800 px) ---
            var contentArea = CreatePanel("ContentArea", safeAreaObj.transform,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-60f, -40f), new Vector2(1420f, 800f),
                Color.white, divanFrame);

            // Panel A: Divan & İtibar
            var panelDivan = CreatePanel("Panel_Divan", contentArea.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1380f, 760f), Color.clear);
            SetSerializedRef(hubUI, "_panelDivan", panelDivan);

            CreateText("DivanTitle", panelDivan.transform, new Vector2(690f, 720f), new Vector2(1200f, 40f), "UÇ BEYLİĞİ DİVANI & BEYLİK NÜFUZLARI", 24, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));

            var repOttoman = CreateText("Rep_Ottoman", panelDivan.transform, new Vector2(690f, 650f), new Vector2(1100f, 35f), "Osmanlı Uç Beyliği: +30 [Dost] (Vergi İndirimi: %25)", 20, FontStyles.Normal, new Color(0.15f, 0.40f, 0.15f));
            var repSteppe = CreateText("Rep_Steppe", panelDivan.transform, new Vector2(690f, 600f), new Vector2(1100f, 35f), "Germiyanoğlu Beyliği: +10 [Nötr] (Standart Vergi)", 20, FontStyles.Normal, new Color(0.25f, 0.25f, 0.25f));
            var repSeljuk = CreateText("Rep_Seljuk", panelDivan.transform, new Vector2(690f, 550f), new Vector2(1100f, 35f), "Karamanoğlu & Selçuklu: +5 [Nötr] (Standart Vergi)", 20, FontStyles.Normal, new Color(0.25f, 0.25f, 0.25f));
            var repByz = CreateText("Rep_Byzantine", panelDivan.transform, new Vector2(690f, 500f), new Vector2(1100f, 35f), "Doğu Roma (Tekfurlar): -15 [Şüpheli] (Ceza Vergisi: %25)", 20, FontStyles.Normal, new Color(0.55f, 0.15f, 0.12f));

            SetSerializedRef(hubUI, "_repOttomanText", repOttoman);
            SetSerializedRef(hubUI, "_repSteppeText", repSteppe);
            SetSerializedRef(hubUI, "_repSeljukText", repSeljuk);
            SetSerializedRef(hubUI, "_repByzantineText", repByz);

            CreateText("DivanActionHeader", panelDivan.transform, new Vector2(690f, 430f), new Vector2(1200f, 35f), "TOPLANAN İSTİHBARATLARI DİVANA ARZ ET", 20, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));

            // Divana sunulabilecek örnek istihbarat kartı
            var intelAsset = AssetDatabase.LoadAssetAtPath<IntelDataSO>("Assets/_Project/ScriptableObjects/Intels/Intel_BilecikKonvoy.asset");
            var submitCard = CreatePanel("SubmitCard_Bilecik", panelDivan.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -80f), new Vector2(1200f, 120f), new Color(0.88f, 0.82f, 0.70f));

            CreateText("CardTitle", submitCard.transform, new Vector2(400f, 80f), new Vector2(700f, 35f), "Bilecik Tekfurunun İkmal Konvoyu", 20, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));
            CreateText("CardDesc", submitCard.transform, new Vector2(400f, 40f), new Vector2(700f, 30f), "Ödül: 200 Akçe | +15 Osmanlı Nüfuzu | %2 Vergi Beratı", 16, FontStyles.Normal, new Color(0.35f, 0.25f, 0.18f));

            var submitBtn = CreateButton("SubmitBtn", submitCard.transform, new Vector2(1050f, 60f), new Vector2(220f, 60f), "DİVANA SUN", new Color(0.165f, 0.45f, 0.25f));
            submitBtn.onClick.AddListener(() =>
            {
                if (intelAsset != null) hubUI.TrySubmitIntelFromUI(intelAsset);
            });

            // Panel B: Han Fısıltıları (İstihbarat Toplama)
            var panelIntel = CreatePanel("Panel_Intel", contentArea.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1380f, 760f), Color.clear);
            SetSerializedRef(hubUI, "_panelIntel", panelIntel);

            CreateText("IntelTitle", panelIntel.transform, new Vector2(690f, 720f), new Vector2(1200f, 40f), "KÖŞEDEKİ FISILTILAR (CASUSLUK & İSTİHBARAT TOPLAMA)", 24, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));
            CreateText("IntelNotice", panelIntel.transform, new Vector2(690f, 670f), new Vector2(1200f, 30f), "Meyhane ve kervansaray köşelerinde bilgi toplarken yakalanırsanız muhafızlara rüşvet vermek zorunda kalırsınız!", 16, FontStyles.Italic, new Color(0.55f, 0.15f, 0.12f));

            // Bilgi Toplama Kartı 1: Bilecik
            var gatherCard1 = CreatePanel("GatherCard_Bilecik", panelIntel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 100f), new Vector2(1200f, 120f), new Color(0.88f, 0.82f, 0.70f));
            CreateText("GTitle1", gatherCard1.transform, new Vector2(400f, 80f), new Vector2(700f, 35f), "Bilecik Tekfurunun İkmal Konvoyu", 20, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));
            CreateText("GDesc1", gatherCard1.transform, new Vector2(400f, 40f), new Vector2(700f, 30f), "Tehlike: %20 Yakalanma Riski | Hedef: Osmanlı Beyliği", 16, FontStyles.Normal, new Color(0.35f, 0.25f, 0.18f));
            var gatherBtn1 = CreateButton("GatherBtn1", gatherCard1.transform, new Vector2(1050f, 60f), new Vector2(220f, 60f), "FISILTI DİNLE", new Color(0.65f, 0.45f, 0.15f));
            gatherBtn1.onClick.AddListener(() =>
            {
                if (intelAsset != null) hubUI.TryGatherIntelFromUI(intelAsset);
            });

            // Bilgi Toplama Kartı 2: İznik
            var iznikAsset = AssetDatabase.LoadAssetAtPath<IntelDataSO>("Assets/_Project/ScriptableObjects/Intels/Intel_IznikSurZafiyeti.asset");
            var gatherCard2 = CreatePanel("GatherCard_Iznik", panelIntel.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -40f), new Vector2(1200f, 120f), new Color(0.88f, 0.82f, 0.70f));
            CreateText("GTitle2", gatherCard2.transform, new Vector2(400f, 80f), new Vector2(700f, 35f), "İznik Doğu Kapısı Savunma Zaafı", 20, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));
            CreateText("GDesc2", gatherCard2.transform, new Vector2(400f, 40f), new Vector2(700f, 30f), "Tehlike: %35 Yakalanma Riski | Hedef: Osmanlı Beyliği", 16, FontStyles.Normal, new Color(0.35f, 0.25f, 0.18f));
            var gatherBtn2 = CreateButton("GatherBtn2", gatherCard2.transform, new Vector2(1050f, 60f), new Vector2(220f, 60f), "FISILTI DİNLE", new Color(0.65f, 0.45f, 0.15f));
            gatherBtn2.onClick.AddListener(() =>
            {
                if (iznikAsset != null) hubUI.TryGatherIntelFromUI(iznikAsset);
            });

            // Panel C: Sabit Dükkân / Han Tezgâhı
            var panelShop = CreatePanel("Panel_Shop", contentArea.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1380f, 760f), Color.clear);
            SetSerializedRef(hubUI, "_panelShop", panelShop);

            CreateText("ShopTitle", panelShop.transform, new Vector2(690f, 720f), new Vector2(1200f, 40f), "SÖĞÜT HAN TEZGÂHI & MÜLKİYETİ", 24, FontStyles.Bold, new Color(0.12f, 0.10f, 0.08f));

            var shopStatus = CreateText("ShopStatus", panelShop.transform, new Vector2(690f, 580f), new Vector2(1100f, 140f),
                "<b>Söğüt Han Tezgâhı</b>\nBedel: 200 Akçe | Gerekli İtibar: En az 20 (Dost)\nGünlük Net Pasif Getiri: +20 Akçe/Gün", 22, FontStyles.Normal, new Color(0.15f, 0.12f, 0.10f));
            SetSerializedRef(hubUI, "_shopStatusText", shopStatus);

            var shopIncome = CreateText("ShopIncome", panelShop.transform, new Vector2(690f, 460f), new Vector2(1100f, 50f),
                "Kasada Biriken Kâr: <b>0 Akçe</b>", 24, FontStyles.Bold, new Color(0.855f, 0.647f, 0.125f));
            SetSerializedRef(hubUI, "_shopIncomeText", shopIncome);

            var buyShopBtn = CreateButton("BuyShopBtn", panelShop.transform, new Vector2(690f, 320f), new Vector2(380f, 70f), "DÜKKÂNI AL (200 Akçe)", new Color(0.165f, 0.45f, 0.25f));
            SetSerializedRef(hubUI, "_buyShopButton", buyShopBtn);

            var collectBtn = CreateButton("CollectBtn", panelShop.transform, new Vector2(690f, 220f), new Vector2(380f, 70f), "KASAYI TAHSİL ET", new Color(0.65f, 0.45f, 0.15f));
            SetSerializedRef(hubUI, "_collectIncomeButton", collectBtn);

            // Başlangıçta panelleri ayarla
            panelDivan.SetActive(true);
            panelIntel.SetActive(false);
            panelShop.SetActive(false);
        }

        #region UI Yardimcilar

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
            CreateText("Label", obj.transform, new Vector2(size.x * 0.5f, size.y * 0.5f), size, label, 16, FontStyles.Bold, Color.white);
            return btn;
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
