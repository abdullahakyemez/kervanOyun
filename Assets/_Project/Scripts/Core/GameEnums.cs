namespace Kervan.Core
{
    /// <summary>
    /// Oyundaki temel tarafları (fraksiyonları) tanımlar (GDD §11/4: Osmanlı, Bizans, Eşkıya, Vahşi Hayvanlar).
    /// Mevcut ScriptableObject (.asset) serileştirme indekslerini (0..4) korumak için değerler sabitlenmiştir.
    /// </summary>
    public enum FactionType
    {
        Ottoman = 0,        // Erken Osmanlı & Gazi Uç Beylikleri
        Steppe = 1,         // Bozkır Türkmenleri / Anadolu Beylikleri (Geriye dönük SO uyumu)
        SeljukLegacy = 2,   // Selçuklu Mirası & Kervansaraylar (Geriye dönük SO uyumu)
        Byzantine = 3,      // Doğu Roma (Bizans Tekfurları & Pontus Rum)
        Bandit = 4,         // Eşkıya / Dağ Haydutları
        WildAnimals = 5     // Vahşi Hayvanlar (Kurt Sürüleri vb.)
    }

    public enum UnitCombatRole
    {
        Melee,          // Kılıç/Kalkan - Ön hat savunma ve darbe
        Ranged,         // Okçu - Uzun menzil saldırı
        Hybrid          // Alp - Hem menzilli ok hem yakın dövüş kılıç
    }

    public enum TerrainType
    {
        Steppe,         // Bozkır / Ova
        Forest,         // Ormanlık Yol
        MountainPass,   // Dağlık Geçit
        Swamp           // Bataklık
    }

    public enum WeatherCondition
    {
        Clear,          // Açık / Ilık
        SnowStorm,      // Kar Fırtınası
        ExtremeHeat,    // Aşırı Sıcak
        HeavyRain       // Sağanak Yağış
    }

    public enum ItemCategory
    {
        Food,           // Tahıl, Kurutulmuş Et
        RawMaterial,    // Demir, Deri, Yün
        Luxury,         // İpek, Baharat, Mücevher
        Military        // Kılıç, Zırh, Yay
    }

    public enum CombatTurnPhase
    {
        PlayerTurn,
        EnemyTurn,
        RoundEnd
    }

    public enum ReputationLevel
    {
        Hostile,    // Düşman (-100 .. -50)
        Suspicious, // Şüpheli (-49 .. -1)
        Neutral,    // Nötr (0 .. 24)
        Friendly,   // Dost (25 .. 74)
        Allied      // Müttefik (75 .. 100)
    }

    public enum IntelType
    {
        MilitarySupply,    // Askeri ikmal / konvoy bilgisi
        FortressWeakness,  // Kale / sur zafiyeti
        LocalRebellion,    // Yerel isyan / hoşnutsuzluk
        TradeSecret,       // Gizli ticaret rotası / kaçakçılık
        RivalPlan          // Rakip beylik akın planı
    }
}

