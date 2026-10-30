using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Kervan.Core.Save
{
    /// <summary>
    /// Oyun durumu diske başarıyla kaydedildiğinde yayınlanan olay.
    /// </summary>
    public readonly struct GameSavedEvent : IGameEvent
    {
        public readonly string FilePath;
        public readonly string SavedAtIso;

        public GameSavedEvent(string filePath, string savedAtIso)
        {
            FilePath = filePath;
            SavedAtIso = savedAtIso;
        }
    }

    /// <summary>
    /// <see cref="GameSession"/> verisini <c>Application.persistentDataPath/save_0.json</c> dosyasına
    /// atomik olarak (<c>.tmp</c> yazıp yeniden adlandırarak) kaydeden ve yükleyen servis.
    /// </summary>
    public class SaveService
    {
        public const string DefaultSaveFileName = "save_0.json";
        private const string TempExtension = ".tmp";
        private const string BackupExtension = ".bak";

        private readonly string _saveDirectory;
        private readonly string _saveFilePath;
        private readonly string _tempFilePath;
        private readonly string _backupFilePath;

        public string SaveFilePath => _saveFilePath;

        public SaveService(string customDirectory = null)
        {
            _saveDirectory = string.IsNullOrWhiteSpace(customDirectory)
                ? Application.persistentDataPath
                : customDirectory;

            _saveFilePath = Path.Combine(_saveDirectory, DefaultSaveFileName);
            _tempFilePath = _saveFilePath + TempExtension;
            _backupFilePath = _saveFilePath + BackupExtension;
        }

        /// <summary>
        /// Disk üzerinde kayıt dosyasının var olup olmadığını döndürür.
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(_saveFilePath);
        }

        /// <summary>
        /// Verilen <see cref="GameSession"/> durumunu atomik olarak diske kaydeder.
        /// </summary>
        public bool SaveSession(GameSession session)
        {
            if (session == null)
            {
                Debug.LogError("[SaveService] Kaydedilecek GameSession referansı null!");
                return false;
            }

            SaveData data = session.ToSaveData();
            return Save(data);
        }

        /// <summary>
        /// <see cref="SaveData"/> nesnesini önce <c>.tmp</c> dosyasına yazar, ardından hedef dosyayla atomik olarak değiştirir.
        /// </summary>
        public bool Save(SaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[SaveService] Kaydedilecek SaveData null!");
                return false;
            }

            try
            {
                if (!Directory.Exists(_saveDirectory))
                {
                    Directory.CreateDirectory(_saveDirectory);
                }

                data.saveVersion = SaveData.CurrentVersion;
                data.savedAtIso = DateTime.UtcNow.ToString("O");

                string json = JsonUtility.ToJson(data, true);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogError("[SaveService] JSON serileştirme sonucu boş döndü, kayıt iptal edildi.");
                    return false;
                }

                File.WriteAllText(_tempFilePath, json, Encoding.UTF8);

                if (File.Exists(_saveFilePath))
                {
                    if (File.Exists(_backupFilePath))
                    {
                        File.Delete(_backupFilePath);
                    }
                    File.Move(_saveFilePath, _backupFilePath);
                }

                File.Move(_tempFilePath, _saveFilePath);

                EventBus.Publish(new GameSavedEvent(_saveFilePath, data.savedAtIso));
                Debug.Log($"[SaveService] Oyun kaydedildi: {_saveFilePath} (Konum: {data.currentNodeId}, Gün: {data.dayCount})");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveService] Kayıt sırasında hata oluştu: {ex.Message}");
                TryCleanupTempFile();
                return false;
            }
        }

        /// <summary>
        /// Diskteki kaydı okuyup verilen <see cref="GameSession"/> üzerine uygular.
        /// Dosya yoksa veya bozuksa çökmeden <c>false</c> döner.
        /// </summary>
        public bool LoadIntoSession(GameSession session)
        {
            if (session == null)
            {
                Debug.LogError("[SaveService] Yüklenecek GameSession referansı null!");
                return false;
            }

            if (!TryLoad(out SaveData data))
            {
                return false;
            }

            session.ApplySaveData(data);
            Debug.Log($"[SaveService] Kayıt başarıyla GameSession'a yüklendi. Konum: {session.CurrentNodeId}, Gün: {session.DayCount}, Akçe: {session.Caravan.CurrentAkce}");
            return true;
        }

        /// <summary>
        /// Diskteki JSON kayıt dosyasını güvenli biçimde okur ve doğrular.
        /// Bozuk JSON veya geçersiz içerikte istisna fırlatmaz, uyarı loglayıp <c>false</c> döner.
        /// </summary>
        public bool TryLoad(out SaveData data)
        {
            data = null;

            if (!File.Exists(_saveFilePath))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(_saveFilePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Debug.LogWarning("[SaveService] Kayıt dosyası boş! Bozuk kayıt yoksayılıyor.");
                    return false;
                }

                SaveData parsed = JsonUtility.FromJson<SaveData>(json);
                if (!ValidateAndMigrate(parsed))
                {
                    Debug.LogWarning("[SaveService] Kayıt dosyası doğrulamadan geçemedi veya sürümü uyumsuz.");
                    return false;
                }

                data = parsed;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveService] Kayıt dosyası okunamadı veya bozuk ({_saveFilePath}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Mevcut kayıt dosyasını ve yedeğini siler.
        /// </summary>
        public void DeleteSaveFile()
        {
            try
            {
                if (File.Exists(_saveFilePath)) File.Delete(_saveFilePath);
                if (File.Exists(_tempFilePath)) File.Delete(_tempFilePath);
                if (File.Exists(_backupFilePath)) File.Delete(_backupFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveService] Kayıt dosyası silinirken uyarı: {ex.Message}");
            }
        }

        private bool ValidateAndMigrate(SaveData data)
        {
            if (data == null) return false;
            if (data.saveVersion <= 0 || data.saveVersion > SaveData.CurrentVersion) return false;
            if (string.IsNullOrWhiteSpace(data.currentNodeId)) return false;

            if (data.dayCount < 1)
            {
                data.dayCount = 1;
            }

            if (data.caravan == null)
            {
                data.caravan = new CaravanSaveData();
            }

            if (data.visitedNodeIds == null)
            {
                data.visitedNodeIds = new System.Collections.Generic.List<string>();
            }

            if (data.cityMarkets == null)
            {
                data.cityMarkets = new System.Collections.Generic.List<CityMarketNodeSaveData>();
            }

            return true;
        }

        private void TryCleanupTempFile()
        {
            try
            {
                if (File.Exists(_tempFilePath))
                {
                    File.Delete(_tempFilePath);
                }
            }
            catch
            {
                // Geçici dosya temizliği başarısız olursa sessizce geç
            }
        }
    }
}
