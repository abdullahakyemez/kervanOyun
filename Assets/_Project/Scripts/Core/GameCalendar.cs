using System;

namespace Kervan.Core
{
    /// <summary>
    /// 14. yüzyıl Anadolu takvimindeki 4 mevsimi temsil eder.
    /// </summary>
    public enum SeasonType
    {
        Spring = 0, // İlkbahar / Bahar (Mart, Nisan, Mayıs)
        Summer = 1, // Yaz (Haziran, Temmuz, Ağustos)
        Autumn = 2, // Sonbahar / Güz (Eylül, Ekim, Kasım)
        Winter = 3  // Kış (Aralık, Ocak, Şubat)
    }

    /// <summary>
    /// Oyun içi takvim ve mevsim hesaplamalarını yöneten saf C# yardımcı sınıfı.
    /// Başlangıç tarihi: 14 Mayıs 1326 (DayCount = 1).
    /// </summary>
    public static class GameCalendar
    {
        public const int StartYear = 1326;
        public const int StartMonth = 5;  // Mayıs
        public const int StartDay = 14;

        private static readonly string[] MonthNames = new string[]
        {
            "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
            "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
        };

        private static readonly DateTime BaseDate = new DateTime(StartYear, StartMonth, StartDay);

        /// <summary>
        /// Gün sayısını (DayCount) alıp takvim tarihine dönüştürür.
        /// </summary>
        /// <param name="dayCount">1 tabanlı oyun günü sayısı.</param>
        /// <returns>Örnek: "14 Mayıs 1326"</returns>
        public static string GetFormattedDate(int dayCount)
        {
            DateTime current = GetCurrentDateTime(dayCount);
            string monthName = MonthNames[current.Month - 1];
            return $"{current.Day} {monthName} {current.Year}";
        }

        /// <summary>
        /// Belirtilen oyun gününe denk gelen mevsimi döndürür.
        /// </summary>
        public static SeasonType GetSeason(int dayCount)
        {
            DateTime current = GetCurrentDateTime(dayCount);
            switch (current.Month)
            {
                case 3:
                case 4:
                case 5:
                    return SeasonType.Spring;
                case 6:
                case 7:
                case 8:
                    return SeasonType.Summer;
                case 9:
                case 10:
                case 11:
                    return SeasonType.Autumn;
                default:
                    return SeasonType.Winter;
            }
        }

        /// <summary>
        /// Mevsim tipinin arayüzde gösterilecek Türkçe metin karşılığını döndürür.
        /// </summary>
        public static string GetSeasonDisplayName(SeasonType season)
        {
            switch (season)
            {
                case SeasonType.Spring:
                    return "Bahar";
                case SeasonType.Summer:
                    return "Yaz";
                case SeasonType.Autumn:
                    return "Güz";
                case SeasonType.Winter:
                    return "Kış";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Belirtilen oyun gününe ait tarih ve mevsim detaylarını ayrıştırır.
        /// </summary>
        public static void GetDateDetails(int dayCount, out int day, out string monthName, out int year, out SeasonType season)
        {
            DateTime current = GetCurrentDateTime(dayCount);
            day = current.Day;
            monthName = MonthNames[current.Month - 1];
            year = current.Year;
            season = GetSeason(dayCount);
        }

        private static DateTime GetCurrentDateTime(int dayCount)
        {
            int safeOffset = Math.Max(0, dayCount - 1);
            return BaseDate.AddDays(safeOffset);
        }
    }
}
