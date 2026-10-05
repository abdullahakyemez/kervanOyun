using System.Collections.Generic;
using UnityEngine;

namespace Kervan.Combat
{
    /// <summary>
    /// Altigen haritada Stamina bazli hareket menzili hesaplama ve
    /// en kisa yol bulma algoritmalarini calistiran statik yardimci sinif.
    /// </summary>
    public static class HexPathfinder
    {
        /// <summary>
        /// Baslangic noktasindan itibaren birimin Stamina'si yettigince ulasabilecegi
        /// tum yurunebilir altigenleri ve maliyetlerini BFS (Breadth-First Search) ile bulur.
        /// </summary>
        public static Dictionary<HexCoordinates, int> GetReachableTiles(HexGrid grid, HexCoordinates startCoords, int maxMovementCost)
        {
            var costSoFar = new Dictionary<HexCoordinates, int>();
            var frontier = new PriorityQueue<HexCoordinates>();

            costSoFar[startCoords] = 0;
            frontier.Enqueue(startCoords, 0);

            while (!frontier.IsEmpty)
            {
                HexCoordinates current = frontier.Dequeue();

                foreach (var neighborTile in grid.GetNeighbors(current))
                {
                    // Engel var mi veya uzerinde baska birim duruyor mu?
                    if (!neighborTile.IsWalkable) continue;

                    int newCost = costSoFar[current] + neighborTile.MovementCost;

                    if (newCost <= maxMovementCost)
                    {
                        if (!costSoFar.ContainsKey(neighborTile.Coordinates) || newCost < costSoFar[neighborTile.Coordinates])
                        {
                            costSoFar[neighborTile.Coordinates] = newCost;
                            frontier.Enqueue(neighborTile.Coordinates, newCost);
                        }
                    }
                }
            }

            // Baslangic noktasini yurume hedefi olmaktan cikar
            costSoFar.Remove(startCoords);
            return costSoFar;
        }

        /// <summary>
        /// Baslangictan hedefe olan en kisa yolu (A* / Dijkstra) dondurur.
        /// </summary>
        public static List<HexCoordinates> FindPath(HexGrid grid, HexCoordinates start, HexCoordinates target, int maxCost = 99)
        {
            var cameFrom = new Dictionary<HexCoordinates, HexCoordinates>();
            var costSoFar = new Dictionary<HexCoordinates, int>();
            var frontier = new PriorityQueue<HexCoordinates>();

            frontier.Enqueue(start, 0);
            cameFrom[start] = start;
            costSoFar[start] = 0;

            bool found = false;

            while (!frontier.IsEmpty)
            {
                HexCoordinates current = frontier.Dequeue();

                if (current == target)
                {
                    found = true;
                    break;
                }

                foreach (var neighborTile in grid.GetNeighbors(current))
                {
                    // Hedef hucre dolu olsa bile (saldiri icin) incelenebilir, ancak ara hucreler yurunebilir olmali
                    bool isTarget = (neighborTile.Coordinates == target);
                    if (!neighborTile.IsWalkable && !isTarget) continue;

                    int newCost = costSoFar[current] + neighborTile.MovementCost;
                    if (newCost <= maxCost)
                    {
                        if (!costSoFar.ContainsKey(neighborTile.Coordinates) || newCost < costSoFar[neighborTile.Coordinates])
                        {
                            costSoFar[neighborTile.Coordinates] = newCost;
                            int priority = newCost + HexCoordinates.Distance(neighborTile.Coordinates, target);
                            frontier.Enqueue(neighborTile.Coordinates, priority);
                            cameFrom[neighborTile.Coordinates] = current;
                        }
                    }
                }
            }

            if (!found) return new List<HexCoordinates>();

            // Yolu sondan basa dogru yeniden insa et
            var path = new List<HexCoordinates>();
            HexCoordinates curr = target;
            while (curr != start)
            {
                path.Add(curr);
                curr = cameFrom[curr];
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Belirli bir menzil (Range) icerisindeki tum altigen hucreleri dondurur.
        /// </summary>
        public static List<HexCoordinates> GetTilesInRange(HexCoordinates origin, int range)
        {
            var results = new List<HexCoordinates>();
            for (int q = -range; q <= range; q++)
            {
                int rMin = Mathf.Max(-range, -q - range);
                int rMax = Mathf.Min(range, -q + range);
                for (int r = rMin; r <= rMax; r++)
                {
                    if (q != 0 || r != 0) // Kendisi haric
                    {
                        results.Add(origin + new HexCoordinates(q, r));
                    }
                }
            }
            return results;
        }

        #region Basit Priority Queue Yardimcisi

        private class PriorityQueue<T>
        {
            private readonly List<KeyValuePair<T, int>> _elements = new List<KeyValuePair<T, int>>();

            public bool IsEmpty => _elements.Count == 0;

            public void Enqueue(T item, int priority)
            {
                _elements.Add(new KeyValuePair<T, int>(item, priority));
            }

            public T Dequeue()
            {
                int bestIndex = 0;
                for (int i = 1; i < _elements.Count; i++)
                {
                    if (_elements[i].Value < _elements[bestIndex].Value)
                    {
                        bestIndex = i;
                    }
                }

                T bestItem = _elements[bestIndex].Key;
                _elements.RemoveAt(bestIndex);
                return bestItem;
            }
        }

        #endregion
    }
}
