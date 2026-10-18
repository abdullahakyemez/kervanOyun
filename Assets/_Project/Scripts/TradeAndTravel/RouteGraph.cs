using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// İki düğüm arasındaki rota sorgusunun (doğrudan komşuluk veya aktarmalı en kısa yol) sonucunu taşır.
    /// </summary>
    public readonly struct RoutePathResult
    {
        /// <summary>Hedef düğüme grafik üzerinden ulaşılabiliyor mu?</summary>
        public readonly bool IsReachable;

        /// <summary>Hedef düğüm doğrudan komşu mu (tek rotayla gidilebiliyor mu)?</summary>
        public readonly bool IsDirect;

        /// <summary>Doğrudan komşuysa kullanılan <see cref="TravelRouteSO"/>; değilse ilk adımın rotası.</summary>
        public readonly TravelRouteSO DirectRoute;

        /// <summary>Toplam tahmini seyahat süresi (gün).</summary>
        public readonly int TotalDays;

        /// <summary>Toplam tahmini erzak gereksinimi (günlük erzak birimi).</summary>
        public readonly int TotalFoodRequired;

        /// <summary>Aktarmalı yolda uğranacak ilk komşu düğümün kimliği (ör. "node_eskisehir").</summary>
        public readonly string FirstHopNodeId;

        /// <summary>Aktarmalı yolda uğranacak ilk komşu düğümün SO referansı.</summary>
        public readonly LocationNodeSO FirstHopNode;

        public RoutePathResult(
            bool isReachable,
            bool isDirect,
            TravelRouteSO directRoute,
            int totalDays,
            int totalFoodRequired,
            string firstHopNodeId,
            LocationNodeSO firstHopNode)
        {
            IsReachable = isReachable;
            IsDirect = isDirect;
            DirectRoute = directRoute;
            TotalDays = totalDays;
            TotalFoodRequired = totalFoodRequired;
            FirstHopNodeId = firstHopNodeId ?? string.Empty;
            FirstHopNode = firstHopNode;
        }

        public static RoutePathResult Unreachable => new RoutePathResult(false, false, null, 0, 0, string.Empty, null);
    }

    /// <summary>
    /// <see cref="TravelRouteSO"/> tanımlarından çift yönlü (undirected) komşuluk grafiği kuran
    /// ve iki düğüm arasındaki doğrudan rotayı veya en kısa aktarmalı yolu (Dijkstra) hesaplayan saf C# sınıfı.
    /// Tüm düğüm eşleştirmeleri ASCII <c>NodeId</c> ve <see cref="StringComparer.Ordinal"/> ile yapılır.
    /// </summary>
    public class RouteGraph
    {
        private readonly struct Edge
        {
            public readonly string TargetNodeId;
            public readonly LocationNodeSO TargetNode;
            public readonly TravelRouteSO Route;
            public readonly int Days;
            public readonly int FoodCost;

            public Edge(string targetNodeId, LocationNodeSO targetNode, TravelRouteSO route)
            {
                TargetNodeId = targetNodeId;
                TargetNode = targetNode;
                Route = route;
                Days = Mathf.Max(1, route.DistanceDays);
                FoodCost = CalculateRouteFoodCost(route);
            }
        }

        private readonly Dictionary<string, List<Edge>> _adjacency = new Dictionary<string, List<Edge>>(StringComparer.Ordinal);
        private readonly Dictionary<string, LocationNodeSO> _nodesById = new Dictionary<string, LocationNodeSO>(StringComparer.Ordinal);
        private int _routeCount;

        /// <summary>
        /// Grafikte kayıtlı geçerli rota sayısı.
        /// </summary>
        public int RouteCount => _routeCount;

        public RouteGraph(RouteDatabaseSO database)
        {
            if (database != null && database.Routes != null)
            {
                BuildGraph(database.Routes);
            }
        }

        public RouteGraph(IEnumerable<TravelRouteSO> routes)
        {
            if (routes != null)
            {
                BuildGraph(routes);
            }
        }

        /// <summary>
        /// Verilen rota listesinden çift yönlü komşuluk grafiğini oluşturur.
        /// </summary>
        public void BuildGraph(IEnumerable<TravelRouteSO> routes)
        {
            _adjacency.Clear();
            _nodesById.Clear();
            _routeCount = 0;

            if (routes == null) return;

            foreach (var route in routes)
            {
                if (route == null || route.FromNode == null || route.ToNode == null) continue;

                string fromId = route.FromNode.NodeId;
                string toId = route.ToNode.NodeId;

                if (string.IsNullOrWhiteSpace(fromId) || string.IsNullOrWhiteSpace(toId)) continue;
                if (string.Equals(fromId, toId, StringComparison.Ordinal)) continue;

                _nodesById[fromId] = route.FromNode;
                _nodesById[toId] = route.ToNode;

                AddDirectedEdge(fromId, toId, route.ToNode, route);
                AddDirectedEdge(toId, fromId, route.FromNode, route);
                _routeCount++;
            }
        }

        /// <summary>
        /// Bir rotanın mesafe günü ve erzak tüketim çarpanına göre tahmini erzak ihtiyacını hesaplar.
        /// </summary>
        public static int CalculateRouteFoodCost(TravelRouteSO route)
        {
            if (route == null) return 0;
            int days = Mathf.Max(1, route.DistanceDays);
            float multiplier = Mathf.Max(0.1f, route.FoodConsumptionMultiplier);
            return Mathf.Max(1, Mathf.CeilToInt(days * multiplier));
        }

        /// <summary>
        /// İki düğüm arasında doğrudan (tek adımlık) bir <see cref="TravelRouteSO"/> olup olmadığını kontrol eder.
        /// </summary>
        public bool TryGetDirectRoute(string fromNodeId, string toNodeId, out TravelRouteSO route)
        {
            route = null;
            if (string.IsNullOrWhiteSpace(fromNodeId) || string.IsNullOrWhiteSpace(toNodeId)) return false;

            if (!_adjacency.TryGetValue(fromNodeId, out List<Edge> edges)) return false;

            for (int i = 0; i < edges.Count; i++)
            {
                if (string.Equals(edges[i].TargetNodeId, toNodeId, StringComparison.Ordinal))
                {
                    route = edges[i].Route;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Başlangıç ve hedef düğüm arasındaki doğrudan rotayı veya en az gün süren aktarmalı yolu (Dijkstra) bulur.
        /// </summary>
        public bool TryFindPath(string fromNodeId, string toNodeId, out RoutePathResult result)
        {
            result = RoutePathResult.Unreachable;

            if (string.IsNullOrWhiteSpace(fromNodeId) || string.IsNullOrWhiteSpace(toNodeId))
            {
                return false;
            }

            if (string.Equals(fromNodeId, toNodeId, StringComparison.Ordinal))
            {
                _nodesById.TryGetValue(fromNodeId, out LocationNodeSO sameNode);
                result = new RoutePathResult(true, true, null, 0, 0, fromNodeId, sameNode);
                return true;
            }

            // 1. Önce doğrudan komşuluk kontrolü
            if (TryGetDirectRoute(fromNodeId, toNodeId, out TravelRouteSO directRoute))
            {
                _nodesById.TryGetValue(toNodeId, out LocationNodeSO targetNode);
                int days = Mathf.Max(1, directRoute.DistanceDays);
                int food = CalculateRouteFoodCost(directRoute);
                result = new RoutePathResult(true, true, directRoute, days, food, toNodeId, targetNode);
                return true;
            }

            // 2. Doğrudan komşu değilse Dijkstra ile en az gün süren aktarmalı yolu ara
            if (!_adjacency.ContainsKey(fromNodeId) || !_adjacency.ContainsKey(toNodeId))
            {
                return false;
            }

            var minDays = new Dictionary<string, int>(StringComparer.Ordinal);
            var minFood = new Dictionary<string, int>(StringComparer.Ordinal);
            var firstHopId = new Dictionary<string, string>(StringComparer.Ordinal);
            var firstEdgeRoute = new Dictionary<string, TravelRouteSO>(StringComparer.Ordinal);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            var frontier = new List<string>();

            minDays[fromNodeId] = 0;
            minFood[fromNodeId] = 0;
            frontier.Add(fromNodeId);

            while (frontier.Count > 0)
            {
                int bestIndex = 0;
                string currentId = frontier[0];
                int currentBestDays = minDays[currentId];

                for (int i = 1; i < frontier.Count; i++)
                {
                    string candidateId = frontier[i];
                    int candidateDays = minDays[candidateId];
                    if (candidateDays < currentBestDays)
                    {
                        currentBestDays = candidateDays;
                        currentId = candidateId;
                        bestIndex = i;
                    }
                }

                frontier.RemoveAt(bestIndex);

                if (string.Equals(currentId, toNodeId, StringComparison.Ordinal))
                {
                    string hopId = firstHopId[toNodeId];
                    _nodesById.TryGetValue(hopId, out LocationNodeSO hopNode);
                    TravelRouteSO initialRoute = firstEdgeRoute[toNodeId];

                    result = new RoutePathResult(
                        isReachable: true,
                        isDirect: false,
                        directRoute: initialRoute,
                        totalDays: minDays[toNodeId],
                        totalFoodRequired: minFood[toNodeId],
                        firstHopNodeId: hopId,
                        firstHopNode: hopNode
                    );
                    return true;
                }

                if (!visited.Add(currentId)) continue;
                if (!_adjacency.TryGetValue(currentId, out List<Edge> neighbors)) continue;

                for (int i = 0; i < neighbors.Count; i++)
                {
                    Edge edge = neighbors[i];
                    if (visited.Contains(edge.TargetNodeId)) continue;

                    int newDays = currentBestDays + edge.Days;
                    int newFood = minFood[currentId] + edge.FoodCost;

                    if (!minDays.TryGetValue(edge.TargetNodeId, out int existingDays) || newDays < existingDays)
                    {
                        minDays[edge.TargetNodeId] = newDays;
                        minFood[edge.TargetNodeId] = newFood;

                        if (string.Equals(currentId, fromNodeId, StringComparison.Ordinal))
                        {
                            firstHopId[edge.TargetNodeId] = edge.TargetNodeId;
                            firstEdgeRoute[edge.TargetNodeId] = edge.Route;
                        }
                        else
                        {
                            firstHopId[edge.TargetNodeId] = firstHopId[currentId];
                            firstEdgeRoute[edge.TargetNodeId] = firstEdgeRoute[currentId];
                        }

                        if (!frontier.Contains(edge.TargetNodeId))
                        {
                            frontier.Add(edge.TargetNodeId);
                        }
                    }
                }
            }

            return false;
        }

        private void AddDirectedEdge(string fromId, string toId, LocationNodeSO targetNode, TravelRouteSO route)
        {
            if (!_adjacency.TryGetValue(fromId, out List<Edge> edges))
            {
                edges = new List<Edge>();
                _adjacency[fromId] = edges;
            }

            // Aynı iki düğüm arasında mükerrer kayıt varsa en kısa günlü olanı tut
            for (int i = 0; i < edges.Count; i++)
            {
                if (string.Equals(edges[i].TargetNodeId, toId, StringComparison.Ordinal))
                {
                    if (route.DistanceDays < edges[i].Route.DistanceDays)
                    {
                        edges[i] = new Edge(toId, targetNode, route);
                    }
                    return;
                }
            }

            edges.Add(new Edge(toId, targetNode, route));
        }
    }
}
