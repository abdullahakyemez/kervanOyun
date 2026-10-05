#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using Kervan.Core;
using Kervan.IntelSystem;

namespace Kervan.EditorTools
{
    public static class IntelDataGenerator
    {
        [MenuItem("Kervan/Olustur/Istihbarat Verilerini Uret")]
        public static void GenerateIntels()
        {
            string folderPath = "Assets/_Project/ScriptableObjects/Intels";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            CreateIntelAsset(folderPath, "Intel_BilecikKonvoy",
                "Bilecik Tekfurunun İkmal Konvoyu",
                "Tekfurun İznik surlarına göndereceği demir ve silah sevkiyatının çıkış saati.",
                IntelType.MilitarySupply, "Bilecik", FactionType.Ottoman, 20, 200, 15, 2f);

            CreateIntelAsset(folderPath, "Intel_IznikSurZafiyeti",
                "İznik Doğu Kapısı Savunma Zaafı",
                "İznik surlarının göl tarafındaki taş temellerinde yaşanan çökmeler ve nöbetçi değişim saatleri.",
                IntelType.FortressWeakness, "İznik", FactionType.Ottoman, 35, 350, 25, 4f);

            CreateIntelAsset(folderPath, "Intel_GermiyanAkinPlani",
                "Germiyan Atlılarının Teftiş Güzergâhı",
                "Kütahya beyliğinin sınır boyundaki deve kervanlarından aldığı haraç noktalarının listesi.",
                IntelType.RivalPlan, "Domaniç", FactionType.Ottoman, 15, 150, 10, 1f);

            CreateIntelAsset(folderPath, "Intel_BursaIpekKacakciligi",
                "Bursa Sarayının Gizli İpek Deposu",
                "Tekfurun gümrük vergisinden kaçırdığı birinci kalite ham ipek stoklarının tutulduğu kiler.",
                IntelType.TradeSecret, "Bursa", FactionType.Ottoman, 30, 280, 20, 3f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Kervan] 4 adet tarihsel istihbarat verisi (IntelDataSO) başarıyla oluşturuldu.");
        }

        private static void CreateIntelAsset(string folder, string fileName, string title, string desc, IntelType type, string sourceCity, FactionType targetFaction, int risk, int rewardAkce, int rewardRep, float taxDiscount)
        {
            string path = $"{folder}/{fileName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<IntelDataSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<IntelDataSO>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.Init(fileName, title, desc, type, sourceCity, targetFaction, risk, rewardAkce, rewardRep, taxDiscount);
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
