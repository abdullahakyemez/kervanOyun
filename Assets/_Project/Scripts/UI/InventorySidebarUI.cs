using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.TradeAndTravel;
using System.Collections.Generic;

namespace Kervan.UI
{
    public class InventorySidebarUI : MonoBehaviour
    {
        [Header("Main Panels")]
        public GameObject sidebarPanel;
        public GameObject darkOverlay;
        public Button openButton;
        public Button closeButton;

        [Header("Character Info (Header)")]
        public Image characterPortrait;
        public TextMeshProUGUI characterNameText;
        public TextMeshProUGUI akceText;

        [Header("Tab Buttons")]
        public Button tabKervan;
        public Button tabGorevler;
        public Button tabYuk;

        [Header("Containers (Scroll View Content)")]
        public GameObject contentKervan;
        public GameObject contentGorevler;
        public GameObject contentYuk;
        
        [Header("Kervan Section")]
        public Transform kervanUnitsContainer;
        public TextMeshProUGUI erzakCountText; // Eski yontem (Kullanilmayabilir)
        public Sprite erzakSprite; // Erzak satiri iconu icin

        [Header("Yuk (Trade Goods) Section")]
        public Transform yukContainer;

        [Header("Gorevler (Quests) Section")]
        public Transform questContainer;

        [Header("Prefabs for List Rows")]
        public GameObject unitRowPrefab; // ProfileCard + Name + Count + Ayirac
        public GameObject itemRowPrefab; // ItemIcon + Name + Count + Ayirac
        public GameObject questRowPrefab; // UI_BG_Itemicon + Quest Text + Ayirac

        private void Start()
        {
            if(openButton != null) openButton.onClick.AddListener(OpenSidebar);
            if(closeButton != null) closeButton.onClick.AddListener(CloseSidebar);
            
            if(darkOverlay != null && darkOverlay.TryGetComponent<Button>(out var overlayBtn))
                overlayBtn.onClick.AddListener(CloseSidebar);

            // Tab sistemiyle tasarim arasinda gecis (Isterseniz hepsini tek sayfada da dizebilirsiniz)
            if(tabKervan != null) tabKervan.onClick.AddListener(() => ShowTab(0));
            if(tabGorevler != null) tabGorevler.onClick.AddListener(() => ShowTab(1));
            if(tabYuk != null) tabYuk.onClick.AddListener(() => ShowTab(2));

            CloseSidebar();
        }

        public void OpenSidebar()
        {
            if (sidebarPanel) sidebarPanel.SetActive(true);
            if (darkOverlay) darkOverlay.SetActive(true);
            RefreshData();
            ShowTab(0); // Varsayilan Kervan sekmesi acik olsun
        }

        public void CloseSidebar()
        {
            if (sidebarPanel) sidebarPanel.SetActive(false);
            if (darkOverlay) darkOverlay.SetActive(false);
        }

        private void ShowTab(int index)
        {
            if(contentKervan) contentKervan.SetActive(index == 0);
            if(contentGorevler) contentGorevler.SetActive(index == 1);
            if(contentYuk) contentYuk.SetActive(index == 2);
        }

        public void RefreshData()
        {
            var caravan = CaravanManager.Instance != null ? CaravanManager.Instance.Caravan : null;
            if (caravan == null) 
            {
                Debug.LogWarning("[Kervan UI] Sahnede CaravanManager bulunamadi veya ici bos! Lutfen test icin bir CaravanManager ekleyin.");
                return;
            }

            // Ust kisim guncellemesi
            if(akceText) akceText.text = caravan.CurrentAkce.ToString();
            if(characterNameText) characterNameText.text = "Ahi Evran"; // Veya data'dan cekilir

            // Kervan (Birimler) Guncellemesi
            if(kervanUnitsContainer && unitRowPrefab)
            {
                ClearContainer(kervanUnitsContainer);
                Dictionary<string, (int count, Sprite portrait)> guardData = new Dictionary<string, (int, Sprite)>();
                foreach (var g in caravan.Guards)
                {
                    if (g.UnitData == null) continue;
                    if (!guardData.ContainsKey(g.UnitData.UnitName))
                        guardData[g.UnitData.UnitName] = (0, g.UnitData.Portrait);
                    
                    var data = guardData[g.UnitData.UnitName];
                    guardData[g.UnitData.UnitName] = (data.count + 1, data.portrait);
                }

                foreach (var kvp in guardData)
                {
                    CreateRowFromPrefab(kervanUnitsContainer, unitRowPrefab, kvp.Key, kvp.Value.count.ToString(), kvp.Value.portrait);
                }

                // Erzak Satirini Listenin Sonuna Ekleyelim
                CreateRowFromPrefab(kervanUnitsContainer, unitRowPrefab, "Erzak", caravan.CurrentFood.ToString() + " Günlük", erzakSprite);
            }
            else
            {
                if (kervanUnitsContainer == null) Debug.LogWarning("[Kervan UI] kervanUnitsContainer (ContentKervan) bagli degil!");
                if (unitRowPrefab == null) Debug.LogWarning("[Kervan UI] unitRowPrefab bagli degil!");
            }

            // Yuk (Envanter / Trade Goods) Guncellemesi
            if(yukContainer && itemRowPrefab)
            {
                ClearContainer(yukContainer);
                foreach (var slot in caravan.Inventory)
                {
                    if (slot.Item != null && slot.Quantity > 0)
                    {
                        CreateRowFromPrefab(yukContainer, itemRowPrefab, slot.Item.ItemName, slot.Quantity.ToString() + " Adet", slot.Item.Icon);
                    }
                }
            }
            else
            {
                if (yukContainer == null) Debug.LogWarning("[Kervan UI] yukContainer (ContentYuk) bagli degil!");
                if (itemRowPrefab == null) Debug.LogWarning("[Kervan UI] itemRowPrefab bagli degil!");
            }

            // Gorevler (Quests) Guncellemesi (Placeholder)
            if(questContainer && questRowPrefab)
            {
                ClearContainer(questContainer);
                // CreateRowFromPrefab(questContainer, questRowPrefab, "Istanbul'a 10 Ipek gotur", "", null);
            }
        }

        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateRowFromPrefab(Transform container, GameObject prefab, string mainText, string valueText, Sprite icon)
        {
            GameObject rowObj = Instantiate(prefab, container);
            
            // Prefab icerisindeki Image bilesenini bul (Ikon icin)
            Image[] images = rowObj.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                // Ayirac (ayirac) veya background degilse ve adi "Icon" veya "Profile" ise
                if (icon != null && (img.gameObject.name == "Icon" || img.gameObject.name == "Profile" || img.gameObject.name == "Portrait"))
                {
                    img.sprite = icon;
                }
            }

            // Prefab icerisindeki TextMeshProUGUI bilesenlerini bul
            TextMeshProUGUI[] texts = rowObj.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var txt in texts)
            {
                if (txt.gameObject.name == "NameText" || txt.gameObject.name == "MainText")
                    txt.text = mainText;
                else if (txt.gameObject.name == "CountText" || txt.gameObject.name == "ValueText")
                    txt.text = valueText;
            }
        }
    }
}
