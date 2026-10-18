using UnityEngine;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Geriye dönük uyumluluk için kervan verilerini doğrudan GameSession üzerinden sunan köprü bileşen.
    /// </summary>
    public class CaravanManager : MonoBehaviour
    {
        public static CaravanManager Instance { get; private set; }

        [SerializeField] private CaravanData _caravanData = new CaravanData();

        public CaravanData Caravan =>
            Services.TryGet<GameSession>(out var session) && session.Caravan != null
                ? session.Caravan
                : _caravanData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Services.Register(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Services.Unregister(this);
            }
        }
    }
}
