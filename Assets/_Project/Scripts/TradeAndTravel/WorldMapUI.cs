using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Kervan.UI
{
    public class WorldMapUI : MonoBehaviour
    {
        [SerializeField] private Button _backButton;

        private void Start()
        {
            if (_backButton != null) _backButton.onClick.AddListener(OnBackClicked);

            // Dinamik olarak tum sehir/han butonlarini bul ve dinle
            Button[] allButtons = GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name.StartsWith("Node_"))
                {
                    string nodeName = btn.gameObject.name.Substring(5); // "Node_" kismini at
                    nodeName = nodeName.Replace("_", " "); // Varsa alt cizgileri bosluk yap
                    btn.onClick.AddListener(() => OnNodeClicked(nodeName));
                }
            }
        }

        private void OnNodeClicked(string nodeName)
        {
            Debug.Log($"[WorldMap] Haritada secilen nokta: {nodeName}");
            // Prototip: Secilen sehre girmek icin TownHub'a geciyoruz
            PlayerPrefs.SetString("CurrentCity", nodeName);
            SceneManager.LoadScene("TownHubScene");
        }

        private void OnBackClicked()
        {
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
