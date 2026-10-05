using UnityEngine;

namespace Kervan.Combat
{
    public readonly struct CombatAttackResult
    {
        public readonly bool IsHit;
        public readonly int RawDamage;
        public readonly int ArmorDamage;
        public readonly int HealthDamage;
        public readonly int RemainingHealth;
        public readonly int RemainingArmor;
        public readonly bool TargetDied;
        public readonly bool TargetPanicked;

        public CombatAttackResult(bool isHit, int rawDamage, int armorDamage, int healthDamage, int remainingHealth, int remainingArmor, bool targetDied, bool targetPanicked)
        {
            IsHit = isHit;
            RawDamage = rawDamage;
            ArmorDamage = armorDamage;
            HealthDamage = healthDamage;
            RemainingHealth = remainingHealth;
            RemainingArmor = remainingArmor;
            TargetDied = targetDied;
            TargetPanicked = targetPanicked;
        }

        public static CombatAttackResult Missed()
        {
            return new CombatAttackResult(false, 0, 0, 0, 0, 0, false, false);
        }
    }

    /// <summary>
    /// Savas isabet zarlarini, zirh sonumlemesini ve moral kirilma
    /// matemetigini hesaplayan cekirdek sinif.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// Saldiran ve savunan birim arasindaki catisma sonucunu cozumler.
        /// </summary>
        public static CombatAttackResult ResolveAttack(CombatUnit attacker, CombatUnit defender, int distance)
        {
            if (attacker == null || defender == null) return CombatAttackResult.Missed();

            // 1. Isabet Zari (Accuracy RNG)
            float accuracy = attacker.Data.BaseAccuracy;

            // Moral etkisi: Yuksek moral +%10 isabet, panik -%30 isabet
            if (attacker.CurrentMorale > 100) accuracy += 0.05f;
            else if (attacker.CurrentMorale < 40) accuracy -= 0.25f;

            // Mesafe cezasi (Yalnizca menzilli saldirilar icin 2 kareden uzaksa)
            if (distance > 2)
            {
                accuracy -= (distance - 2) * 0.05f;
            }

            accuracy = Mathf.Clamp(accuracy, 0.20f, 0.95f); // Her zaman en az %20, en fazla %95
            bool isHit = Random.value <= accuracy;

            if (!isHit)
            {
                return CombatAttackResult.Missed();
            }

            // 2. Ham Hasar Hesabi
            int rawDamage = Random.Range(attacker.Data.MinDamage, attacker.Data.MaxDamage + 1);

            // 3. Zirh Sonumleme Matemigi
            int currentArmor = defender.CurrentArmor;
            int armorDamage = 0;
            int healthDamage = 0;

            if (currentArmor > 0)
            {
                // Siper (Defend) modunda ise zirh hasarin %70'ini emer
                float absorptionRate = defender.IsDefending ? 0.70f : 0.50f;
                int absorbable = Mathf.RoundToInt(rawDamage * absorptionRate);

                armorDamage = Mathf.Min(currentArmor, absorbable);
                healthDamage = rawDamage - armorDamage;
            }
            else
            {
                // Zirh yoksa hasarin tamami cana gider
                healthDamage = rawDamage;
            }

            int finalHealth = Mathf.Max(0, defender.CurrentHealth - healthDamage);
            int finalArmor = Mathf.Max(0, defender.CurrentArmor - armorDamage);
            bool targetDied = (finalHealth <= 0);

            // 4. Moral Darbesi (Hasar alan birimin morali duser)
            int moraleLoss = targetDied ? 100 : (healthDamage / 2) + 5;
            int newMorale = Mathf.Max(0, defender.CurrentMorale - moraleLoss);
            bool targetPanicked = (!targetDied && newMorale < 25);

            return new CombatAttackResult(
                isHit: true,
                rawDamage: rawDamage,
                armorDamage: armorDamage,
                healthDamage: healthDamage,
                remainingHealth: finalHealth,
                remainingArmor: finalArmor,
                targetDied: targetDied,
                targetPanicked: targetPanicked
            );
        }

        /// <summary>
        /// UI icin tahmini hasar ve isabet orani tahmini uretir (Floating Prediction).
        /// </summary>
        public static (int hitChancePercent, int minEstimatedDamage, int maxEstimatedDamage) PredictCombat(CombatUnit attacker, CombatUnit defender, int distance)
        {
            if (attacker == null || defender == null) return (0, 0, 0);

            float accuracy = attacker.Data.BaseAccuracy;
            if (distance > 2) accuracy -= (distance - 2) * 0.05f;
            int hitPercent = Mathf.RoundToInt(Mathf.Clamp(accuracy, 0.20f, 0.95f) * 100f);

            int minDmg = attacker.Data.MinDamage;
            int maxDmg = attacker.Data.MaxDamage;

            return (hitPercent, minDmg, maxDmg);
        }
    }
}
