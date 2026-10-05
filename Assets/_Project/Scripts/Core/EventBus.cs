using System;
using System.Collections.Generic;

namespace Kervan.Core
{
    /// <summary>
    /// EventBus uzerinden yayinlanan tum olaylarin temel arayuzu.
    /// </summary>
    public interface IGameEvent { }

    /// <summary>
    /// Sistemlerin (Savas, Pazar, Seyahat, Arayuz) birbirine bagimli olmadan
    /// gevsek bagli (decoupled) haberlesmesini saglayan merkezi olay hatti.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> callback) where T : IGameEvent
        {
            if (callback == null) return;

            Type eventType = typeof(T);
            if (!_subscribers.TryGetValue(eventType, out var list))
            {
                list = new List<Delegate>();
                _subscribers[eventType] = list;
            }

            if (!list.Contains(callback))
            {
                list.Add(callback);
            }
        }

        public static void Unsubscribe<T>(Action<T> callback) where T : IGameEvent
        {
            if (callback == null) return;

            Type eventType = typeof(T);
            if (_subscribers.TryGetValue(eventType, out var list))
            {
                list.Remove(callback);
                if (list.Count == 0)
                {
                    _subscribers.Remove(eventType);
                }
            }
        }

        public static void Publish<T>(T eventData) where T : IGameEvent
        {
            Type eventType = typeof(T);
            if (_subscribers.TryGetValue(eventType, out var list))
            {
                // Iterasyon esnasinda unsubscribe ihtimaline karsi kopya uzerinden calisilir
                var snapshot = list.ToArray();
                for (int i = 0; i < snapshot.Length; i++)
                {
                    if (snapshot[i] is Action<T> action)
                    {
                        action.Invoke(eventData);
                    }
                }
            }
        }

        public static void ClearAll()
        {
            _subscribers.Clear();
        }
    }

    #region Temel Oyun Olaylari (Common Game Events)

    /// <summary>
    /// Savas turu degistiginde yayinlanir.
    /// </summary>
    public readonly struct CombatTurnChangedEvent : IGameEvent
    {
        public readonly CombatTurnPhase Turn;
        public readonly int RoundNumber;

        public CombatTurnChangedEvent(CombatTurnPhase turn, int roundNumber)
        {
            Turn = turn;
            RoundNumber = roundNumber;
        }
    }

    /// <summary>
    /// Kervanin temel istatistikleri (akce, erzak, yuk) degistiginde yayinlanir.
    /// </summary>
    public readonly struct CaravanStatsChangedEvent : IGameEvent
    {
        public readonly int CurrentAkce;
        public readonly int CurrentFood;
        public readonly float CurrentWeight;
        public readonly float MaxWeightCapacity;

        public CaravanStatsChangedEvent(int currentAkce, int currentFood, float currentWeight, float maxWeightCapacity)
        {
            CurrentAkce = currentAkce;
            CurrentFood = currentFood;
            CurrentWeight = currentWeight;
            MaxWeightCapacity = maxWeightCapacity;
        }
    }

    /// <summary>
    /// Sehir pazarinda alis veya satis gerceklestiginde yayinlanir.
    /// </summary>
    public readonly struct MarketTransactionEvent : IGameEvent
    {
        public readonly string ItemId;
        public readonly int Quantity;
        public readonly int UnitPrice;
        public readonly bool IsBuying; // True: Oyuncu aldi, False: Oyuncu satti

        public MarketTransactionEvent(string itemId, int quantity, int unitPrice, bool isBuying)
        {
            ItemId = itemId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            IsBuying = isBuying;
        }
    }

    /// <summary>
    /// Yeni bir istihbarat parcasi ele gecirildiginde yayinlanir.
    /// </summary>
    public readonly struct IntelAcquiredEvent : IGameEvent
    {
        public readonly string IntelId;
        public readonly string Title;
        public readonly FactionType TargetFaction;

        public IntelAcquiredEvent(string intelId, string title, FactionType targetFaction)
        {
            IntelId = intelId;
            Title = title;
            TargetFaction = targetFaction;
        }
    }

    #endregion
}
