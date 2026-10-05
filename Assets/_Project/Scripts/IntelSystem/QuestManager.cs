using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;
using Kervan.TradeAndTravel;

namespace Kervan.IntelSystem
{
    public struct IntelGatheredEvent : IGameEvent
    {
        public IntelDataSO Intel;
        public bool Succeeded;
        public string Message;

        public IntelGatheredEvent(IntelDataSO intel, bool succeeded, string message)
        {
            Intel = intel;
            Succeeded = succeeded;
            Message = message;
        }
    }

    public struct IntelSubmittedEvent : IGameEvent
    {
        public IntelDataSO Intel;
        public int RewardAkce;
        public int RewardReputation;

        public IntelSubmittedEvent(IntelDataSO intel, int akce, int rep)
        {
            Intel = intel;
            RewardAkce = akce;
            RewardReputation = rep;
        }
    }

    /// <summary>
    /// Şehirlerdeki istihbarat parçacıklarını toplama, casusluk zarı ve beylik divanına teslimat motoru.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        private CaravanData _caravanData;

        [Header("Mevcut İstihbarat Envanteri")]
        [SerializeField] private List<IntelDataSO> _collectedIntels = new List<IntelDataSO>();

        public IReadOnlyList<IntelDataSO> CollectedIntels => _collectedIntels;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ResolveCaravanData();
        }

        private void ResolveCaravanData()
        {
            if (_caravanData == null && CaravanManager.Instance != null)
            {
                _caravanData = CaravanManager.Instance.Caravan;
            }
        }

        public void SetCaravanData(CaravanData caravanData)
        {
            _caravanData = caravanData;
        }

        /// <summary>
        /// Şehir pazarında veya meyhanesinde fısıltı dinleyerek istihbarat toplamaya çalışır.
        /// </summary>
        public bool TryGatherIntel(IntelDataSO intel, out string resultMessage)
        {
            ResolveCaravanData();

            if (_collectedIntels.Contains(intel))
            {
                resultMessage = "Bu havadisi zaten daha önce topladınız!";
                EventBus.Publish(new IntelGatheredEvent(intel, false, resultMessage));
                return false;
            }

            // Casusluk zarı (0 - 100)
            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < intel.DiscoveryRisk)
            {
                // Yakalanma / ifşa olma tehlikesi!
                int currentGold = _caravanData != null ? _caravanData.CurrentAkce : 50;
                int bribe = Mathf.Min(50, currentGold);
                if (_caravanData != null && bribe > 0)
                {
                    _caravanData.ModifyAkce(-bribe);
                }

                if (ReputationManager.Instance != null)
                {
                    // Şehrin beyliğiyle itibar düşer
                    ReputationManager.Instance.AddReputation(intel.TargetFaction, -5);
                }

                resultMessage = $"[Casusluk İfşası!] Şehir muhafızları sizi fark etti. Muhafızlara {bribe} Akçe rüşvet verilerek can zor kurtarıldı!";
                EventBus.Publish(new IntelGatheredEvent(intel, false, resultMessage));
                return false;
            }

            _collectedIntels.Add(intel);
            resultMessage = $"[İstihbarat Toplandı!] '{intel.Title}' havadisi kervan seyir defterine kaydedildi.";
            EventBus.Publish(new IntelGatheredEvent(intel, true, resultMessage));
            Debug.Log($"[Kervan-İstihbarat] {resultMessage}");
            return true;
        }

        /// <summary>
        /// Toplanan istihbaratı ilgili beylik divanına (Örn: Söğüt Beylik Çadırı) teslim eder.
        /// </summary>
        public bool SubmitIntel(IntelDataSO intel, out string rewardSummary)
        {
            ResolveCaravanData();

            if (!_collectedIntels.Contains(intel))
            {
                rewardSummary = "Bu istihbarat elinizde bulunmuyor!";
                return false;
            }

            _collectedIntels.Remove(intel);

            // 1. Akçe Ödülü
            if (_caravanData != null)
            {
                _caravanData.ModifyAkce(intel.RewardAkce);
            }

            // 2. Beylik İtibar Ödülü
            if (ReputationManager.Instance != null)
            {
                ReputationManager.Instance.AddReputation(intel.TargetFaction, intel.RewardReputation);
            }

            rewardSummary = $"Havadis beylik divanına arz edildi! Kazanılan: {intel.RewardAkce} Akçe, +{intel.RewardReputation} Beylik Nüfuzu.";
            EventBus.Publish(new IntelSubmittedEvent(intel, intel.RewardAkce, intel.RewardReputation));

            Debug.Log($"[Kervan-Divan] {rewardSummary}");
            return true;
        }
    }
}
