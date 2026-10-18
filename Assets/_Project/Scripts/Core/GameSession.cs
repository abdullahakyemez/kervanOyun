using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.TradeAndTravel;

namespace Kervan.Core
{
    /// <summary>
    /// Devam eden bir seyahatin çalışma anı durumunu temsil eder.
    /// </summary>
    [Serializable]
    public class ActiveTravelState
    {
        public string RouteId;
        public string FromNodeId;
        public string ToNodeId;
        public int DaysElapsed;
        public int TotalDays;

        public ActiveTravelState(string fromNodeId, string toNodeId, int totalDays, string routeId = "")
        {
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            TotalDays = Mathf.Max(1, totalDays);
            DaysElapsed = 0;
            RouteId = routeId ?? string.Empty;
        }
    }

    #region Oturum Durum Olayları (GameSession Events)

    /// <summary>
    /// Oyuncunun bulunduğu düğüm (şehir/han) değiştiğinde yayınlanır.
    /// </summary>
    public readonly struct LocationChangedEvent : IGameEvent
    {
        public readonly string PreviousNodeId;
        public readonly string CurrentNodeId;

        public LocationChangedEvent(string previousNodeId, string currentNodeId)
        {
            PreviousNodeId = previousNodeId;
            CurrentNodeId = currentNodeId;
        }
    }

    /// <summary>
    /// Oyun içi takvimde gün ilerlediğinde yayınlanır.
    /// </summary>
    public readonly struct DayPassedEvent : IGameEvent
    {
        public readonly int CurrentDayCount;
        public readonly int DaysAdded;

        public DayPassedEvent(int currentDayCount, int daysAdded)
        {
            CurrentDayCount = currentDayCount;
            DaysAdded = daysAdded;
        }
    }

    /// <summary>
    /// Kervan yeni bir rotada seyahate başladığında yayınlanır.
    /// </summary>
    public readonly struct TravelStartedEvent : IGameEvent
    {
        public readonly string FromNodeId;
        public readonly string ToNodeId;
        public readonly int EstimatedDays;

        public TravelStartedEvent(string fromNodeId, string toNodeId, int estimatedDays)
        {
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            EstimatedDays = estimatedDays;
        }
    }

    #endregion

    /// <summary>
    /// Sahneler arası tek doğruluk kaynağı (Single Source of Truth) olan çalışma anı oyun durumu.
    /// Kervan kaynakları, mevcut düğüm kimliği, gün sayısı, aktif seyahat ve keşfedilen noktaları tutar.
    /// </summary>
    public class GameSession
    {
        public const string DefaultStartNodeId = "node_sogut";
        private const string LegacyPrefCurrentNodeId = "CurrentNodeId";
        private const string LegacyPrefTravelDestNodeId = "TravelDestinationNodeId";

        private string _currentNodeId = DefaultStartNodeId;
        private int _dayCount = 1;
        private CaravanData _caravan = new CaravanData();
        private ActiveTravelState _activeTravel;
        private readonly HashSet<string> _visitedNodeIds = new HashSet<string>(StringComparer.Ordinal);
        private bool _hasSeenIntro;

        public string CurrentNodeId => _currentNodeId;
        public int DayCount => _dayCount;
        public CaravanData Caravan => _caravan;
        public ActiveTravelState ActiveTravel => _activeTravel;
        public IReadOnlyCollection<string> VisitedNodeIds => _visitedNodeIds;
        public bool HasSeenIntro
        {
            get => _hasSeenIntro;
            set => _hasSeenIntro = value;
        }

        public GameSession()
        {
            // A2.3'te JSON SaveService gelene kadar mevcut PlayerPrefs bayrağıyla senkronize başla
            string savedNodeId = PlayerPrefs.GetString(LegacyPrefCurrentNodeId, DefaultStartNodeId);
            if (string.IsNullOrWhiteSpace(savedNodeId))
            {
                savedNodeId = DefaultStartNodeId;
            }

            _currentNodeId = savedNodeId;
            _visitedNodeIds.Add(_currentNodeId);
        }

        /// <summary>
        /// Yeni oyun başlatıldığında tüm oturum verilerini başlangıç değerlerine sıfırlar.
        /// </summary>
        public void StartNewGame(string startNodeId = DefaultStartNodeId)
        {
            string resolvedStart = string.IsNullOrWhiteSpace(startNodeId) ? DefaultStartNodeId : startNodeId;
            string previousNodeId = _currentNodeId;

            _currentNodeId = resolvedStart;
            _dayCount = 1;
            _caravan = new CaravanData();
            _activeTravel = null;
            _visitedNodeIds.Clear();
            _visitedNodeIds.Add(_currentNodeId);

            SyncLegacyPlayerPrefs();

            EventBus.Publish(new LocationChangedEvent(previousNodeId, _currentNodeId));
            Debug.Log($"[GameSession] Yeni oyun oturumu başlatıldı. Başlangıç düğümü: {_currentNodeId}, Gün: {_dayCount}");
        }

        /// <summary>
        /// A2.3'e kadar MainMenuUI'ın PlayerPrefs üzerinden yaptığı sıfırlamaları yakalayıp senkronize eder.
        /// </summary>
        public void SyncFromLegacyPrefsIfChanged()
        {
            string prefNodeId = PlayerPrefs.GetString(LegacyPrefCurrentNodeId, DefaultStartNodeId);
            if (!string.IsNullOrWhiteSpace(prefNodeId) && !string.Equals(prefNodeId, _currentNodeId, StringComparison.Ordinal))
            {
                SetCurrentNode(prefNodeId);
            }
        }

        /// <summary>
        /// Oyuncunun bulunduğu düğümü günceller ve keşfedilen düğümlere ekler.
        /// </summary>
        public void SetCurrentNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) return;

            string previousNodeId = _currentNodeId;
            _currentNodeId = nodeId;
            _visitedNodeIds.Add(_currentNodeId);

            SyncLegacyPlayerPrefs();

            if (!string.Equals(previousNodeId, _currentNodeId, StringComparison.Ordinal))
            {
                EventBus.Publish(new LocationChangedEvent(previousNodeId, _currentNodeId));
            }
        }

        /// <summary>
        /// Hedef düğüme yeni bir seyahat başlatır.
        /// </summary>
        public void StartTravel(string toNodeId, int estimatedDays, string routeId = "")
        {
            if (string.IsNullOrWhiteSpace(toNodeId)) return;

            _activeTravel = new ActiveTravelState(_currentNodeId, toNodeId, estimatedDays, routeId);
            PlayerPrefs.SetString(LegacyPrefTravelDestNodeId, toNodeId);

            // Not: A2.4'e kadar mevcut akışın kırılmaması için konum güncellenir;
            // A2.4/A3.4'te konum yalnızca varışta (CompleteTravel) güncellenecektir.
            SetCurrentNode(toNodeId);

            EventBus.Publish(new TravelStartedEvent(_activeTravel.FromNodeId, toNodeId, estimatedDays));
        }

        /// <summary>
        /// Aktif seyahati tamamlar ve oyuncunun konumunu hedef düğüme taşır.
        /// </summary>
        public void CompleteTravel()
        {
            if (_activeTravel == null) return;

            string targetNodeId = _activeTravel.ToNodeId;
            _activeTravel = null;
            SetCurrentNode(targetNodeId);
        }

        /// <summary>
        /// Oyun gününü belirtilen gün kadar ilerletir ve olay yayınlar.
        /// </summary>
        public void AdvanceDays(int days = 1)
        {
            if (days <= 0) return;

            _dayCount += days;
            if (_activeTravel != null)
            {
                _activeTravel.DaysElapsed += days;
            }

            EventBus.Publish(new DayPassedEvent(_dayCount, days));
        }

        private void SyncLegacyPlayerPrefs()
        {
            PlayerPrefs.SetString(LegacyPrefCurrentNodeId, _currentNodeId);
            PlayerPrefs.Save();
        }
    }
}
