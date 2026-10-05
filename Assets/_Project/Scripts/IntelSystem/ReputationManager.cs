using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;

namespace Kervan.IntelSystem
{
    public struct ReputationChangedEvent : IGameEvent
    {
        public FactionType Faction;
        public int NewScore;
        public ReputationLevel Level;

        public ReputationChangedEvent(FactionType faction, int newScore, ReputationLevel level)
        {
            Faction = faction;
            NewScore = newScore;
            Level = level;
        }
    }

    /// <summary>
    /// Beylikler ve fraksiyonlar arasındaki nüfuz, güven ve itibar motoru.
    /// Vergileri, şehir giriş izinlerini ve özel ayrıcalıkları yönetir.
    /// </summary>
    public class ReputationManager : MonoBehaviour
    {
        public static ReputationManager Instance { get; private set; }

        [System.Serializable]
        public class FactionRepEntry
        {
            public FactionType faction;
            [Range(-100, 100)] public int score;
        }

        [SerializeField] private List<FactionRepEntry> _initialReputations = new List<FactionRepEntry>
        {
            new FactionRepEntry { faction = FactionType.Ottoman, score = 30 },       // Dost başlangıç
            new FactionRepEntry { faction = FactionType.Steppe, score = 10 },        // Nötr (Germiyan)
            new FactionRepEntry { faction = FactionType.SeljukLegacy, score = 5 },   // Nötr (Karaman)
            new FactionRepEntry { faction = FactionType.Byzantine, score = -15 },    // Şüpheli (Tekfurlar)
            new FactionRepEntry { faction = FactionType.Bandit, score = -100 }       // Düşman (Haydutlar)
        };

        private readonly Dictionary<FactionType, int> _repScores = new Dictionary<FactionType, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeScores();
        }

        private void InitializeScores()
        {
            _repScores.Clear();
            foreach (var entry in _initialReputations)
            {
                _repScores[entry.faction] = Mathf.Clamp(entry.score, -100, 100);
            }
        }

        public int GetReputation(FactionType faction)
        {
            return _repScores.TryGetValue(faction, out int score) ? score : 0;
        }

        public ReputationLevel GetReputationLevel(FactionType faction)
        {
            int score = GetReputation(faction);
            if (score <= -50) return ReputationLevel.Hostile;
            if (score < 0) return ReputationLevel.Suspicious;
            if (score < 25) return ReputationLevel.Neutral;
            if (score < 75) return ReputationLevel.Friendly;
            return ReputationLevel.Allied;
        }

        public void AddReputation(FactionType faction, int amount)
        {
            int current = GetReputation(faction);
            int updated = Mathf.Clamp(current + amount, -100, 100);
            _repScores[faction] = updated;

            var level = GetReputationLevel(faction);
            EventBus.Publish(new ReputationChangedEvent(faction, updated, level));

            Debug.Log($"[Kervan-İtibar] {faction} itibarı değişti: {current} -> {updated} ({level})");
        }

        /// <summary>
        /// İtibar seviyesine göre pazar vergisi indirim veya ceza katsayısı.
        /// Allied: %50 indirim (0.5x), Friendly: %25 indirim (0.75x), Suspicious: %25 zam (1.25x), Hostile: %100 ceza (2.0x).
        /// </summary>
        public float GetTaxMultiplier(FactionType faction)
        {
            var level = GetReputationLevel(faction);
            switch (level)
            {
                case ReputationLevel.Allied: return 0.50f;     // %50 Muafiyet / Berat
                case ReputationLevel.Friendly: return 0.75f;   // %25 İndirim
                case ReputationLevel.Neutral: return 1.0f;     // Standart
                case ReputationLevel.Suspicious: return 1.25f; // %25 Ceza vergisi
                case ReputationLevel.Hostile: return 2.0f;     // %100 Ağır haraç
                default: return 1.0f;
            }
        }

        public bool CanEnterCity(FactionType faction)
        {
            // Düşman seviyesindeki şehirlere gizlenmeden / rüşvet vermeden girilemez
            return GetReputationLevel(faction) != ReputationLevel.Hostile;
        }

        public string GetFactionDisplayName(FactionType faction)
        {
            switch (faction)
            {
                case FactionType.Ottoman: return "Osmanlı Uç Beyliği";
                case FactionType.Steppe: return "Germiyanoğlu Beyliği";
                case FactionType.SeljukLegacy: return "Karamanoğlu & Selçuklu";
                case FactionType.Byzantine: return "Doğu Roma (Bizans)";
                case FactionType.Bandit: return "Dağ Eşkıyası";
                default: return faction.ToString();
            }
        }
    }
}
