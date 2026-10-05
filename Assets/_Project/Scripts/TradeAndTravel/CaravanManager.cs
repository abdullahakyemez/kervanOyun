using UnityEngine;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Kervan verilerini sahneler arasında kalıcı (DontDestroyOnLoad) tutan tekil yönetici bileşen.
    /// </summary>
    public class CaravanManager : MonoBehaviour
    {
        public static CaravanManager Instance { get; private set; }

        [SerializeField] private CaravanData _caravanData = new CaravanData();

        public CaravanData Caravan => _caravanData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
