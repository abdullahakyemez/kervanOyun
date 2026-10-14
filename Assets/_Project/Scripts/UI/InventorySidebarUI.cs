using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.TradeAndTravel;
using Kervan.Audio;
using System.Collections.Generic;

namespace Kervan.UI
{
    /// <summary>
    /// Kervan envanter ve bilgi çekmecesini (Sidebar) yöneten ana kullanıcı arayüzü kontrolcüsü.
    /// Mobil Safe Area (çentik / notch) koruması, dinamik temiz satır üretimi ve sekme yönetimi içerir.
    /// </summary>
    public class InventorySidebarUI : MonoBehaviour
    {
        [Header("Safe Area ve Mobil Ayarlar")]
        [Tooltip("Cihaz çentiğinden (Notch) ve kör alanlardan korunmak için Safe Area payını uygular.")]
        public bool applySafeArea = true;

        [Header("Satır Üretim Modu")]
        [Tooltip("True yapılırsa Inspector'daki prefabları kullanır. False ise (önerilen) sıfır hatasız, taşma yapmayan dinamik satırları üretir.")]
        public bool useCustomPrefabs = false;

        [Header("Ana Paneller ve Butonlar")]
        public GameObject sidebarPanel;
        public GameObject darkOverlay;
        public Button openButton;
        public Button closeButton;

        [Header("Karakter ve Akçe Başlığı (Header)")]
        public Image characterPortrait;
        public TextMeshProUGUI characterNameText;
        public TextMeshProUGUI akceText;

        [Header("Sekme Butonları")]
        public Button tabKervan;
        public Button tabYuk;
        public Button tabGorevler;

        [Header("Sekme İçerik Objeleri")]
        public GameObject contentKervan;
        public GameObject contentYuk;
        public GameObject contentGorevler;

        [Header("Liste Konteynerleri (Scroll İçerikleri)")]
        public Transform kervanUnitsContainer;
        public Transform yukContainer;
        public Transform questContainer;
        [SerializeField] private TextMeshProUGUI _capacityText;

        [Header("Satır Prefabları (İsteğe Bağlı)")]
        public GameObject unitRowPrefab;
        public GameObject itemRowPrefab;
        public GameObject questRowPrefab;

        [Header("Tipografi")]
        [SerializeField] private TMP_FontAsset _fontAsset;

        [Header("Profil Kartları (Portraits)")]
        [SerializeField] private Sprite _leaderSprite;  // Alp (Kıdemli / Lider) -> Ahi_ProfileCard
        [SerializeField] private Sprite _guardSprite;   // Kervan Muhafızı -> Mauhafiz_ProfileCard
        [SerializeField] private Sprite _archerSprite;  // Gazi Okçu -> Okcu_ProfileCard
        [SerializeField] private Sprite _horseSprite;   // Binek Atı -> Horse_ProfileCard
        [SerializeField] private Sprite _muleSprite;    // Yük Katırı -> Mule_ProfileCard
        [SerializeField] private Sprite _camelSprite;   // Kervan Devesi -> Camel_ProfileCard

        [Header("Ticaret Malları İkonları (Item Sprites)")]
        [SerializeField] private Sprite _ipekSprite;    // Bursa İpeği
        [SerializeField] private Sprite _demirSprite;   // Dövme Demir
        [SerializeField] private Sprite _bugdaySprite;  // Anadolu Buğdayı
        [SerializeField] private Sprite _baharatSprite; // Baharat
        [SerializeField] private Sprite _celikSprite;   // Şam Çeliği
        [SerializeField] private Sprite _kurkSprite;    // Kürk

        [Header("Süsleme ve Ayraçlar")]
        [SerializeField] private Sprite _ayiracSprite;     // UI_BG_ayirac
        [SerializeField] private Sprite _questBadgeSprite; // UI_BG_Itemicon (Görev rozeti)

        private Rect _lastSafeArea = Rect.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            AutoBindReferences();
            SetupButtonListeners();
            CleanScrollViewBackgrounds();
        }

        private void Start()
        {
            AutoBindReferences();
            ApplySafeArea();
            CleanScrollViewBackgrounds();

            EnsureLayout(kervanUnitsContainer);
            EnsureLayout(yukContainer);
            EnsureLayout(questContainer);

            RefreshData();
            CloseSidebar();
        }

        private void Update()
        {
            // Unity Device Simulator veya mobil cihaz yön değiştirdiğinde canlı Safe Area güncellemesi
            if (applySafeArea && (_lastSafeArea != Screen.safeArea || _lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height))
            {
                ApplySafeArea();
            }
        }

        /// <summary>
        /// Unity varsayılan ScrollRect arka planındaki gri kutuyu gizlerken Mask bileşeninin çalışmasını korur.
        /// Viewport'un Image rengini şeffaf YAPMAZ (çünkü Mask alpha = 0 olunca tüm çocukları gizler).
        /// Bunun yerine Mask.showMaskGraphic = false yaparak görseli gizler.
        /// </summary>
        public void CleanScrollViewBackgrounds()
        {
            var allScrolls = GetComponentsInChildren<ScrollRect>(true);
            foreach (var sr in allScrolls)
            {
                // ScrollRect üzerindeki görseli kontrol et:
                // Kullanıcının atadığı özel arayüz desenini (UI_SidebarScrollBG vb.) ASLA kapatma!
                // Yalnızca Unity'nin varsayılan boş/UISprite görseli varsa kapat.
                var srImg = sr.GetComponent<Image>();
                if (srImg != null)
                {
                    if (srImg.sprite == null || srImg.sprite.name == "UISprite" || srImg.sprite.name == "Background")
                    {
                        srImg.enabled = false;
                    }
                    else
                    {
                        srImg.enabled = true;
                        srImg.color = Color.white; // Tam opak ve canlı göster
                    }
                }

                // Viewport maskesini ve boyutlandırmasını düzelt: tam esnemeyi garanti et
                if (sr.viewport != null)
                {
                    sr.viewport.anchorMin = Vector2.zero;
                    sr.viewport.anchorMax = Vector2.one;
                    sr.viewport.offsetMin = Vector2.zero;
                    sr.viewport.offsetMax = Vector2.zero;
                    sr.viewport.sizeDelta = Vector2.zero;

                    var mask = sr.viewport.GetComponent<Mask>();
                    if (mask != null)
                    {
                        mask.showMaskGraphic = false; // Görseli çizme ama stencil maskesini aktif tut!
                    }

                    var vpImg = sr.viewport.GetComponent<Image>();
                    if (vpImg != null)
                    {
                        vpImg.color = Color.white; // Alpha = 1 kalmalı, aksi halde Mask içeriği keser!
                    }
                }
            }
        }

        /// <summary>
        /// Mobil ekran çentiğine (Notch) ve Dynamic Island gibi kör noktalara göre çekmecenin
        /// sol konumunu güvenli alana kenetler.
        /// </summary>
        public void ApplySafeArea()
        {
            if (!applySafeArea || _rectTransform == null) return;

            Rect safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0) return;

            // Eğer Sidebar zaten SafeAreaFitter bileşeni olan bir kapsayıcının altındaysa ek pay ekleme
            if (transform.parent != null && transform.parent.GetComponent<SafeAreaFitter>() != null)
            {
                Vector2 pos = _rectTransform.anchoredPosition;
                pos.x = 0f;
                _rectTransform.anchoredPosition = pos;
                return;
            }

            // Doğrudan Canvas altındaysa sol çentik payını Canvas pixel koordinatına göre hesapla
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null) return;

            RectTransform canvasRt = rootCanvas.GetComponent<RectTransform>();
            if (canvasRt == null) return;

            float leftRatio = safeArea.xMin / Screen.width;
            float leftInsetCanvas = leftRatio * canvasRt.rect.width;

            Vector2 newPos = _rectTransform.anchoredPosition;
            newPos.x = leftInsetCanvas;
            _rectTransform.anchoredPosition = newPos;
        }

        private void SetupButtonListeners()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(OpenSidebar);
                openButton.onClick.AddListener(OpenSidebar);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseSidebar);
                closeButton.onClick.AddListener(CloseSidebar);
            }

            if (darkOverlay != null && darkOverlay.TryGetComponent<Button>(out var overlayBtn))
            {
                overlayBtn.onClick.RemoveListener(CloseSidebar);
                overlayBtn.onClick.AddListener(CloseSidebar);
            }

            // Sekme geçiş dinleyicileri (0: Kervan, 1: Yük, 2: Görevler)
            if (tabKervan != null)
            {
                tabKervan.onClick.AddListener(() => ShowTab(0));
            }
            if (tabYuk != null)
            {
                tabYuk.onClick.AddListener(() => ShowTab(1));
            }
            if (tabGorevler != null)
            {
                tabGorevler.onClick.AddListener(() => ShowTab(2));
            }
        }

        public void OpenSidebar()
        {
            AutoBindReferences();
            ApplySafeArea();
            CleanScrollViewBackgrounds();

            if (sidebarPanel != null) sidebarPanel.SetActive(true);
            if (darkOverlay != null) darkOverlay.SetActive(true);

            PlayOpenSound();
            RefreshData();
            ShowTab(0);
        }

        public void CloseSidebar()
        {
            if (sidebarPanel != null && sidebarPanel.activeSelf)
            {
                PlayCloseSound();
            }

            if (sidebarPanel != null) sidebarPanel.SetActive(false);
            if (darkOverlay != null) darkOverlay.SetActive(false);
        }

        private void PlayOpenSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPaperRustle();
            }
            else
            {
                AudioClip clip = Resources.Load<AudioClip>("Audio/SFX/SFX_PaperRustle");
                if (clip != null)
                {
                    AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.85f);
                }
            }
        }

        private void PlayCloseSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }

        public void ShowTab(int index)
        {
            if (contentKervan != null) contentKervan.SetActive(index == 0);
            if (contentYuk != null) contentYuk.SetActive(index == 1);
            if (contentGorevler != null) contentGorevler.SetActive(index == 2);

            UpdateTabButtonVisuals(index);

            // Eğer içerik listeleri boş kalmışsa anında veriyi doldur
            if ((index == 0 && kervanUnitsContainer != null && kervanUnitsContainer.childCount == 0) ||
                (index == 1 && yukContainer != null && yukContainer.childCount == 0) ||
                (index == 2 && questContainer != null && questContainer.childCount == 0))
            {
                RefreshData();
            }

            var scrollRect = GetComponentInChildren<ScrollRect>(true);
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }

            Canvas.ForceUpdateCanvases();
            if (kervanUnitsContainer is RectTransform rtK) LayoutRebuilder.ForceRebuildLayoutImmediate(rtK);
            if (yukContainer is RectTransform rtY) LayoutRebuilder.ForceRebuildLayoutImmediate(rtY);
            if (questContainer is RectTransform rtQ) LayoutRebuilder.ForceRebuildLayoutImmediate(rtQ);
        }

        private void UpdateTabButtonVisuals(int activeIndex)
        {
            SetButtonVisual(tabKervan, activeIndex == 0);
            SetButtonVisual(tabYuk, activeIndex == 1);
            SetButtonVisual(tabGorevler, activeIndex == 2);
        }

        private void SetButtonVisual(Button btn, bool isSelected)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isSelected ? Color.white : new Color(0.75f, 0.75f, 0.75f, 0.85f);
            }
        }

        private void EnsureLayout(Transform container)
        {
            if (container == null) return;

            if (container is RectTransform rt)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = Vector2.zero;
            }

            var vlg = container.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = container.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 8f; // Ayraç motifi için nefes payı
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childControlWidth = true;   // Satır genişliğini konteyner ile tam eşitle
            vlg.childControlHeight = true;  // Satır yüksekliğini LayoutElement ile eşitle
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = container.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = container.gameObject.AddComponent<ContentSizeFitter>();
            }
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public void RefreshData()
        {
            CleanScrollViewBackgrounds();
            EnsureLayout(kervanUnitsContainer);
            EnsureLayout(yukContainer);
            EnsureLayout(questContainer);

            var caravan = CaravanManager.Instance != null ? CaravanManager.Instance.Caravan : null;

            int akce = caravan != null ? caravan.CurrentAkce : 500;
            float currentWeight = caravan != null ? caravan.CalculateCurrentWeight() : 45f;
            float maxWeight = caravan != null ? caravan.MaxWeightCapacity : 300f;

            // 1. Üst Başlık Bilgileri
            if (akceText != null) akceText.text = akce.ToString();
            if (characterNameText != null) characterNameText.text = "Gazi Alp";
            if (characterPortrait != null && _leaderSprite != null)
            {
                characterPortrait.sprite = _leaderSprite;
                characterPortrait.preserveAspect = true;
            }

            // 2. Yük Kapasitesi Metni
            if (_capacityText != null)
            {
                _capacityText.text = $"Yük Kapasitesi: {currentWeight:0.#} / {maxWeight:0.#} Kg";
                _capacityText.color = new Color(0.96f, 0.90f, 0.83f, 1f); // #F5E6D3 Parşömen kremi
            }

            // 3. Kervan Sekmesi (Birimler ve Hayvanlar)
            if (kervanUnitsContainer != null)
            {
                ClearContainer(kervanUnitsContainer);

                if (caravan != null && caravan.Guards != null && caravan.Guards.Count > 0)
                {
                    Dictionary<string, (int count, Sprite portrait)> guardData = new Dictionary<string, (int, Sprite)>();
                    foreach (var g in caravan.Guards)
                    {
                        if (g.UnitData == null) continue;
                        string uName = g.UnitData.UnitName;
                        Sprite p = GetPortraitForUnit(uName);

                        if (!guardData.ContainsKey(uName))
                            guardData[uName] = (0, p);

                        var data = guardData[uName];
                        guardData[uName] = (data.count + 1, data.portrait);
                    }

                    foreach (var kvp in guardData)
                    {
                        CreateRow(kervanUnitsContainer, unitRowPrefab, kvp.Key, kvp.Value.count + " Kişi", kvp.Value.portrait);
                    }
                }
                else
                {
                    // Varsayılan / Test kervan kadrosu
                    CreateRow(kervanUnitsContainer, unitRowPrefab, "Alp (Lider)", "1 Kişi", GetPortraitForUnit("Alp"));
                    CreateRow(kervanUnitsContainer, unitRowPrefab, "Kervan Muhafızı", "3 Kişi", GetPortraitForUnit("Muhafız"));
                    CreateRow(kervanUnitsContainer, unitRowPrefab, "Gazi Okçu", "2 Kişi", GetPortraitForUnit("Okçu"));
                }

                // Binek ve Yük Hayvanları
                int horses = caravan != null ? caravan.Horses : 2;
                int mules = caravan != null ? caravan.Mules : 2;
                int camels = caravan != null ? caravan.Camels : 1;

                if (horses > 0) CreateRow(kervanUnitsContainer, unitRowPrefab, "Binek Atı", horses + " Baş", GetPortraitForUnit("At"));
                if (mules > 0) CreateRow(kervanUnitsContainer, unitRowPrefab, "Yük Katırı", mules + " Baş", GetPortraitForUnit("Katır"));
                if (camels > 0) CreateRow(kervanUnitsContainer, unitRowPrefab, "Kervan Devesi", camels + " Baş", GetPortraitForUnit("Deve"));
            }

            // 4. Yük Sekmesi (Ticaret Emtiaları - Ağırlık parantezi kaldırıldı, sadece net adet)
            if (yukContainer != null)
            {
                ClearContainer(yukContainer);

                if (caravan != null && caravan.Inventory != null && caravan.Inventory.Count > 0)
                {
                    foreach (var slot in caravan.Inventory)
                    {
                        if (slot.Item != null && slot.Quantity > 0)
                        {
                            string detail = $"{slot.Quantity} Adet";
                            Sprite itemIcon = GetIconForItem(slot.Item.ItemName, slot.Item.Icon);
                            CreateRow(yukContainer, itemRowPrefab, slot.Item.ItemName, detail, itemIcon);
                        }
                    }
                }
                else
                {
                    // Varsayılan / Test ticaret yükleri (Net adetler)
                    CreateRow(yukContainer, itemRowPrefab, "Bursa İpeği", "10 Adet", GetIconForItem("Bursa İpeği"));
                    CreateRow(yukContainer, itemRowPrefab, "Dövme Demir", "15 Külçe", GetIconForItem("Dövme Demir"));
                    CreateRow(yukContainer, itemRowPrefab, "Anadolu Buğdayı", "20 Çuval", GetIconForItem("Anadolu Buğdayı"));
                }
            }

            // 5. Görevler Sekmesi (UI_BG_Itemicon rozeti ile geniş, iki satırlı profesyonel kartlar)
            if (questContainer != null)
            {
                ClearContainer(questContainer);
                CreateQuestRow(questContainer, "İlk Sefer: İznik Hanı", "Söğüt pazarından yüklenen ipek balyalarını İznik Hanı'na güvenle ulaştır.");
                CreateQuestRow(questContainer, "Kervan Güvenliği", "Yol boyunca pusulardan sakınmak için muhafızları teyakkuzda tut.");
            }

            Canvas.ForceUpdateCanvases();
            if (kervanUnitsContainer is RectTransform rtK2) LayoutRebuilder.ForceRebuildLayoutImmediate(rtK2);
            if (yukContainer is RectTransform rtY2) LayoutRebuilder.ForceRebuildLayoutImmediate(rtY2);
            if (questContainer is RectTransform rtQ2) LayoutRebuilder.ForceRebuildLayoutImmediate(rtQ2);
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Kervan ve Yük için tek satırlık temiz öğe üretir.
        /// </summary>
        private void CreateRow(Transform container, GameObject prefab, string mainText, string valueText, Sprite icon)
        {
            if (container == null) return;

            if (useCustomPrefabs && prefab != null)
            {
                CreateRowFromPrefab(container, prefab, mainText, valueText, icon);
            }
            else
            {
                CreateProceduralRow(container, mainText, valueText, icon);
            }
        }

        private void CreateRowFromPrefab(Transform container, GameObject prefab, string mainText, string valueText, Sprite icon)
        {
            GameObject rowObj = Instantiate(prefab, container);
            rowObj.SetActive(true);

            RectTransform rowRt = rowObj.GetComponent<RectTransform>();
            if (rowRt != null)
            {
                rowRt.anchorMin = new Vector2(0f, 1f);
                rowRt.anchorMax = new Vector2(1f, 1f);
                rowRt.pivot = new Vector2(0.5f, 1f);
                rowRt.sizeDelta = new Vector2(0f, 70f);
                rowRt.offsetMin = Vector2.zero;
                rowRt.offsetMax = Vector2.zero;
            }

            var le = rowObj.GetComponent<LayoutElement>();
            if (le == null) le = rowObj.AddComponent<LayoutElement>();
            le.minHeight = 70f;
            le.preferredHeight = 70f;
            le.flexibleHeight = 0f;
            le.flexibleWidth = 1f;

            Transform rowContent = rowObj.transform.Find("RowContent");
            if (rowContent != null)
            {
                RectTransform cRt = rowContent.GetComponent<RectTransform>();
                if (cRt != null)
                {
                    cRt.anchorMin = Vector2.zero;
                    cRt.anchorMax = Vector2.one;
                    cRt.offsetMin = new Vector2(8f, 2f);
                    cRt.offsetMax = new Vector2(-8f, -2f);
                }

                HorizontalLayoutGroup hlg = rowContent.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.padding = new RectOffset(6, 6, 2, 2);
                    hlg.spacing = 12f;
                    hlg.childAlignment = TextAnchor.MiddleCenter;
                    hlg.childControlWidth = true;
                    hlg.childControlHeight = true;
                    hlg.childForceExpandWidth = false;
                    hlg.childForceExpandHeight = false;
                }
            }

            Image[] images = rowObj.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                string imgName = img.gameObject.name.ToLowerInvariant();
                if (imgName.Contains("icon") || imgName.Contains("profile") || imgName.Contains("portrait") || imgName.Contains("resim"))
                {
                    if (icon != null)
                    {
                        img.sprite = icon;
                        img.color = Color.white;
                        img.preserveAspect = true;
                    }
                    RectTransform imgRt = img.GetComponent<RectTransform>();
                    if (imgRt != null) imgRt.sizeDelta = new Vector2(56f, 56f);

                    var imgLe = img.GetComponent<LayoutElement>();
                    if (imgLe == null) imgLe = img.gameObject.AddComponent<LayoutElement>();
                    imgLe.minWidth = 56f;
                    imgLe.preferredWidth = 56f;
                    imgLe.flexibleWidth = 0f;
                    break;
                }
            }

            TextMeshProUGUI[] texts = rowObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in texts)
            {
                string txtName = txt.gameObject.name.ToLowerInvariant();
                if (txtName.Contains("name") || txtName.Contains("main") || txtName.Contains("title") || txtName.Contains("baslik"))
                {
                    if (_fontAsset != null) txt.font = _fontAsset;
                    txt.text = mainText;
                    txt.fontSize = 20f;
                    txt.fontSizeMin = 13f;
                    txt.fontSizeMax = 22f;
                    txt.enableAutoSizing = true;
                    txt.fontStyle = FontStyles.Bold;
                    txt.color = new Color(0.96f, 0.90f, 0.83f, 1f); // #F5E6D3 Parşömen kremi
                    txt.alignment = TextAlignmentOptions.MidlineLeft;
                    txt.overflowMode = TextOverflowModes.Truncate;

                    var txtLe = txt.GetComponent<LayoutElement>();
                    if (txtLe == null) txtLe = txt.gameObject.AddComponent<LayoutElement>();
                    txtLe.minWidth = 120f;
                    txtLe.preferredWidth = 190f;
                    txtLe.flexibleWidth = 1f;
                }
                else if (txtName.Contains("count") || txtName.Contains("value") || txtName.Contains("detail") || txtName.Contains("miktar"))
                {
                    if (_fontAsset != null) txt.font = _fontAsset;
                    txt.text = valueText;
                    txt.fontSize = 17f;
                    txt.fontSizeMin = 11f;
                    txt.fontSizeMax = 18f;
                    txt.enableAutoSizing = true;
                    txt.fontStyle = FontStyles.Normal;
                    txt.color = new Color(0.85f, 0.70f, 0.35f, 1f); // #D9B359 Altın kum
                    txt.alignment = TextAlignmentOptions.MidlineRight;

                    var txtLe = txt.GetComponent<LayoutElement>();
                    if (txtLe == null) txtLe = txt.gameObject.AddComponent<LayoutElement>();
                    txtLe.minWidth = 80f;
                    txtLe.preferredWidth = 110f;
                    txtLe.flexibleWidth = 0f;
                }
            }

            Transform sep = rowObj.transform.Find("Seperator") ?? rowObj.transform.Find("Ayirac");
            if (sep != null && sep.TryGetComponent<RectTransform>(out var sepRt))
            {
                sepRt.anchorMin = new Vector2(0.04f, 0f);
                sepRt.anchorMax = new Vector2(0.96f, 0f);
                sepRt.pivot = new Vector2(0.5f, 0f);
                sepRt.sizeDelta = new Vector2(0f, 2f);
                sepRt.anchoredPosition = Vector2.zero;
            }
        }

        private GameObject CreateProceduralRow(Transform container, string mainText, string valueText, Sprite icon)
        {
            GameObject rowObj = new GameObject("Row_" + mainText, typeof(RectTransform), typeof(LayoutElement));
            rowObj.transform.SetParent(container, false);

            RectTransform rowRt = rowObj.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.sizeDelta = new Vector2(0f, 70f);

            LayoutElement le = rowObj.GetComponent<LayoutElement>();
            le.minHeight = 70f;
            le.preferredHeight = 70f;
            le.flexibleHeight = 0f;
            le.flexibleWidth = 1f;

            // Yatay içerik taşıyıcısı
            GameObject contentObj = new GameObject("RowContent", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            contentObj.transform.SetParent(rowObj.transform, false);

            RectTransform cRt = contentObj.GetComponent<RectTransform>();
            cRt.anchorMin = Vector2.zero;
            cRt.anchorMax = Vector2.one;
            cRt.offsetMin = new Vector2(8f, 2f);
            cRt.offsetMax = new Vector2(-8f, -2f);

            HorizontalLayoutGroup hlg = contentObj.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(6, 6, 2, 2);
            hlg.spacing = 14f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 1. Portre / İkon (56x56 px)
            if (icon != null)
            {
                GameObject iconObj = new GameObject("Portrait", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                iconObj.transform.SetParent(contentObj.transform, false);

                Image img = iconObj.GetComponent<Image>();
                img.sprite = icon;
                img.preserveAspect = true;

                LayoutElement iconLe = iconObj.GetComponent<LayoutElement>();
                iconLe.minWidth = 56f;
                iconLe.preferredWidth = 56f;
                iconLe.flexibleWidth = 0f;
                iconLe.minHeight = 56f;
                iconLe.preferredHeight = 56f;
                iconLe.flexibleHeight = 0f;
            }

            // 2. Birim / Mal Adı (Orta Esnek Alan)
            GameObject nameObj = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            nameObj.transform.SetParent(contentObj.transform, false);

            TextMeshProUGUI nameTmp = nameObj.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) nameTmp.font = _fontAsset;
            nameTmp.text = mainText;
            nameTmp.fontSize = 20f;
            nameTmp.fontSizeMin = 13f;
            nameTmp.fontSizeMax = 22f;
            nameTmp.enableAutoSizing = true;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = new Color(0.96f, 0.90f, 0.83f, 1f); // #F5E6D3 Parşömen kremi
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.overflowMode = TextOverflowModes.Truncate;

            LayoutElement nameLe = nameObj.GetComponent<LayoutElement>();
            nameLe.minWidth = 120f;
            nameLe.preferredWidth = 190f;
            nameLe.flexibleWidth = 1f;

            // 3. Miktar / Adet Metni (Sağa Hizalı)
            GameObject countObj = new GameObject("CountText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            countObj.transform.SetParent(contentObj.transform, false);

            TextMeshProUGUI countTmp = countObj.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) countTmp.font = _fontAsset;
            countTmp.text = valueText;
            countTmp.fontSize = 17f;
            countTmp.fontSizeMin = 11f;
            countTmp.fontSizeMax = 18f;
            countTmp.enableAutoSizing = true;
            countTmp.fontStyle = FontStyles.Normal;
            countTmp.color = new Color(0.85f, 0.70f, 0.35f, 1f); // #D9B359 Altın kum
            countTmp.alignment = TextAlignmentOptions.MidlineRight;

            LayoutElement countLe = countObj.GetComponent<LayoutElement>();
            countLe.minWidth = 80f;
            countLe.preferredWidth = 110f;
            countLe.flexibleWidth = 0f;

            // 4. Alt Zarif Ayraç Motifi (UI_BG_ayirac_2)
            CreateOrnamentSeparator(rowObj);

            return rowObj;
        }

        /// <summary>
        /// Görevler sekmesi için geniş, iki satırlı ve UI_BG_Itemicon rozetli profesyonel görev kartı üretir.
        /// </summary>
        private GameObject CreateQuestRow(Transform container, string questTitle, string questObjective)
        {
            GameObject rowObj = new GameObject("QuestRow_" + questTitle, typeof(RectTransform), typeof(LayoutElement));
            rowObj.transform.SetParent(container, false);

            RectTransform rowRt = rowObj.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.sizeDelta = new Vector2(0f, 86f);

            LayoutElement le = rowObj.GetComponent<LayoutElement>();
            le.minHeight = 86f;
            le.preferredHeight = 86f;
            le.flexibleHeight = 0f;
            le.flexibleWidth = 1f;

            // Yatay taşıyıcı
            GameObject contentObj = new GameObject("RowContent", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            contentObj.transform.SetParent(rowObj.transform, false);

            RectTransform cRt = contentObj.GetComponent<RectTransform>();
            cRt.anchorMin = Vector2.zero;
            cRt.anchorMax = Vector2.one;
            cRt.offsetMin = new Vector2(8f, 4f);
            cRt.offsetMax = new Vector2(-8f, -4f);

            HorizontalLayoutGroup hlg = contentObj.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(4, 4, 2, 2);
            hlg.spacing = 14f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 1. Sol: Görev Rozeti (UI_BG_Itemicon)
            if (_questBadgeSprite != null)
            {
                GameObject badgeObj = new GameObject("QuestBadge", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                badgeObj.transform.SetParent(contentObj.transform, false);

                Image img = badgeObj.GetComponent<Image>();
                img.sprite = _questBadgeSprite;
                img.preserveAspect = true;

                LayoutElement badgeLe = badgeObj.GetComponent<LayoutElement>();
                badgeLe.minWidth = 52f;
                badgeLe.preferredWidth = 52f;
                badgeLe.flexibleWidth = 0f;
                badgeLe.minHeight = 52f;
                badgeLe.preferredHeight = 52f;
                badgeLe.flexibleHeight = 0f;
            }

            // 2. Sağ: Metin Bloğu (Başlık + Açıklama)
            GameObject textBlockObj = new GameObject("TextBlock", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            textBlockObj.transform.SetParent(contentObj.transform, false);

            LayoutElement blockLe = textBlockObj.GetComponent<LayoutElement>();
            blockLe.minWidth = 150f;
            blockLe.preferredWidth = 350f;
            blockLe.flexibleWidth = 1f;

            VerticalLayoutGroup vlg = textBlockObj.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 2, 2);
            vlg.spacing = 3f;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // 2.a Görev Başlığı
            GameObject titleObj = new GameObject("QuestTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(textBlockObj.transform, false);

            TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) titleTmp.font = _fontAsset;
            titleTmp.text = questTitle;
            titleTmp.fontSize = 19f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = new Color(0.96f, 0.90f, 0.83f, 1f); // #F5E6D3 Parşömen kremi
            titleTmp.alignment = TextAlignmentOptions.TopLeft;

            // 2.b Görev Hedefi / Detayı
            GameObject descObj = new GameObject("QuestDesc", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(textBlockObj.transform, false);

            TextMeshProUGUI descTmp = descObj.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) descTmp.font = _fontAsset;
            descTmp.text = questObjective;
            descTmp.fontSize = 14f;
            descTmp.fontStyle = FontStyles.Normal;
            descTmp.color = new Color(0.84f, 0.77f, 0.63f, 1f); // #D6C4A1 Sıcak açık parşömen
            descTmp.alignment = TextAlignmentOptions.TopLeft;
            descTmp.textWrappingMode = TextWrappingModes.Normal;
            descTmp.lineSpacing = -8f;

            // 3. Alt Zarif Ayraç Motifi (UI_BG_ayirac_2)
            CreateOrnamentSeparator(rowObj);

            return rowObj;
        }

        /// <summary>
        /// Satır altına ortalanmış zarif UI_BG_ayirac_2 rozet motifi ve kılcal altın ayraç kanatları üretir.
        /// </summary>
        private void CreateOrnamentSeparator(GameObject parentRow)
        {
            if (_ayiracSprite == null) return;

            GameObject sepContainer = new GameObject("SeperatorContainer", typeof(RectTransform));
            sepContainer.transform.SetParent(parentRow.transform, false);

            RectTransform cRt = sepContainer.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 0f);
            cRt.anchorMax = new Vector2(1f, 0f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.sizeDelta = new Vector2(0f, 18f);
            cRt.anchoredPosition = new Vector2(0f, -2f);

            // 1. Sol İnce Kılcal Çizgi
            GameObject leftLine = new GameObject("LeftLine", typeof(RectTransform), typeof(Image));
            leftLine.transform.SetParent(sepContainer.transform, false);
            RectTransform lRt = leftLine.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0.06f, 0.5f);
            lRt.anchorMax = new Vector2(0.44f, 0.5f);
            lRt.pivot = new Vector2(1f, 0.5f);
            lRt.sizeDelta = new Vector2(0f, 1.5f);
            lRt.anchoredPosition = Vector2.zero;
            Image lImg = leftLine.GetComponent<Image>();
            lImg.color = new Color(0.85f, 0.70f, 0.40f, 0.35f);
            lImg.raycastTarget = false;

            // 2. Orta Rozet Motifi (UI_BG_ayirac_2)
            GameObject motifObj = new GameObject("Motif", typeof(RectTransform), typeof(Image));
            motifObj.transform.SetParent(sepContainer.transform, false);
            RectTransform mRt = motifObj.GetComponent<RectTransform>();
            mRt.anchorMin = new Vector2(0.5f, 0.5f);
            mRt.anchorMax = new Vector2(0.5f, 0.5f);
            mRt.pivot = new Vector2(0.5f, 0.5f);
            mRt.sizeDelta = new Vector2(22f, 22f);
            mRt.anchoredPosition = Vector2.zero;
            Image mImg = motifObj.GetComponent<Image>();
            mImg.sprite = _ayiracSprite;
            mImg.preserveAspect = true; // 85x85 kare motifi orijinal oranında koru!
            mImg.color = new Color(0.95f, 0.85f, 0.60f, 0.85f); // Sıcak altın parşömen
            mImg.raycastTarget = false;

            // 3. Sağ İnce Kılcal Çizgi
            GameObject rightLine = new GameObject("RightLine", typeof(RectTransform), typeof(Image));
            rightLine.transform.SetParent(sepContainer.transform, false);
            RectTransform rRt = rightLine.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.56f, 0.5f);
            rRt.anchorMax = new Vector2(0.94f, 0.5f);
            rRt.pivot = new Vector2(0f, 0.5f);
            rRt.sizeDelta = new Vector2(0f, 1.5f);
            rRt.anchoredPosition = Vector2.zero;
            Image rImg = rightLine.GetComponent<Image>();
            rImg.color = new Color(0.85f, 0.70f, 0.40f, 0.35f);
            rImg.raycastTarget = false;
        }

        public Sprite GetPortraitForUnit(string unitName, Sprite fallback = null)
        {
            string n = (unitName ?? "").ToLowerInvariant();
            if (n.Contains("okçu") || n.Contains("okcu") || n.Contains("archer"))
                return _archerSprite != null ? _archerSprite : fallback;
            if (n.Contains("muhafız") || n.Contains("muhafiz") || n.Contains("guard"))
                return _guardSprite != null ? _guardSprite : fallback;
            if (n.Contains("alp") || n.Contains("lider") || n.Contains("ahi") || n.Contains("kıdemli"))
                return _leaderSprite != null ? _leaderSprite : fallback;
            if (n.Contains("at") || n.Contains("horse") || n.Contains("binek"))
                return _horseSprite != null ? _horseSprite : fallback;
            if (n.Contains("katır") || n.Contains("katir") || n.Contains("mule"))
                return _muleSprite != null ? _muleSprite : fallback;
            if (n.Contains("deve") || n.Contains("camel"))
                return _camelSprite != null ? _camelSprite : fallback;

            return _guardSprite != null ? _guardSprite : fallback;
        }

        public Sprite GetIconForItem(string itemName, Sprite defaultIcon = null)
        {
            string n = (itemName ?? "").ToLowerInvariant();
            if (n.Contains("ipek")) return _ipekSprite != null ? _ipekSprite : defaultIcon;
            if (n.Contains("demir")) return _demirSprite != null ? _demirSprite : defaultIcon;
            if (n.Contains("buğday") || n.Contains("bugday")) return _bugdaySprite != null ? _bugdaySprite : defaultIcon;
            if (n.Contains("baharat")) return _baharatSprite != null ? _baharatSprite : defaultIcon;
            if (n.Contains("çelik") || n.Contains("celik")) return _celikSprite != null ? _celikSprite : defaultIcon;
            if (n.Contains("kürk") || n.Contains("kurk")) return _kurkSprite != null ? _kurkSprite : defaultIcon;

            return defaultIcon;
        }

        public void AutoBindReferences()
        {
            if (sidebarPanel == null)
            {
                var p = transform.Find("SidebarPanel");
                sidebarPanel = p != null ? p.gameObject : gameObject;
            }

            // Tab butonlarını bul
            Button[] allButtons = GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                string n = btn.gameObject.name.ToLowerInvariant();
                if (tabKervan == null && (n.Contains("kervan") || n == "btnkervan"))
                    tabKervan = btn;
                else if (tabYuk == null && (n.Contains("yuk") || n == "btnyuk"))
                    tabYuk = btn;
                else if (tabGorevler == null && (n.Contains("gorev") || n == "btngorev"))
                    tabGorevler = btn;
                else if (closeButton == null && (n.Contains("close") || n.Contains("kapat") || n == "btnclose"))
                    closeButton = btn;
            }

            // Sekme içerik konteynerlerini bul
            Transform[] allTransforms = GetComponentsInChildren<Transform>(true);
            foreach (var t in allTransforms)
            {
                string n = t.gameObject.name.ToLowerInvariant();
                if (contentKervan == null && (n == "contentkervan" || n == "kervancontent"))
                    contentKervan = t.gameObject;
                else if (contentYuk == null && (n == "contentyuk" || n == "yukcontent"))
                    contentYuk = t.gameObject;
                else if (contentGorevler == null && (n == "contentgorev" || n == "contentgorevler" || n == "gorevcontent"))
                    contentGorevler = t.gameObject;
            }

            // Scroll alt liste konteynerleri
            if (kervanUnitsContainer == null && contentKervan != null)
                kervanUnitsContainer = contentKervan.transform;
            if (yukContainer == null && contentYuk != null)
                yukContainer = contentYuk.transform;
            if (questContainer == null && contentGorevler != null)
                questContainer = contentGorevler.transform;

            // Metin bileşenlerini bul
            TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in allTexts)
            {
                string n = txt.gameObject.name.ToLowerInvariant();
                if (characterNameText == null && (n.Contains("character") || n.Contains("leader") || n.Contains("gazi")))
                    characterNameText = txt;
                else if (akceText == null && (n.Contains("akce") || n.Contains("gold") || txt.transform.parent?.name.ToLowerInvariant().Contains("akce") == true))
                    akceText = txt;
                else if (_capacityText == null && (n.Contains("capacity") || n.Contains("kapasite") || n.Contains("yuk")))
                    _capacityText = txt;
            }

            // Karakter portresi
            if (characterPortrait == null)
            {
                Image[] allImgs = GetComponentsInChildren<Image>(true);
                foreach (var img in allImgs)
                {
                    string n = img.gameObject.name.ToLowerInvariant();
                    if (n.Contains("leaderportrait") || n.Contains("characterportrait") || n.Contains("liderresim"))
                    {
                        characterPortrait = img;
                        break;
                    }
                }
            }

            // Ebeveynde dışarıdaki envanter butonu ve koyu overlay'i ara
            if (transform.parent != null)
            {
                if (openButton == null)
                {
                    var btnInv = transform.parent.Find("Btn_Inventory") ?? transform.parent.Find("BtnInventory");
                    if (btnInv != null) openButton = btnInv.GetComponent<Button>();
                }
                if (darkOverlay == null)
                {
                    var overlay = transform.parent.Find("DarkOverlay") ?? transform.parent.Find("Overlay");
                    if (overlay != null) darkOverlay = overlay.gameObject;
                }
            }

            // Ayraç ve rozet görseli (UI_BG_ayirac_2)
            if (_ayiracSprite == null || _ayiracSprite.name != "UI_BG_ayirac_2")
            {
                var sepTop = transform.Find("SidebarPanel/SidebarBackground/SeperatorTop") ?? 
                             transform.Find("SidebarBackground/SeperatorTop") ??
                             transform.Find("SeperatorTop");
                if (sepTop != null && sepTop.TryGetComponent<Image>(out var sImg) && sImg.sprite != null)
                {
                    _ayiracSprite = sImg.sprite;
                }
            }

#if UNITY_EDITOR
            AutoAssignAssetsEditor();
#endif
        }

#if UNITY_EDITOR
        private void Reset()
        {
            AutoBindReferences();
            AutoAssignAssetsEditor();
            ApplySafeArea();
            CleanScrollViewBackgrounds();
        }

        private void AutoAssignAssetsEditor()
        {
            // Yazı Tipi (TMP Font)
            if (_fontAsset == null) _fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/MedievalSharp-Regular SDF.asset");

            // Portreler
            if (_leaderSprite == null) _leaderSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/UI/Portraits/Ahi_ProfileCard.png");
            if (_guardSprite == null) _guardSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/UI/Portraits/Mauhafiz_ProfileCard.png");
            if (_archerSprite == null) _archerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/UI/Portraits/Okcu_ProfileCard.png");
            if (_horseSprite == null) _horseSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/UI/Portraits/Horse_ProfileCard.png");
            if (_muleSprite == null) _muleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/UI/Portraits/Mule_ProfileCard.png");
            if (_camelSprite == null) _camelSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/UI/Portraits/Camel_ProfileCard.png");

            // Ticaret Malları
            if (_ipekSprite == null) _ipekSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Items/Item_Ipek.png");
            if (_demirSprite == null) _demirSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Items/Item_Demir.png");
            if (_bugdaySprite == null) _bugdaySprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Items/Item_Bugday.png");
            if (_baharatSprite == null) _baharatSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Items/Item_Baharat.png");
            if (_celikSprite == null) _celikSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Items/Item_Celik.png");
            if (_kurkSprite == null) _kurkSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Items/Item_Kurk.png");

            // Ayraç ve Rozet (UI_BG_ayirac_2)
            if (_ayiracSprite == null || _ayiracSprite.name != "UI_BG_ayirac_2")
            {
                var allDecorations = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Sprites/UI/Decorations/UI_BG_ayirac.png");
                foreach (var a in allDecorations)
                {
                    if (a is Sprite s && s.name == "UI_BG_ayirac_2")
                    {
                        _ayiracSprite = s;
                        break;
                    }
                }
            }

            if (_questBadgeSprite == null) _questBadgeSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/UI/Backgrounds/UI_BG_Itemicon.png");
        }
#endif
    }
}
