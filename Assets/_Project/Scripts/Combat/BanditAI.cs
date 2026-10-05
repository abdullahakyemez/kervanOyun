using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kervan.Combat
{
    /// <summary>
    /// Haydut ve yabani dusmanlarin sira tabanli taktiksel karar
    /// mekanizmasini (AI) calistiran kontrolcu.
    /// </summary>
    public static class BanditAI
    {
        public static IEnumerator ExecuteAITurn(CombatUnit bandit, HexGrid grid, List<CombatUnit> playerUnits, Action onFinish)
        {
            if (bandit == null || bandit.IsDead)
            {
                onFinish?.Invoke();
                yield break;
            }

            // 1. Yasayan en yakin dost birimi bul
            CombatUnit closestTarget = null;
            int minDistance = int.MaxValue;

            for (int i = 0; i < playerUnits.Count; i++)
            {
                var unit = playerUnits[i];
                if (unit != null && !unit.IsDead)
                {
                    int dist = HexCoordinates.Distance(bandit.Coordinates, unit.Coordinates);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestTarget = unit;
                    }
                }
            }

            if (closestTarget == null)
            {
                onFinish?.Invoke();
                yield break;
            }

            // 2. Halihazirda vurabiliyor muyuz?
            if (bandit.CanAttack(closestTarget))
            {
                yield return new WaitForSeconds(0.4f);
                bandit.ExecuteAttack(closestTarget);
                yield return new WaitForSeconds(0.4f);
                onFinish?.Invoke();
                yield break;
            }

            // 3. Vuramiyorsak hedefe yaklasacak yolu bul
            var fullPath = HexPathfinder.FindPath(grid, bandit.Coordinates, closestTarget.Coordinates);
            if (fullPath.Count > 1)
            {
                // Hedefin uzerine degil, hedeften bir onceki altigene kadar yuru
                fullPath.RemoveAt(fullPath.Count - 1);

                // Haydudun stamina puanina gore adim sayisini sınırla
                int maxSteps = Mathf.Min(bandit.CurrentStamina, fullPath.Count);
                if (maxSteps > 0)
                {
                    var reachableSubPath = fullPath.GetRange(0, maxSteps);
                    bool moveFinished = false;
                    bandit.MoveAlongPath(reachableSubPath, grid, () => moveFinished = true);

                    while (!moveFinished) yield return null;
                    yield return new WaitForSeconds(0.2f);
                }
            }

            // 4. Yurume sonrasi vurabiliyorsa vur
            if (bandit.CanAttack(closestTarget))
            {
                yield return new WaitForSeconds(0.3f);
                bandit.ExecuteAttack(closestTarget);
                yield return new WaitForSeconds(0.4f);
            }

            onFinish?.Invoke();
        }
    }
}
