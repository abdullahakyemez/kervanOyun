using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core.Save;
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

        private string _currentNodeId = DefaultStartNodeId;
        private int _dayCount = 1;
        private CaravanData _caravan = new CaravanData();
        private ActiveTravelState _activeTravel;
        private readonly HashSet<string> _visitedNodeIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<InventorySlotSaveData> _pendingSavedItems = new List<InventorySlotSaveData>();
        private bool _hasSeenIntro;
        private bool _isSessionActive;

        public string CurrentNodeId => _currentNodeId;
        public int DayCount => _dayCount;
        public CaravanData Caravan => _caravan;
        public ActiveTravelState ActiveTravel => _activeTravel;
        public IReadOnlyCollection<string> VisitedNodeIds => _visitedNodeIds;

        /// <summary>
        /// Oyuncunun Yeni Oyun veya Devam Et ile aktif bir oturum başlatıp başlatmadığını belirtir.
        /// Ana menüde henüz seçim yapılmamışken mevcut disk kaydının ezilmesini önler.
        /// </summary>
        public bool IsSessionActive => _isSessionActive;

        public bool HasSeenIntro
        {
            get => _hasSeenIntro;
            set => _hasSeenIntro = value;
        }

        public GameSession()
        {
            _currentNodeId = DefaultStartNodeId;
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
            _hasSeenIntro = false;
            _isSessionActive = true;
            _visitedNodeIds.Clear();
            _visitedNodeIds.Add(_currentNodeId);
            _pendingSavedItems.Clear();

            ClearLegacyPlayerPrefs();

            EventBus.Publish(new LocationChangedEvent(previousNodeId, _currentNodeId));
            Debug.Log($"[GameSession] Yeni oyun oturumu başlatıldı. Başlangıç düğümü: {_currentNodeId}, Gün: {_dayCount}");
        }

        /// <summary>
        /// Editörde doğrudan WorldMapScene'den Play'e basıldığında oturum henüz aktif değilse
        /// varsa diskteki kaydı yükler ve oturumu aktif olarak işaretler.
        /// </summary>
        public void SyncFromLegacyPrefsIfChanged()
        {
            if (_isSessionActive) return;

            if (Services.TryGet<SaveService>(out var saveService) && saveService.HasSaveFile())
            {
                if (saveService.LoadIntoSession(this))
                {
                    return;
                }
            }

            _isSessionActive = true;
        }

        /// <summary>
        /// Çalışma anı oyun durumunu JSON serileştirme için <see cref="SaveData"/> nesnesine dönüştürür.
        /// </summary>
        public SaveData ToSaveData()
        {
            var data = new SaveData
            {
                saveVersion = SaveData.CurrentVersion,
                currentNodeId = string.IsNullOrWhiteSpace(_currentNodeId) ? DefaultStartNodeId : _currentNodeId,
                dayCount = Mathf.Max(1, _dayCount),
                hasSeenIntro = _hasSeenIntro,
                visitedNodeIds = new List<string>(_visitedNodeIds)
            };

            if (_caravan != null)
            {
                data.caravan.currentAkce = _caravan.CurrentAkce;
                data.caravan.currentFood = _caravan.CurrentFood;
                data.caravan.maxWeightCapacity = _caravan.MaxWeightCapacity;
                data.caravan.horses = _caravan.Horses;
                data.caravan.mules = _caravan.Mules;
                data.caravan.camels = _caravan.Camels;

                if (_caravan.Inventory != null && _caravan.Inventory.Count > 0)
                {
                    for (int i = 0; i < _caravan.Inventory.Count; i++)
                        {
                        var slot = _caravan.Inventory[i];
                        if (slot == null || slot.Item == null || string.IsNullOrWhiteSpace(slot.Item.ItemId)) continue;

                        data.caravan.items.Add(new InventorySlotSaveData
                        {
                            itemId = slot.Item.ItemId,
                            quantity = slot.Quantity,
                            averagePurchasePrice = slot.AveragePurchasePrice
                        });
                    }
                }
                else if (_pendingSavedItems.Count > 0)
                {
                    for (int i = 0; i < _pendingSavedItems.Count; i++)
                    {
                        var pending = _pendingSavedItems[i];
                        if (pending == null || string.IsNullOrWhiteSpace(pending.itemId) || pending.quantity <= 0) continue;

                        data.caravan.items.Add(new InventorySlotSaveData
                        {
                            itemId = pending.itemId,
                            quantity = pending.quantity,
                            averagePurchasePrice = pending.averagePurchasePrice
                        });
                    }
                }

                if (_caravan.Guards != null)
                {
                    for (int i = 0; i < _caravan.Guards.Count; i++)
                    {
                        var guard = _caravan.Guards[i];
                        if (guard == null) continue;

                        data.caravan.guards.Add(new GuardSaveData
                        {
                            guardId = guard.GuardId ?? string.Empty,
                            unitDataId = guard.UnitData != null ? guard.UnitData.UnitId : string.Empty,
                            currentHealth = guard.CurrentHealth,
                            currentMorale = guard.CurrentMorale
                        });
                    }
                }
            }

            if (_activeTravel != null)
            {
                data.activeTravel.isActive = true;
                data.activeTravel.routeId = _activeTravel.RouteId ?? string.Empty;
                data.activeTravel.fromNodeId = _activeTravel.FromNodeId ?? string.Empty;
                data.activeTravel.toNodeId = _activeTravel.ToNodeId ?? string.Empty;
                data.activeTravel.daysElapsed = _activeTravel.DaysElapsed;
                data.activeTravel.totalDays = _activeTravel.TotalDays;
            }
            else
            {
                data.activeTravel.isActive = false;
            }

            return data;
        }

        /// <summary>
        /// Diskten okunan <see cref="SaveData"/> verisini çalışma anı oturumuna uygular.
        /// </summary>
        public void ApplySaveData(SaveData data)
        {
            if (data == null) return;

            string previousNodeId = _currentNodeId;
            _currentNodeId = string.IsNullOrWhiteSpace(data.currentNodeId) ? DefaultStartNodeId : data.currentNodeId;
            _dayCount = Mathf.Max(1, data.dayCount);
            _hasSeenIntro = data.hasSeenIntro;
            _isSessionActive = true;

            _visitedNodeIds.Clear();
            if (data.visitedNodeIds != null)
            {
                for (int i = 0; i < data.visitedNodeIds.Count; i++)
                {
                    string nodeId = data.visitedNodeIds[i];
                    if (!string.IsNullOrWhiteSpace(nodeId))
                    {
                        _visitedNodeIds.Add(nodeId);
                    }
                }
            }
            _visitedNodeIds.Add(_currentNodeId);

            if (_caravan == null)
            {
                _caravan = new CaravanData();
            }

            if (data.caravan != null)
            {
                _caravan.RestoreFromSave(
                    data.caravan.currentAkce,
                    data.caravan.currentFood,
                    data.caravan.maxWeightCapacity,
                    data.caravan.horses,
                    data.caravan.mules,
                    data.caravan.camels
                );

                _caravan.ClearInventory();
                _pendingSavedItems.Clear();
                if (data.caravan.items != null)
                {
                    for (int i = 0; i < data.caravan.items.Count; i++)
                    {
                        var itemSave = data.caravan.items[i];
                        if (itemSave == null || string.IsNullOrWhiteSpace(itemSave.itemId) || itemSave.quantity <= 0) continue;

                        _pendingSavedItems.Add(new InventorySlotSaveData
                        {
                            itemId = itemSave.itemId,
                            quantity = itemSave.quantity,
                            averagePurchasePrice = itemSave.averagePurchasePrice
                        });
                    }
                }
            }

            if (data.activeTravel != null && data.activeTravel.isActive && !string.IsNullOrWhiteSpace(data.activeTravel.toNodeId))
            {
                _activeTravel = new ActiveTravelState(
                    data.activeTravel.fromNodeId,
                    data.activeTravel.toNodeId,
                    data.activeTravel.totalDays,
                    data.activeTravel.routeId
                )
                {
                    DaysElapsed = Mathf.Max(0, data.activeTravel.daysElapsed)
                };
            }
            else
            {
                _activeTravel = null;
            }

            if (!string.Equals(previousNodeId, _currentNodeId, StringComparison.Ordinal))
            {
                EventBus.Publish(new LocationChangedEvent(previousNodeId, _currentNodeId));
            }
        }

        /// <summary>
        /// Diskten yüklenen envanter kayıtlarını (<see cref="InventorySlotSaveData"/>) verilen <see cref="ItemDataSO"/> kataloğu
        /// üzerinden çözümleyerek <see cref="CaravanData.Inventory"/> listesine aktarır.
        /// </summary>
        public void ResolveSavedInventoryItems(IEnumerable<ItemDataSO> itemCatalog)
        {
            if (_pendingSavedItems.Count == 0 || itemCatalog == null || _caravan == null) return;

            var catalogById = new Dictionary<string, ItemDataSO>(StringComparer.Ordinal);
            foreach (var item in itemCatalog)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.ItemId) && !catalogById.ContainsKey(item.ItemId))
                {
                    catalogById.Add(item.ItemId, item);
                }
            }

            if (catalogById.Count == 0) return;

            var unresolved = new List<InventorySlotSaveData>();
            for (int i = 0; i < _pendingSavedItems.Count; i++)
            {
                var savedSlot = _pendingSavedItems[i];
                if (savedSlot == null || string.IsNullOrWhiteSpace(savedSlot.itemId) || savedSlot.quantity <= 0) continue;

                if (catalogById.TryGetValue(savedSlot.itemId, out var itemSo))
                {
                    _caravan.RestoreInventorySlot(itemSo, savedSlot.quantity, savedSlot.averagePurchasePrice);
                }
                else
                {
                    unresolved.Add(savedSlot);
                }
            }

            _pendingSavedItems.Clear();
            _pendingSavedItems.AddRange(unresolved);
        }

        /// <summary>
        /// Oyuncunun bulunduğu düğümü günceller ve keşfedilen düğümlere ekler.
        /// </summary>
        public void SetCurrentNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) return;

            string previousNodeId = _currentNodeId;
            _currentNodeId = nodeId;
            _isSessionActive = true;
            _visitedNodeIds.Add(_currentNodeId);

            if (!string.Equals(previousNodeId, _currentNodeId, StringComparison.Ordinal))
            {
                EventBus.Publish(new LocationChangedEvent(previousNodeId, _currentNodeId));
            }
        }

        /// <summary>
        /// Hedef düğüme yeni bir seyahat başlatır. Konum (<see cref="CurrentNodeId"/>) yola çıkarken değişmez;
        /// yalnızca varışta (<see cref="CompleteTravel"/>) güncellenir.
        /// </summary>
        public void StartTravel(string toNodeId, int estimatedDays, string routeId = "")
        {
            if (string.IsNullOrWhiteSpace(toNodeId)) return;

            _isSessionActive = true;
            _activeTravel = new ActiveTravelState(_currentNodeId, toNodeId, estimatedDays, routeId);

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

            _isSessionActive = true;
            _dayCount += days;
            if (_activeTravel != null)
            {
                _activeTravel.DaysElapsed += days;
            }

            EventBus.Publish(new DayPassedEvent(_dayCount, days));
        }

        private static void ClearLegacyPlayerPrefs()
        {
            if (PlayerPrefs.HasKey("HasSavedGame")) PlayerPrefs.DeleteKey("HasSavedGame");
            if (PlayerPrefs.HasKey("CurrentNodeId")) PlayerPrefs.DeleteKey("CurrentNodeId");
            if (PlayerPrefs.HasKey("TravelDestinationNodeId")) PlayerPrefs.DeleteKey("TravelDestinationNodeId");
            if (PlayerPrefs.HasKey("HasSeenIntro_v1")) PlayerPrefs.DeleteKey("HasSeenIntro_v1");
            PlayerPrefs.Save();
        }
    }
}
