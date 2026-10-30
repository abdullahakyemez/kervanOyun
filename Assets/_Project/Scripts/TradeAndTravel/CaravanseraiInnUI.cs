using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Kervan.Core;
using Kervan.Core.Save;
using Kervan.Audio;
using Kervan.Combat;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Kervansaray (Han) sekmesindeki hizmetleri (Konaklama/İyileşme, Erzak İkmali,
    /// Muhafız Kiralama, Yük Hayvanı Satın Alma ve Casus Tutma) yöneten kullanıcı arayüzü kontrolcüsü.
    /// </summary>
    public class CaravanseraiInnUI : MonoBehaviour
    {
        [Header("1. Kart: Konaklama & Erzak (Card_RestAndSupply)")]
        [SerializeField] private Button _btnRest1Day;
        [SerializeField] private Button _btnBuySupplies;
        [SerializeField] private TextMeshProUGUI _txtRestDesc;
        [SerializeField] private TextMeshProUGUI _txtRestButtonLabel;
        [SerializeField] private TextMeshProUGUI _txtSuppliesButtonLabel;

        [Header("2. Kart: Muhafız Ocağı (Card_HireGuards)")]
        [SerializeField] private Button _btnHireMuhafiz;
        [SerializeField] private Button _btnHireArcher;
        [SerializeField] private TextMeshProUGUI _txtGuardsDesc;
        [SerializeField] private TextMeshProUGUI _txtHireMuhafizLabel;
        [SerializeField] private TextMeshProUGUI _txtHireArcherLabel;
        [SerializeField] private UnitDataSO _unitMuhafiz;
        [SerializeField] private UnitDataSO _unitArcher;

        [Header("3. Kart: Ahır & Binekler (Card_HireMounts)")]
        [SerializeField] private Button _btnHireMule;
        [SerializeField] private Button _btnHireCamel;
        [SerializeField] private TextMeshProUGUI _txtMountsDesc;
        [SerializeField] private TextMeshProUGUI _txtHireMuleLabel;
        [SerializeField] private TextMeshProUGUI _txtHireCamelLabel;

        [Header("Alt Bar: Kervan Durumu & Dedikodular (BottomBar_InnMeta)")]
        [SerializeField] private TextMeshProUGUI _spyInfoText;
        [SerializeField] private TextMeshProUGUI _statusFeedbackText;
        [SerializeField] private Button _btnHireSpy;
        [SerializeField] private TextMeshProUGUI _txtHireSpyLabel;

        [Header("Ekonomi & Denge Değerleri")]
        [Tooltip("1 gün konaklama bedeli (Akçe)")]
        [SerializeField] private int _restCostAkce = 15;
        [Tooltip("1 gün konaklamada kazanılan sağlık (HP)")]
        [SerializeField] private int _restHealAmount = 35;
        [Tooltip("1 gün konaklamada kazanılan moral")]
        [SerializeField] private int _restMoraleBonus = 25;
        [Tooltip("Satın alınan erzak miktarı (Günlük)")]
        [SerializeField] private int _suppliesFoodAmount = 10;
        [Tooltip("10 günlük erzak bedeli (Akçe)")]
        [SerializeField] private int _suppliesCostAkce = 25;
        [Tooltip("Kılıçlı Muhafız kiralama bedeli (Akçe)")]
        [SerializeField] private int _hireMuhafizCostAkce = 80;
        [Tooltip("Okçu kiralama bedeli (Akçe)")]
        [SerializeField] private int _hireArcherCostAkce = 100;
        [Tooltip("Katır satın alma bedeli (Akçe)")]
        [SerializeField] private int _hireMuleCostAkce = 75;
        [Tooltip("Katırın kervana kazandırdığı ek yük kapasitesi (Kg)")]
        [SerializeField] private float _muleCapacityBonusKg = 50f;
        [Tooltip("Deve satın alma bedeli (Akçe)")]
        [SerializeField] private int _hireCamelCostAkce = 150;
        [Tooltip("Devenin kervana kazandırdığı ek yük kapasitesi (Kg)")]
        [SerializeField] private float _camelCapacityBonusKg = 100f;
        [Tooltip("Casus tutma bedeli (Akçe)")]
        [SerializeField] private int _hireSpyCostAkce = 50;

        private GameSession _gameSession;
        private CaravanData _caravanData;
        private LocationNodeSO _currentNode;

        private void Awake()
        {
#if UNITY_EDITOR
            if (_unitMuhafiz == null)
            {
                _unitMuhafiz = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/_Project/ScriptableObjects/Units/Unit_Muhafiz.asset");
            }
            if (_unitArcher == null)
            {
                _unitArcher = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitDataSO>("Assets/_Project/ScriptableObjects/Units/Unit_Okcu.asset");
            }
#endif
            FindChildReferencesIfMissing();
        }

        private void OnEnable()
        {
            FindChildReferencesIfMissing();
            BindSession();
            SetupButtonListeners();
            EventBus.Subscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
            EventBus.Subscribe<DayPassedEvent>(OnDayPassed);
            RefreshAll();
        }

        private void OnDisable()
        {
            RemoveButtonListeners();
            EventBus.Unsubscribe<CaravanStatsChangedEvent>(OnCaravanStatsChanged);
            EventBus.Unsubscribe<DayPassedEvent>(OnDayPassed);
        }

        /// <summary>
        /// Kervansaray ekranı açıldığında veya şehir değiştiğinde çağrılarak görünümü tazeler.
        /// </summary>
        public void RefreshView(LocationNodeSO currentNode = null)
        {
            if (currentNode != null)
            {
                _currentNode = currentNode;
            }

            FindChildReferencesIfMissing();
            BindSession();
            SetupButtonListeners();
            RefreshAll();
        }

        private void BindSession()
        {
            if (Services.TryGet<GameSession>(out var session))
            {
                session.SyncFromLegacyPrefsIfChanged();
                _gameSession = session;
                _caravanData = session.Caravan;
            }
            else if (CaravanManager.Instance != null)
            {
                _caravanData = CaravanManager.Instance.Caravan;
            }

            if (_caravanData == null)
            {
                Debug.LogWarning("[CaravanseraiInnUI] CaravanData bulunamadı, oturum varsayılan kervanına bağlanıyor.");
                _caravanData = new CaravanData();
            }
        }

        private void SetupButtonListeners()
        {
            if (_btnRest1Day != null)
            {
                _btnRest1Day.onClick.RemoveListener(OnRest1DayClicked);
                _btnRest1Day.onClick.AddListener(OnRest1DayClicked);
            }
            if (_btnBuySupplies != null)
            {
                _btnBuySupplies.onClick.RemoveListener(OnBuySuppliesClicked);
                _btnBuySupplies.onClick.AddListener(OnBuySuppliesClicked);
            }
            if (_btnHireMuhafiz != null)
            {
                _btnHireMuhafiz.onClick.RemoveListener(OnHireMuhafizClicked);
                _btnHireMuhafiz.onClick.AddListener(OnHireMuhafizClicked);
            }
            if (_btnHireArcher != null)
            {
                _btnHireArcher.onClick.RemoveListener(OnHireArcherClicked);
                _btnHireArcher.onClick.AddListener(OnHireArcherClicked);
            }
            if (_btnHireMule != null)
            {
                _btnHireMule.onClick.RemoveListener(OnHireMuleClicked);
                _btnHireMule.onClick.AddListener(OnHireMuleClicked);
            }
            if (_btnHireCamel != null)
            {
                _btnHireCamel.onClick.RemoveListener(OnHireCamelClicked);
                _btnHireCamel.onClick.AddListener(OnHireCamelClicked);
            }
            if (_btnHireSpy != null)
            {
                _btnHireSpy.onClick.RemoveListener(OnHireSpyClicked);
                _btnHireSpy.onClick.AddListener(OnHireSpyClicked);
            }

            Debug.Log($"[CaravanseraiInnUI] Butonlar dinleyicilere bağlandı. (Konakla: {_btnRest1Day != null}, Erzak: {_btnBuySupplies != null}, Muhafız: {_btnHireMuhafiz != null}, Okçu: {_btnHireArcher != null}, Katır: {_btnHireMule != null}, Deve: {_btnHireCamel != null}, Casus: {_btnHireSpy != null})");
        }

        private void RemoveButtonListeners()
        {
            if (_btnRest1Day != null) _btnRest1Day.onClick.RemoveListener(OnRest1DayClicked);
            if (_btnBuySupplies != null) _btnBuySupplies.onClick.RemoveListener(OnBuySuppliesClicked);
            if (_btnHireMuhafiz != null) _btnHireMuhafiz.onClick.RemoveListener(OnHireMuhafizClicked);
            if (_btnHireArcher != null) _btnHireArcher.onClick.RemoveListener(OnHireArcherClicked);
            if (_btnHireMule != null) _btnHireMule.onClick.RemoveListener(OnHireMuleClicked);
            if (_btnHireCamel != null) _btnHireCamel.onClick.RemoveListener(OnHireCamelClicked);
            if (_btnHireSpy != null) _btnHireSpy.onClick.RemoveListener(OnHireSpyClicked);
        }

        private void OnCaravanStatsChanged(CaravanStatsChangedEvent evt)
        {
            RefreshAll();
        }

        private void OnDayPassed(DayPassedEvent evt)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (_caravanData == null) BindSession();
            if (_caravanData == null) return;

            int currentAkce = _caravanData.CurrentAkce;

            // 1. Konaklama ve Erzak Kartı
            if (_btnRest1Day != null)
            {
                _btnRest1Day.interactable = currentAkce >= _restCostAkce;
            }
            if (_txtRestButtonLabel != null)
            {
                _txtRestButtonLabel.text = $"1 Gün Konakla ({_restCostAkce} Akçe)";
            }

            if (_btnBuySupplies != null)
            {
                _btnBuySupplies.interactable = currentAkce >= _suppliesCostAkce;
            }
            if (_txtSuppliesButtonLabel != null)
            {
                _txtSuppliesButtonLabel.text = $"+{_suppliesFoodAmount} Erzak Satın Al ({_suppliesCostAkce} Akçe)";
            }

            if (_txtRestDesc != null)
            {
                int injuredCount = CountInjuredGuards();
                string healthStatus = injuredCount > 0
                    ? $"<color=#E5534B>{injuredCount} yaralı muhafız var!</color>"
                    : "<color=#52D669>Tüm muhafızlar sağlıklı.</color>";
                _txtRestDesc.text =
                    $"Kervansarayda sıcak çorba ve emniyetli döşek kervanın moralini tazeler.\n" +
                    $"<b>Mevcut Erzak:</b> {_caravanData.CurrentFood} Günlük  ·  <b>Durum:</b> {healthStatus}";
            }

            // 2. Muhafız Ocağı Kartı
            bool canRecruit = _caravanData.CanRecruitGuard();
            int currentGuardCount = _caravanData.Guards != null ? _caravanData.Guards.Count : 0;

            if (_btnHireMuhafiz != null)
            {
                _btnHireMuhafiz.interactable = canRecruit && currentAkce >= _hireMuhafizCostAkce;
            }
            if (_txtHireMuhafizLabel != null)
            {
                _txtHireMuhafizLabel.text = $"Muhafız Kirala ({_hireMuhafizCostAkce} Akçe)";
            }

            if (_btnHireArcher != null)
            {
                _btnHireArcher.interactable = canRecruit && currentAkce >= _hireArcherCostAkce;
            }
            if (_txtHireArcherLabel != null)
            {
                _txtHireArcherLabel.text = $"Okçu Kirala ({_hireArcherCostAkce} Akçe)";
            }

            if (_txtGuardsDesc != null)
            {
                string capacityStr = canRecruit
                    ? $"Kadro: <b>{currentGuardCount}/{CaravanData.MaxGuardsCount}</b>"
                    : $"<color=#E69138>Kadro Dolu ({currentGuardCount}/{CaravanData.MaxGuardsCount})</color>";
                _txtGuardsDesc.text =
                    $"Pusulara ve kurt sürülerine karşı tecrübeli gaziler ve okçular kiralayın.\n" +
                    $"{capacityStr}  ·  Her muhafız kervanın caydırıcılığını artırır.";
            }

            // 3. Ahır & Binekler Kartı
            if (_btnHireMule != null)
            {
                _btnHireMule.interactable = currentAkce >= _hireMuleCostAkce;
            }
            if (_txtHireMuleLabel != null)
            {
                _txtHireMuleLabel.text = $"Katır Satın Al ({_hireMuleCostAkce} Akçe)";
            }

            if (_btnHireCamel != null)
            {
                _btnHireCamel.interactable = currentAkce >= _hireCamelCostAkce;
            }
            if (_txtHireCamelLabel != null)
            {
                _txtHireCamelLabel.text = $"Deve Satın Al ({_hireCamelCostAkce} Akçe)";
            }

            if (_txtMountsDesc != null)
            {
                _txtMountsDesc.text =
                    $"Yük kapasitenizi artırmak için güçlü binekler ve katırlar edinin.\n" +
                    $"<b>Binekler:</b> {_caravanData.Horses} At, {_caravanData.Mules} Katır, {_caravanData.Camels} Deve  ·  <b>Kapasite:</b> {_caravanData.MaxWeightCapacity:F0} Kg";
            }

            // Alt Bar: Casus Tutma Butonu
            if (_btnHireSpy != null)
            {
                _btnHireSpy.interactable = currentAkce >= _hireSpyCostAkce;
            }
            if (_txtHireSpyLabel != null)
            {
                _txtHireSpyLabel.text = $"CASUS TUT\n({_hireSpyCostAkce} Akçe)";
            }

            // Alt Bar: Dedikodu ve Kervan Fısıltıları
            UpdateRumorAndStatusText();
        }

        private void OnRest1DayClicked()
        {
            Debug.Log("[CaravanseraiInnUI] '1 Gün Konakla' tıklandı.");
            if (_caravanData == null) BindSession();
            if (_caravanData == null) return;

            bool success = _caravanData.RestAtInn(_restCostAkce, _restHealAmount, _restMoraleBonus);
            if (!success)
            {
                ShowFeedback("Konaklama ücreti için yeterli akçeniz yok!");
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayParchment();
            }

            // Günü ilerlet (Takvim, pazar toparlanması ve otomatik kayıt çalışır)
            if (_gameSession != null)
            {
                _gameSession.AdvanceDays(1);
            }

            ShowFeedback($"Kervansarayda 1 gün dinlenildi. Muhafızlar şifa buldu (+{_restHealAmount} HP, +{_restMoraleBonus} Moral).");
            SaveSessionState();
            RefreshAll();
        }

        private void OnBuySuppliesClicked()
        {
            Debug.Log("[CaravanseraiInnUI] 'Erzak Satın Al' tıklandı.");
            if (_caravanData == null) BindSession();
            if (_caravanData == null) return;

            bool success = _caravanData.BuySupplies(_suppliesFoodAmount, _suppliesCostAkce);
            if (!success)
            {
                ShowFeedback("Erzak ikmali için yeterli akçeniz yok!");
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoins();
            }

            ShowFeedback($"+{_suppliesFoodAmount} günlük erzak kervan kilerine yüklendi.");
            SaveSessionState();
            RefreshAll();
        }

        private void OnHireMuhafizClicked()
        {
            Debug.Log("[CaravanseraiInnUI] 'Muhafız Kirala' tıklandı.");
            if (_caravanData == null) BindSession();
            if (_caravanData == null) return;

            UnitDataSO targetUnit = _unitMuhafiz != null ? _unitMuhafiz : LoadFallbackUnit("Unit_Muhafiz");
            if (targetUnit == null)
            {
                Debug.LogWarning("[CaravanseraiInnUI] Muhafız birim tanımı (UnitDataSO) bulunamadı!");
                return;
            }

            bool success = _caravanData.RecruitGuardWithCost(targetUnit, _hireMuhafizCostAkce);
            if (!success)
            {
                ShowFeedback("Muhafız kiralamak için yeterli akçe veya kadro boşluğu yok!");
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoins();
            }

            ShowFeedback($"Kılıçlı bir muhafız ({targetUnit.UnitName}) kervan muhafız ocağına katıldı.");
            SaveSessionState();
            RefreshAll();
        }

        private void OnHireArcherClicked()
        {
            Debug.Log("[CaravanseraiInnUI] 'Okçu Kirala' tıklandı.");
            if (_caravanData == null) BindSession();
            if (_caravanData == null) return;

            UnitDataSO targetUnit = _unitArcher != null ? _unitArcher : LoadFallbackUnit("Unit_Okcu");
            if (targetUnit == null)
            {
                Debug.LogWarning("[CaravanseraiInnUI] Okçu birim tanımı (UnitDataSO) bulunamadı!");
                return;
            }

            bool success = _caravanData.RecruitGuardWithCost(targetUnit, _hireArcherCostAkce);
            if (!success)
            {
                ShowFeedback("Okçu kiralamak için yeterli akçe veya kadro boşluğu yok!");
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoins();
            }

            ShowFeedback($"Usta bir okçu ({targetUnit.UnitName}) kervana dahil oldu.");
            SaveSessionState();
            RefreshAll();
        }

        private void OnHireMuleClicked()
        {
            Debug.Log("[CaravanseraiInnUI] 'Katır Satın Al' tıklandı.");
            if (_caravanData == null) BindSession();
            if (_caravanData == null) return;

            bool success = _caravanData.AddPackAnimal(PackAnimalType.Mule, _muleCapacityBonusKg, _hireMuleCostAkce);
            if (!success)
            {
                ShowFeedback("Katır satın almak için yeterli akçeniz yok!");
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoins();
            }

            ShowFeedback($"Ahırdan yeni bir katır alındı. Kervan yük kapasitesi +{_muleCapacityBonusKg:F0} Kg arttı.");
            SaveSessionState();
            RefreshAll();
        }

        private void OnHireCamelClicked()
        {
            Debug.Log("[CaravanseraiInnUI] 'Deve Satın Al' tıklandı.");
            if (_caravanData == null) BindSession();
            if (_caravanData == null) return;

            bool success = _caravanData.AddPackAnimal(PackAnimalType.Camel, _camelCapacityBonusKg, _hireCamelCostAkce);
            if (!success)
            {
                ShowFeedback("Deve satın almak için yeterli akçeniz yok!");
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoins();
            }

            ShowFeedback($"Güçlü bir çöl devesi kervana katıldı. Yük kapasitesi +{_camelCapacityBonusKg:F0} Kg arttı.");
            SaveSessionState();
            RefreshAll();
        }

        private void OnHireSpyClicked()
        {
            Debug.Log("[CaravanseraiInnUI] 'Casus Tut' tıklandı.");
            if (_caravanData == null) BindSession();
            if (_caravanData == null) return;

            if (_caravanData.CurrentAkce < _hireSpyCostAkce)
            {
                ShowFeedback("Casus tutmak için yeterli akçeniz yok! (50 Akçe)");
                return;
            }

            _caravanData.ModifyAkce(-_hireSpyCostAkce);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCoins();
            }

            string secretReport = GenerateSecretSpyReport();
            if (_spyInfoText != null)
            {
                _spyInfoText.text =
                    $"<b><color=#52D669>Casus Meyhaneden Döndü (Gizli Rapor):</color></b>\n" +
                    $"<size=88%><color=#FFD966>{secretReport}</color></size>";
            }

            ShowFeedback("Casus 50 Akçe karşılığı meyhanede fısıltıları topladı, gizli istihbarat getirdi.");
            SaveSessionState();
            RefreshAll();
        }

        private string GenerateSecretSpyReport()
        {
            if (_currentNode != null)
            {
                switch (_currentNode.RulingFaction)
                {
                    case FactionType.Ottoman:
                        return "\"Gazi alpler Bursa ve İznik çevresinde devriyeyi sıklaştırmış. Tüccarlar Söğüt yolunda pusu tehlikesi olmadığını fısıldıyor lakin silah ve demir fiyatları hızla yükseliyor.\"";
                    case FactionType.Byzantine:
                        return "\"Tekfurun garnizonuna maaş ödenmemiş; Rum muhafızlar rüşvet almadan kapıdan kervan geçirmez olmuş. Gece vakti gizli patikaları kullananlar vergi vermeden pazara sızabiliyor.\"";
                    default:
                        return "\"Seyyahlar beylik sınırlarında kuraklık başladığını, tahıl ve tuz stoklarının hızla tükendiğini anlatıyor. Bu malları getiren kervanlar üç katı kârla satar.\"";
                }
            }

            return "\"Meyhanedeki haramiler yarın şafakta dağ yolundan geçecek zengin bir kervanı gözlerine kestirmiş. Yanınıza tecrübeli okçu almadan yola çıkmamanız salık verilir.\"";
        }

        private void UpdateRumorAndStatusText()
        {
            if (_spyInfoText == null) return;

            string locationName = _currentNode != null ? _currentNode.NodeName : "Han Meydanı";
            string factionRumor = GetAtmosphericRumor();

            _spyInfoText.text =
                $"<b><color=#FFD966>Kervansaray Fısıltıları ({locationName}):</color></b>\n" +
                $"<size=88%><color=#EAD8C3>{factionRumor}</color></size>";
        }

        private string GetAtmosphericRumor()
        {
            if (_currentNode != null)
            {
                switch (_currentNode.RulingFaction)
                {
                    case FactionType.Ottoman:
                        return "<i>\"Osman Gazi'nin alpleri Bitinya yollarını sıkı tutar. İpek yolunda haydut barınamaz lakin Bursa pazarında mal kıymetlidir.\"</i>";
                    case FactionType.Byzantine:
                        return "<i>\"Tekfur surları tahkim eder, Rum muhafızlar kervanlardan gümrük akçesi ister. Dikkatli olasın, taş döşeli yollarda pusu kol gezer.\"</i>";
                    default:
                        return "<i>\"Bozkırın seyyahları doğudan haber getirir; tuz ve demir taşıyan kervanlar beylik pazarlarında büyük kâr eder.\"</i>";
                }
            }

            return "<i>\"Kervan yolcuları akşam ateşinde toplanmış; yollarda kurt sürülerinin ve haramilerin gezindiği fısıldanıyor.\"</i>";
        }

        private void ShowFeedback(string message)
        {
            if (_statusFeedbackText != null)
            {
                _statusFeedbackText.text = message;
            }
            Debug.Log($"[CaravanseraiInnUI] {message}");
        }

        private int CountInjuredGuards()
        {
            if (_caravanData == null || _caravanData.Guards == null) return 0;
            int count = 0;
            for (int i = 0; i < _caravanData.Guards.Count; i++)
            {
                if (_caravanData.Guards[i] != null && _caravanData.Guards[i].IsInjured)
                {
                    count++;
                }
            }
            return count;
        }

        private void SaveSessionState()
        {
            if (_gameSession != null && Services.TryGet<SaveService>(out var saveService))
            {
                saveService.SaveSession(_gameSession);
            }
        }

        private UnitDataSO LoadFallbackUnit(string assetName)
        {
#if UNITY_EDITOR
            string assetPath = assetName.Contains("Okcu")
                ? "Assets/_Project/ScriptableObjects/Units/Unit_Okcu.asset"
                : "Assets/_Project/ScriptableObjects/Units/Unit_Muhafiz.asset";
            var loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<UnitDataSO>(assetPath);
            if (loaded != null) return loaded;
#endif
            var fallback = ScriptableObject.CreateInstance<UnitDataSO>();
            string cleanName = assetName.Contains("Okcu") ? "Okçu" : "Kılıçlı Muhafız";
            string id = assetName.ToLowerInvariant();
            fallback.Initialize(id, cleanName, 100, 100);
            return fallback;
        }

        private void FindChildReferencesIfMissing()
        {
            // 1. Kart: Card_RestAndSupply
            var cardRest = FindDeepChild(transform, "Card_RestAndSupply");
            if (cardRest != null)
            {
                if (_txtRestDesc == null)
                {
                    var desc = FindDeepChild(cardRest, "DescText");
                    if (desc != null) _txtRestDesc = desc.GetComponent<TextMeshProUGUI>();
                }
                if (_btnRest1Day == null)
                {
                    var b1 = FindDeepChild(cardRest, "Btn_Primary");
                    if (b1 != null)
                    {
                        _btnRest1Day = b1.GetComponent<Button>();
                        _txtRestButtonLabel = b1.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
                if (_btnBuySupplies == null)
                {
                    var b2 = FindDeepChild(cardRest, "Btn_Secondary");
                    if (b2 != null)
                    {
                        _btnBuySupplies = b2.GetComponent<Button>();
                        _txtSuppliesButtonLabel = b2.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
            }

            // 2. Kart: Card_HireGuards
            var cardGuards = FindDeepChild(transform, "Card_HireGuards");
            if (cardGuards != null)
            {
                if (_txtGuardsDesc == null)
                {
                    var desc = FindDeepChild(cardGuards, "DescText");
                    if (desc != null) _txtGuardsDesc = desc.GetComponent<TextMeshProUGUI>();
                }
                if (_btnHireMuhafiz == null)
                {
                    var b1 = FindDeepChild(cardGuards, "Btn_Primary");
                    if (b1 != null)
                    {
                        _btnHireMuhafiz = b1.GetComponent<Button>();
                        _txtHireMuhafizLabel = b1.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
                if (_btnHireArcher == null)
                {
                    var b2 = FindDeepChild(cardGuards, "Btn_Secondary");
                    if (b2 != null)
                    {
                        _btnHireArcher = b2.GetComponent<Button>();
                        _txtHireArcherLabel = b2.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
            }

            // 3. Kart: Card_HireMounts
            var cardMounts = FindDeepChild(transform, "Card_HireMounts");
            if (cardMounts != null)
            {
                if (_txtMountsDesc == null)
                {
                    var desc = FindDeepChild(cardMounts, "DescText");
                    if (desc != null) _txtMountsDesc = desc.GetComponent<TextMeshProUGUI>();
                }
                if (_btnHireMule == null)
                {
                    var b1 = FindDeepChild(cardMounts, "Btn_Primary");
                    if (b1 != null)
                    {
                        _btnHireMule = b1.GetComponent<Button>();
                        _txtHireMuleLabel = b1.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
                if (_btnHireCamel == null)
                {
                    var b2 = FindDeepChild(cardMounts, "Btn_Secondary");
                    if (b2 != null)
                    {
                        _btnHireCamel = b2.GetComponent<Button>();
                        _txtHireCamelLabel = b2.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
            }

            // Alt Bar: BottomBar_InnMeta
            var bottomBar = FindDeepChild(transform, "BottomBar_InnMeta");
            if (bottomBar != null)
            {
                if (_spyInfoText == null)
                {
                    var spy = FindDeepChild(bottomBar, "SpyInfoText");
                    if (spy != null) _spyInfoText = spy.GetComponent<TextMeshProUGUI>();
                }

                if (_btnHireSpy == null)
                {
                    var spyBtn = FindDeepChild(bottomBar, "Btn_HireSpy");
                    if (spyBtn != null)
                    {
                        _btnHireSpy = spyBtn.GetComponent<Button>();
                        _txtHireSpyLabel = spyBtn.GetComponentInChildren<TextMeshProUGUI>();
                    }
                }
            }
        }

        private static Transform FindDeepChild(Transform parent, string targetName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(targetName)) return null;
            if (string.Equals(parent.name, targetName, StringComparison.Ordinal)) return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                var result = FindDeepChild(parent.GetChild(i), targetName);
                if (result != null) return result;
            }

            return null;
        }
    }
}
