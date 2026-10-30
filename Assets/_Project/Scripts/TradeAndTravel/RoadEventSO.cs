using System.Collections.Generic;
using UnityEngine;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Seyahat sırasında karşılaşılabilecek yol olaylarının ana kategorileri.
    /// A5 aşamasında <see cref="BanditAmbush"/> ve <see cref="WildBeastAttack"/>
    /// doğrudan Kaç / Haraç Ver / Savaş karar penceresini tetikler.
    /// </summary>
    public enum RoadEventCategory
    {
        Incident = 0,        // Lojistik yol hadisesi (aks kırılması, semer hasarı, fırtına vb.)
        Positive = 1,        // Olumlu karşılaşma (gezgin derviş, pınar/erzak bulma vb.)
        BanditAmbush = 2,    // Haydut / eşkıya pususu (Kaç / Haraç Ver / Savaş)
        WildBeastAttack = 3  // Yabani hayvan saldırısı — kurt sürüsü, ayı (Kaç / Savaş)
    }

    /// <summary>
    /// Sefer sırasında gün geçişlerinde tetiklenen yol hadiselerini, haydut pusularını ve
    /// yabani hayvan karşılaşmalarını tanımlayan ScriptableObject veri modeli.
    /// </summary>
    [CreateAssetMenu(fileName = "Event_New", menuName = "Kervan/Travel/Road Event", order = 3)]
    public class RoadEventSO : ScriptableObject
    {
        [Header("Kimlik ve Tanım")]
        [Tooltip("Benzersiz ASCII olay kimliği (ör. event_broken_axle, event_wolf_pack)")]
        [SerializeField] private string _eventId = "event_new";

        [Tooltip("Olay penceresinde görünecek Türkçe başlık")]
        [SerializeField] private string _eventTitle = "Yol Hadisesi";

        [Tooltip("Olay penceresinde görünecek tarihsel anlatım metni")]
        [TextArea(3, 6)]
        [SerializeField] private string _eventDescription = "Kervan yolda beklenmedik bir durumla karşılaştı.";

        [Tooltip("Olayın türü (Lojistik Hadise, Olumlu, Haydut Pususu, Yabani Hayvan Saldırısı)")]
        [SerializeField] private RoadEventCategory _category = RoadEventCategory.Incident;

        [Tooltip("Olay penceresinde gösterilecek opsiyonel vinyet/ikon görseli")]
        [SerializeField] private Sprite _eventIcon;

        [Header("Olasılık Ağırlığı")]
        [Tooltip("Havuz içinden seçilme ağırlığı (1.0 = standart olasılık)")]
        [Range(0.05f, 10f)]
        [SerializeField] private float _selectionWeight = 1f;

        [Header("Kervan Etkileri (Anlık)")]
        [Tooltip("Akçe değişimi (+ kazanç, - kayıp/masraf)")]
        [SerializeField] private int _akceDelta;

        [Tooltip("Erzak değişimi (+ bulunan erzak günü, - zayi olan erzak günü)")]
        [SerializeField] private int _foodDelta;

        [Tooltip("Muhafızların can/dinlenme değişimi (+ iyileşme, - yıpranma)")]
        [SerializeField] private int _healDelta;

        [Header("Savaş & Karar Hazırlığı (A5 Altyapısı)")]
        [Tooltip("A5 aşamasında Kaç / Haraç Ver / Savaş seçim penceresini açar mı?")]
        [SerializeField] private bool _requiresCombatChoice;

        [Tooltip("Haydut pususunda 'Haraç Ver' seçildiğinde istenecek akçe miktarı (Yabani hayvanlarda 0)")]
        [Min(0)]
        [SerializeField] private int _tributeAkceCost = 30;

        [Tooltip("Kaçış denemesinde temel başarı olasılığı (0.0 = İmkânsız, 1.0 = Kesin kaçış)")]
        [Range(0.05f, 1f)]
        [SerializeField] private float _fleeChance = 0.65f;

        public string EventId => _eventId;
        public string EventTitle => _eventTitle;
        public string EventDescription => _eventDescription;
        public RoadEventCategory Category => _category;
        public Sprite EventIcon => _eventIcon;
        public float SelectionWeight => Mathf.Max(0.05f, _selectionWeight);
        public int AkceDelta => _akceDelta;
        public int FoodDelta => _foodDelta;
        public int HealDelta => _healDelta;
        public bool RequiresCombatChoice => _requiresCombatChoice;
        public int TributeAkceCost => _tributeAkceCost;
        public float FleeChance => Mathf.Clamp01(_fleeChance > 0f ? _fleeChance : 0.65f);

        /// <summary>
        /// Olayın kervan üzerinde oluşturduğu net etkiyi oyuncu için okunaklı Türkçe özet metnine dönüştürür.
        /// </summary>
        public string BuildOutcomeSummary()
        {
            List<string> parts = new List<string>(3);

            if (_akceDelta != 0)
            {
                string sign = _akceDelta > 0 ? "+" : string.Empty;
                parts.Add($"{sign}{_akceDelta} Akçe");
            }

            if (_foodDelta != 0)
            {
                string sign = _foodDelta > 0 ? "+" : string.Empty;
                parts.Add($"{sign}{_foodDelta} Gün Erzak");
            }

            if (_healDelta != 0)
            {
                string sign = _healDelta > 0 ? "+" : string.Empty;
                parts.Add(_healDelta > 0
                    ? $"Muhafızlara {sign}{_healDelta} Şifa / Moral"
                    : $"Muhafızlarda {_healDelta} Yıpranma");
            }

            if (parts.Count == 0)
            {
                if (_category == RoadEventCategory.Positive)
                {
                    return "Muhafızların morali tazelendi";
                }

                if (_category == RoadEventCategory.BanditAmbush || _category == RoadEventCategory.WildBeastAttack)
                {
                    return "Kervan muhafızları tetikte bekledi";
                }

                return "Yolculuk tedbirle sürüyor";
            }

            return string.Join("  ·  ", parts);
        }

        /// <summary>
        /// Çalışma anında Inspector'da henüz SO atanmamışsa güvenli varsayılan olay oluşturmak için kullanılır.
        /// </summary>
        public static RoadEventSO CreateRuntimeInstance(
            string id,
            string title,
            string description,
            RoadEventCategory category,
            int akceDelta,
            int foodDelta,
            int healDelta = 0,
            float weight = 1f,
            int tributeCost = 0,
            float fleeChance = 0.65f)
        {
            RoadEventSO instance = CreateInstance<RoadEventSO>();
            instance.name = id;
            instance._eventId = id;
            instance._eventTitle = title;
            instance._eventDescription = description;
            instance._category = category;
            instance._akceDelta = akceDelta;
            instance._foodDelta = foodDelta;
            instance._healDelta = healDelta;
            instance._selectionWeight = Mathf.Max(0.05f, weight);
            instance._requiresCombatChoice = category == RoadEventCategory.BanditAmbush || category == RoadEventCategory.WildBeastAttack;
            instance._tributeAkceCost = tributeCost > 0 ? tributeCost : (category == RoadEventCategory.BanditAmbush ? 30 : 0);
            instance._fleeChance = fleeChance > 0f ? fleeChance : 0.65f;
            return instance;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(_eventId))
            {
                _eventId = _eventId.Trim().ToLowerInvariant();
            }
        }
#endif
    }
}
