using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;
using Kervan.Core.Save;

namespace Kervan.IntelSystem
{
    /// <summary>
    /// Bir fraksiyon nezdindeki itibar puanı veya kademesi değiştiğinde yayınlanır.
    /// </summary>
    public readonly struct ReputationChangedEvent : IGameEvent
    {
        public readonly FactionType Faction;
        public readonly int NewScore;
        public readonly ReputationLevel Level;

        public ReputationChangedEvent(FactionType faction, int newScore, ReputationLevel level)
        {
            Faction = faction;
            NewScore = newScore;
            Level = level;
        }
    }

    /// <summary>
    /// Beylikler ve fraksiyonlar arasındaki nüfuz, güven ve itibar motoru (saf C# servisi).
    /// Vergileri, şehir giriş izinlerini ve özel ayrıcalıkları yönetir.
    /// </summary>
    public class ReputationManager
    {
        public const int MinReputationScore = -100;
        public const int MaxReputationScore = 100;

        /// <summary>
        /// Servis bulucu (<see cref="Services"/>) üzerinden aktif <see cref="ReputationManager"/> örneğine erişim sağlar.
        /// </summary>
        public static ReputationManager Instance =>
            Services.TryGet<ReputationManager>(out var manager) ? manager : null;

        [Serializable]
        public class FactionRepEntry
        {
            public FactionType faction;
            [Range(-100, 100)] public int score;
        }

        private readonly Dictionary<FactionType, int> _repScores = new Dictionary<FactionType, int>();

        public ReputationManager()
        {
            ResetToDefaults();
        }

        /// <summary>
        /// Tüm fraksiyon itibar puanlarını yeni oyun başlangıç değerlerine sıfırlar.
        /// </summary>
        public void ResetToDefaults()
        {
            _repScores.Clear();
            _repScores[FactionType.Ottoman] = 30;        // Dost başlangıç (Osmanlı Uç Beyliği)
            _repScores[FactionType.Byzantine] = -15;     // Şüpheli (Bizans Tekfurları)
            _repScores[FactionType.Bandit] = -100;       // Düşman (Dağ Eşkıyaları)
            _repScores[FactionType.WildAnimals] = -100;  // Doğal Tehdit (Vahşi Hayvanlar)

            // Geriye dönük ScriptableObject uyumu (Anadolu Beylikleri)
            _repScores[FactionType.Steppe] = 10;         // Nötr (Germiyan / Bozkır)
            _repScores[FactionType.SeljukLegacy] = 5;    // Nötr (Karaman / Selçuklu Mirası)
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
            int updated = Mathf.Clamp(current + amount, MinReputationScore, MaxReputationScore);
            _repScores[faction] = updated;

            var level = GetReputationLevel(faction);
            EventBus.Publish(new ReputationChangedEvent(faction, updated, level));

            Debug.Log($"[ReputationManager] {faction} itibarı değişti: {current} -> {updated} ({level})");
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
                case FactionType.WildAnimals: return "Vahşi Hayvanlar";
                default: return faction.ToString();
            }
        }

        /// <summary>
        /// Mevcut itibar puanlarını JSON kaydı için serileştirilebilir listeye dönüştürür.
        /// </summary>
        public List<FactionReputationSaveData> ToSaveData()
        {
            var list = new List<FactionReputationSaveData>(_repScores.Count);
            foreach (var pair in _repScores)
            {
                list.Add(new FactionReputationSaveData
                {
                    faction = pair.Key,
                    score = Mathf.Clamp(pair.Value, MinReputationScore, MaxReputationScore)
                });
            }
            return list;
        }

        /// <summary>
        /// Diskten okunan itibar kayıtlarını çalışma anı sözlüğüne uygular.
        /// </summary>
        public void ApplySaveData(List<FactionReputationSaveData> savedReputations)
        {
            ResetToDefaults();
            if (savedReputations == null || savedReputations.Count == 0) return;

            for (int i = 0; i < savedReputations.Count; i++)
            {
                var entry = savedReputations[i];
                if (entry == null) continue;
                _repScores[entry.faction] = Mathf.Clamp(entry.score, MinReputationScore, MaxReputationScore);
            }
        }
    }
}
