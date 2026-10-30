using System;
using System.Collections.Generic;

namespace Kervan.Core.Save
{
    /// <summary>
    /// Disk üzerine JSON olarak kaydedilen ana oyun durumu veri modeli.
    /// Sürüm yükseltmelerinde geriye dönük uyumluluk için <see cref="saveVersion"/> alanı zorunludur.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int saveVersion = CurrentVersion;
        public string savedAtIso = string.Empty;
        public string currentNodeId = GameSession.DefaultStartNodeId;
        public int dayCount = 1;
        public bool hasSeenIntro = false;
        public CaravanSaveData caravan = new CaravanSaveData();
        public ActiveTravelSaveData activeTravel = new ActiveTravelSaveData();
        public List<string> visitedNodeIds = new List<string>();
        public List<CityMarketNodeSaveData> cityMarkets = new List<CityMarketNodeSaveData>();
    }

    /// <summary>
    /// Bir şehrin (düğümün) pazarındaki emtiaların kalıcı stok ve doygunluk kayıtlarını tutar.
    /// </summary>
    [Serializable]
    public class CityMarketNodeSaveData
    {
        public string nodeId = string.Empty;
        public List<CityMarketItemSaveData> items = new List<CityMarketItemSaveData>();
    }

    /// <summary>
    /// Bir şehir pazarındaki tek bir ticari malın kalıcı stok, başlangıç dengesi ve doygunluk kaydı.
    /// </summary>
    [Serializable]
    public class CityMarketItemSaveData
    {
        public string itemId = string.Empty;
        public int currentStock;
        public int initialStock;
        public float saturation;
    }

    /// <summary>
    /// Kervan kaynaklarının, bineklerinin ve envanter/muhafız kimliklerinin serileştirilebilir modeli.
    /// ScriptableObject referansları yalnızca ASCII ID olarak saklanır.
    /// </summary>
    [Serializable]
    public class CaravanSaveData
    {
        public int currentAkce = 500;
        public int currentFood = 20;
        public float maxWeightCapacity = 300f;
        public int horses = 2;
        public int mules = 2;
        public int camels = 1;
        public List<InventorySlotSaveData> items = new List<InventorySlotSaveData>();
        public List<GuardSaveData> guards = new List<GuardSaveData>();
    }

    /// <summary>
    /// Kervan envanterindeki tek bir ticari mal yuvasının kayıt verisi.
    /// </summary>
    [Serializable]
    public class InventorySlotSaveData
    {
        public string itemId = string.Empty;
        public int quantity;
        public int averagePurchasePrice;
    }

    /// <summary>
    /// Kervandaki tek bir muhafızın kayıt verisi.
    /// </summary>
    [Serializable]
    public class GuardSaveData
    {
        public string guardId = string.Empty;
        public string unitDataId = string.Empty;
        public int currentHealth;
        public int currentMorale;
    }

    /// <summary>
    /// Devam eden bir seyahatin kayıt verisi.
    /// </summary>
    [Serializable]
    public class ActiveTravelSaveData
    {
        public bool isActive;
        public string routeId = string.Empty;
        public string fromNodeId = string.Empty;
        public string toNodeId = string.Empty;
        public int daysElapsed;
        public int totalDays;
    }
}
