using System.Collections.Generic;
using UnityEngine;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Dünya Haritası üzerindeki tüm aktif seyahat rotalarını (<see cref="TravelRouteSO"/>)
    /// tek bir merkezde toplayan ScriptableObject veri tabanı.
    /// <see cref="RouteGraph"/> çalışma anında komşuluk ve en kısa yol grafiğini bu listeden kurar.
    /// </summary>
    [CreateAssetMenu(fileName = "RouteDatabase", menuName = "Kervan/Travel/Route Database", order = 3)]
    public class RouteDatabaseSO : ScriptableObject
    {
        [Header("Kayıtlı Seyahat Rotaları")]
        [Tooltip("Haritada iki düğüm arasında geçerli olan tüm TravelRouteSO varlıkları (çift yönlü kabul edilir)")]
        [SerializeField] private List<TravelRouteSO> _routes = new List<TravelRouteSO>();

        /// <summary>
        /// Veri tabanında tanımlı tüm seyahat rotalarının salt okunur listesi.
        /// </summary>
        public IReadOnlyList<TravelRouteSO> Routes => _routes;
    }
}
