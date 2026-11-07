using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;
using Kervan.TradeAndTravel;

namespace Kervan.IntelSystem
{
    /// <summary>
    /// Bir şehirde istihbarat toplama girişimi sonuçlandığında yayınlanır.
    /// </summary>
    public readonly struct IntelGatheredEvent : IGameEvent
    {
        public readonly IntelDataSO Intel;
        public readonly bool Succeeded;
        public readonly string Message;

        public IntelGatheredEvent(IntelDataSO intel, bool succeeded, string message)
        {
            Intel = intel;
            Succeeded = succeeded;
            Message = message;
        }
    }

    /// <summary>
    /// Toplanmış bir istihbarat beylik divanına teslim edildiğinde yayınlanır.
    /// </summary>
    public readonly struct IntelSubmittedEvent : IGameEvent
    {
        public readonly IntelDataSO Intel;
        public readonly int RewardAkce;
        public readonly int RewardReputation;

        public IntelSubmittedEvent(IntelDataSO intel, int akce, int rep)
        {
            Intel = intel;
            RewardAkce = akce;
            RewardReputation = rep;
        }
    }

    /// <summary>
    /// Şehirlerdeki istihbarat parçacıklarını toplama, casusluk zarı ve beylik divanına teslimat motoru (saf C# servisi).
    /// </summary>
    public class QuestManager
    {
        private const int DiscoveryBribeCapAkce = 50;
        private const int DiscoveryReputationPenalty = -5;

        /// <summary>
        /// Servis bulucu (<see cref="Services"/>) üzerinden aktif <see cref="QuestManager"/> örneğine erişim sağlar.
        /// </summary>
        public static QuestManager Instance =>
            Services.TryGet<QuestManager>(out var manager) ? manager : null;

        private CaravanData _caravanData;
        private readonly List<IntelDataSO> _collectedIntels = new List<IntelDataSO>();
        private readonly HashSet<string> _collectedIntelIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _submittedIntelIds = new HashSet<string>(StringComparer.Ordinal);

        public IReadOnlyList<IntelDataSO> CollectedIntels => _collectedIntels;
        public IReadOnlyCollection<string> CollectedIntelIds => _collectedIntelIds;
        public IReadOnlyCollection<string> SubmittedIntelIds => _submittedIntelIds;

        /// <summary>
        /// Tüm istihbarat kayıtlarını yeni oyun başlangıç durumuna sıfırlar.
        /// </summary>
        public void ResetToDefaults()
        {
            _collectedIntels.Clear();
            _collectedIntelIds.Clear();
            _submittedIntelIds.Clear();
            _caravanData = null;
        }

        private CaravanData ResolveCaravanData()
        {
            if (Services.TryGet<GameSession>(out var session) && session.Caravan != null)
            {
                _caravanData = session.Caravan;
                return _caravanData;
            }

            if (_caravanData == null && CaravanManager.Instance != null)
            {
                _caravanData = CaravanManager.Instance.Caravan;
            }

            return _caravanData;
        }

        public void SetCaravanData(CaravanData caravanData)
        {
            _caravanData = caravanData;
        }

        /// <summary>
        /// Belirtilen istihbarat kimliğinin şu an kervanın elinde (toplanmış ama henüz teslim edilmemiş) olup olmadığını döndürür.
        /// </summary>
        public bool HasCollectedIntel(string intelId)
        {
            if (string.IsNullOrWhiteSpace(intelId)) return false;
            return _collectedIntelIds.Contains(intelId);
        }

        /// <summary>
        /// Belirtilen istihbarat kimliğinin daha önce divana teslim edilip edilmediğini döndürür.
        /// </summary>
        public bool HasSubmittedIntel(string intelId)
        {
            if (string.IsNullOrWhiteSpace(intelId)) return false;
            return _submittedIntelIds.Contains(intelId);
        }

        /// <summary>
        /// Şehir pazarında veya kervansarayında fısıltı dinleyerek istihbarat toplamaya çalışır.
        /// </summary>
        public bool TryGatherIntel(IntelDataSO intel, out string resultMessage)
        {
            if (intel == null || string.IsNullOrWhiteSpace(intel.IntelId))
            {
                resultMessage = "Geçersiz istihbarat kaydı.";
                return false;
            }

            var caravan = ResolveCaravanData();

            if (_submittedIntelIds.Contains(intel.IntelId))
            {
                resultMessage = "Bu istihbaratı zaten beylik divanına teslim ettiniz!";
                EventBus.Publish(new IntelGatheredEvent(intel, false, resultMessage));
                return false;
            }

            if (_collectedIntelIds.Contains(intel.IntelId))
            {
                if (!_collectedIntels.Contains(intel))
                {
                    _collectedIntels.Add(intel);
                }
                resultMessage = "Bu havadisi zaten daha önce topladınız!";
                EventBus.Publish(new IntelGatheredEvent(intel, false, resultMessage));
                return false;
            }

            // Casusluk zarı (0 - 100)
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < intel.DiscoveryRisk)
            {
                // Yakalanma / ifşa olma tehlikesi!
                int currentGold = caravan != null ? caravan.CurrentAkce : DiscoveryBribeCapAkce;
                int bribe = Mathf.Min(DiscoveryBribeCapAkce, currentGold);
                if (caravan != null && bribe > 0)
                {
                    caravan.ModifyAkce(-bribe);
                }

                if (Services.TryGet<ReputationManager>(out var repManager))
                {
                    repManager.AddReputation(intel.TargetFaction, DiscoveryReputationPenalty);
                }

                resultMessage = $"[Casusluk İfşası!] Şehir muhafızları sizi fark etti. Muhafızlara {bribe} Akçe rüşvet verilerek can zor kurtarıldı!";
                EventBus.Publish(new IntelGatheredEvent(intel, false, resultMessage));
                return false;
            }

            _collectedIntelIds.Add(intel.IntelId);
            if (!_collectedIntels.Contains(intel))
            {
                _collectedIntels.Add(intel);
            }

            resultMessage = $"[İstihbarat Toplandı!] '{intel.Title}' havadisi kervan seyir defterine kaydedildi.";
            EventBus.Publish(new IntelGatheredEvent(intel, true, resultMessage));
            Debug.Log($"[QuestManager] {resultMessage}");
            return true;
        }

        /// <summary>
        /// Toplanan istihbaratı ilgili beylik divanına (Örn: Söğüt Beylik Divanı) teslim eder.
        /// </summary>
        public bool SubmitIntel(IntelDataSO intel, out string rewardSummary)
        {
            if (intel == null || string.IsNullOrWhiteSpace(intel.IntelId))
            {
                rewardSummary = "Geçersiz istihbarat kaydı.";
                return false;
            }

            var caravan = ResolveCaravanData();

            if (!_collectedIntelIds.Contains(intel.IntelId))
            {
                rewardSummary = "Bu istihbarat elinizde bulunmuyor!";
                return false;
            }

            _collectedIntelIds.Remove(intel.IntelId);
            _collectedIntels.RemoveAll(x => x != null && string.Equals(x.IntelId, intel.IntelId, StringComparison.Ordinal));
            _submittedIntelIds.Add(intel.IntelId);

            // 1. Akçe Ödülü
            if (caravan != null && intel.RewardAkce > 0)
            {
                caravan.ModifyAkce(intel.RewardAkce);
            }

            // 2. Beylik İtibar Ödülü
            if (Services.TryGet<ReputationManager>(out var repManager))
            {
                repManager.AddReputation(intel.TargetFaction, intel.RewardReputation);
            }

            rewardSummary = $"Havadis beylik divanına arz edildi! Kazanılan: {intel.RewardAkce} Akçe, +{intel.RewardReputation} Beylik Nüfuzu.";
            EventBus.Publish(new IntelSubmittedEvent(intel, intel.RewardAkce, intel.RewardReputation));

            Debug.Log($"[QuestManager] {rewardSummary}");
            return true;
        }

        /// <summary>
        /// Diskten yüklenen istihbarat kimliklerini verilen <see cref="IntelDataSO"/> kataloğu ile eşleştirerek
        /// çalışma anı <see cref="CollectedIntels"/> listesini doldurur.
        /// </summary>
        public void ResolveSavedIntels(IEnumerable<IntelDataSO> intelCatalog)
        {
            if (intelCatalog == null || _collectedIntelIds.Count == 0) return;

            foreach (var intel in intelCatalog)
            {
                if (intel == null || string.IsNullOrWhiteSpace(intel.IntelId)) continue;
                if (!_collectedIntelIds.Contains(intel.IntelId)) continue;

                bool alreadyInList = false;
                for (int i = 0; i < _collectedIntels.Count; i++)
                {
                    if (_collectedIntels[i] != null &&
                        string.Equals(_collectedIntels[i].IntelId, intel.IntelId, StringComparison.Ordinal))
                    {
                        alreadyInList = true;
                        break;
                    }
                }

                if (!alreadyInList)
                {
                    _collectedIntels.Add(intel);
                }
            }
        }

        public List<string> GetCollectedIntelIdsForSave()
        {
            return new List<string>(_collectedIntelIds);
        }

        public List<string> GetSubmittedIntelIdsForSave()
        {
            return new List<string>(_submittedIntelIds);
        }

        public void ApplySaveData(List<string> collectedIds, List<string> submittedIds)
        {
            _collectedIntels.Clear();
            _collectedIntelIds.Clear();
            _submittedIntelIds.Clear();

            if (submittedIds != null)
            {
                for (int i = 0; i < submittedIds.Count; i++)
                {
                    string id = submittedIds[i];
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        _submittedIntelIds.Add(id);
                    }
                }
            }

            if (collectedIds != null)
            {
                for (int i = 0; i < collectedIds.Count; i++)
                {
                    string id = collectedIds[i];
                    if (!string.IsNullOrWhiteSpace(id) && !_submittedIntelIds.Contains(id))
                    {
                        _collectedIntelIds.Add(id);
                    }
                }
            }
        }
    }
}
