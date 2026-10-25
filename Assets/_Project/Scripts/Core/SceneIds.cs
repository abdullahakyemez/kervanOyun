namespace Kervan.Core
{
    /// <summary>
    /// Projedeki tüm Unity sahnelerinin adlarını tek merkezde toplayan sabit sınıfı.
    /// Dağınık string sahne adı kullanımını ve yazım hatalarını (TD-14) önler.
    /// </summary>
    public static class SceneIds
    {
        public const string Boot = "00_Boot";
        public const string MainMenu = "MainMenuScene";
        public const string WorldMap = "WorldMapScene";
        public const string Travel = "TravelScene";
        public const string Market = "MarketScene";
        public const string TownHub = "TownHubScene";
        public const string Combat = "CombatScene";
    }
}
