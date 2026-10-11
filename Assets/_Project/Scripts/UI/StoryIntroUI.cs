using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Kervan.UI
{
    [System.Serializable]
    public class StoryDialogue
    {
        public string characterName;
        [TextArea(3, 5)]
        public string dialogueText;
        public Sprite characterSprite; // Opsiyonel karakter portresi
        public bool isRightSide; // Portre sağ tarafta mı görünsün?
    }

    public class StoryIntroUI : MonoBehaviour
    {
        [Header("UI Referansları")]
        [SerializeField] private GameObject _introPanel;
        [SerializeField] private Image _backgroundImage; // Arka plani karartmak/blur icin
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Image _characterPortrait;
        [SerializeField] private Button _nextButton; // Ekrana tiklama butonu veya ozel bir ileri butonu

        [Header("Hikaye Verisi")]
        [SerializeField] private List<StoryDialogue> _dialogues = new List<StoryDialogue>();

        private int _currentDialogueIndex = 0;
        private const string HAS_SEEN_INTRO_KEY = "HasSeenIntro_v1"; // Test icin versiyonladik

        private void Start()
        {
            // Eger oyuncu daha once hikayeyi gormusse paneli kapali tut
            if (PlayerPrefs.GetInt(HAS_SEEN_INTRO_KEY, 0) == 1)
            {
                _introPanel.SetActive(false);
                return;
            }

            if (_dialogues != null && _dialogues.Count > 0)
            {
                _introPanel.SetActive(true);
                _currentDialogueIndex = 0;
                ShowDialogue(_currentDialogueIndex);
                
                if (_nextButton != null)
                {
                    _nextButton.onClick.RemoveAllListeners();
                    _nextButton.onClick.AddListener(OnNextClicked);
                }
            }
            else
            {
                // Diyalog yoksa paneli kapat
                _introPanel.SetActive(false);
            }
        }

        private void ShowDialogue(int index)
        {
            var dialogue = _dialogues[index];
            
            if (_characterNameText != null) 
                _characterNameText.text = dialogue.characterName;
                
            if (_dialogueText != null) 
                _dialogueText.text = dialogue.dialogueText;
            
            if (_characterPortrait != null)
            {
                if (dialogue.characterSprite != null)
                {
                    _characterPortrait.sprite = dialogue.characterSprite;
                    _characterPortrait.gameObject.SetActive(true);
                    
                    RectTransform rt = _characterPortrait.GetComponent<RectTransform>();
                    if (dialogue.isRightSide)
                    {
                        rt.anchorMin = new Vector2(0.70f, rt.anchorMin.y);
                        rt.anchorMax = new Vector2(0.95f, rt.anchorMax.y);
                        // Karakter sağda ise sola baksın diye x ekseninde çevir
                        rt.localScale = new Vector3(-1, 1, 1);
                    }
                    else
                    {
                        rt.anchorMin = new Vector2(0.05f, rt.anchorMin.y);
                        rt.anchorMax = new Vector2(0.30f, rt.anchorMax.y);
                        // Karakter solda ise sağa baksın
                        rt.localScale = new Vector3(1, 1, 1);
                    }
                }
                else
                {
                    _characterPortrait.gameObject.SetActive(false);
                }
            }
        }

        private void OnNextClicked()
        {
            _currentDialogueIndex++;
            if (_currentDialogueIndex < _dialogues.Count)
            {
                ShowDialogue(_currentDialogueIndex);
            }
            else
            {
                EndIntro();
            }
        }

        private void EndIntro()
        {
            PlayerPrefs.SetInt(HAS_SEEN_INTRO_KEY, 1);
            PlayerPrefs.Save();
            
            // Paneli animasyonla veya direkt kapatabiliriz
            _introPanel.SetActive(false);
            
            Debug.Log("[StoryIntro] Hikaye girisi tamamlandi. Oyuncu harita ile etkilesime girebilir.");
        }
        
        // Gelistirici/Test amacli: Hikayeyi tekrar izlemek icin tetiklenebilir
        [ContextMenu("Reset Intro (Test)")]
        public void ResetIntro()
        {
            PlayerPrefs.SetInt(HAS_SEEN_INTRO_KEY, 0);
            Debug.Log("[StoryIntro] Hikaye gosterimi sifirlandi. Bir sonraki harita girisinde tekrar gosterilecek.");
        }
    }
}
