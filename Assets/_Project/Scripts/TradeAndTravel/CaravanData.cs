using System;
using System.Collections.Generic;
using UnityEngine;
using Kervan.Core;
using Kervan.Combat;

namespace Kervan.TradeAndTravel
{
    [Serializable]
    public class CaravanItemSlot
    {
        public ItemDataSO Item;
        public int Quantity;
        public int AveragePurchasePrice;

        public CaravanItemSlot(ItemDataSO item, int quantity, int averagePurchasePrice)
        {
            Item = item;
            Quantity = quantity;
            AveragePurchasePrice = averagePurchasePrice;
        }

        public float TotalWeight => Item != null ? Item.UnitWeightKg * Quantity : 0f;
    }

    [Serializable]
    public class CaravanGuardRecord
    {
        public string GuardId;
        public UnitDataSO UnitData;
        public int CurrentHealth;
        public int CurrentMorale;
        public bool IsInjured => CurrentHealth < (UnitData != null ? UnitData.MaxHealth * 0.4f : 1);

        public CaravanGuardRecord(UnitDataSO unitData)
        {
            GuardId = Guid.NewGuid().ToString().Substring(0, 8);
            UnitData = unitData;
            CurrentHealth = unitData != null ? unitData.MaxHealth : 100;
            CurrentMorale = unitData != null ? unitData.BaseMorale : 100;
        }
    }

    /// <summary>
    /// Kervanın seyahat, ticaret ve savaş boyunca sahip olduğu canlı verileri
    /// (Akçe, Erzak, Envanter, Muhafızlar ve Yük) yöneten çekirdek veri modeli.
    /// </summary>
    [Serializable]
    public class CaravanData
    {
        [Header("Temel Kaynaklar")]
        [SerializeField] private int _currentAkce = 500;
        [SerializeField] private int _currentFood = 20; // 20 günlük temel erzak
        [SerializeField] private float _maxWeightCapacity = 300f; // 300 Kg temel yük kapasitesi

        [Header("Binek ve Yük Hayvanları")]
        [SerializeField] private int _horses = 2; // At
        [SerializeField] private int _mules = 2;  // Katır
        [SerializeField] private int _camels = 1; // Deve

        [Header("Kadro ve Envanter")]
        [SerializeField] private List<CaravanItemSlot> _inventory = new List<CaravanItemSlot>();
        [SerializeField] private List<CaravanGuardRecord> _guards = new List<CaravanGuardRecord>();

        // Getters
        public int CurrentAkce => _currentAkce;
        public int CurrentFood => _currentFood;
        public float MaxWeightCapacity => _maxWeightCapacity;
        public int Horses => _horses;
        public int Mules => _mules;
        public int Camels => _camels;
        public IReadOnlyList<CaravanItemSlot> Inventory => _inventory;
        public IReadOnlyList<CaravanGuardRecord> Guards => _guards;

        /// <summary>
        /// Diskteki JSON kaydından okunan temel kervan değerlerini çalışma anı modeline uygular.
        /// </summary>
        public void RestoreFromSave(int akce, int food, float maxWeightCapacity, int horses, int mules, int camels)
        {
            _currentAkce = Mathf.Max(0, akce);
            _currentFood = Mathf.Max(0, food);
            _maxWeightCapacity = maxWeightCapacity > 0f ? maxWeightCapacity : 300f;
            _horses = Mathf.Max(0, horses);
            _mules = Mathf.Max(0, mules);
            _camels = Mathf.Max(0, camels);
            NotifyStatsChanged();
        }

        public void SetAnimals(int horses, int mules, int camels)
        {
            _horses = Mathf.Max(0, horses);
            _mules = Mathf.Max(0, mules);
            _camels = Mathf.Max(0, camels);
            NotifyStatsChanged();
        }

        public float CalculateCurrentWeight()
        {
            float total = 0f;
            for (int i = 0; i < _inventory.Count; i++)
            {
                total += _inventory[i].TotalWeight;
            }
            return total;
        }

        public bool CanCarry(float additionalWeight)
        {
            return (CalculateCurrentWeight() + additionalWeight) <= _maxWeightCapacity;
        }

        public bool ModifyAkce(int delta)
        {
            if (_currentAkce + delta < 0) return false;
            _currentAkce += delta;
            NotifyStatsChanged();
            return true;
        }

        public bool ConsumeFood(int daysToConsume)
        {
            if (_currentFood < daysToConsume)
            {
                _currentFood = 0;
                NotifyStatsChanged();
                return false; // Erzak bitti, aclik/moral cezasi uygulanmali
            }
            _currentFood -= daysToConsume;
            NotifyStatsChanged();
            return true;
        }

        public void AddFood(int amount)
        {
            _currentFood = Mathf.Max(0, _currentFood + amount);
            NotifyStatsChanged();
        }

        /// <summary>
        /// Envanterdeki tüm ticaret mallarını temizler (kayıt yükleme veya yeni oyun için).
        /// </summary>
        public void ClearInventory()
        {
            _inventory.Clear();
            NotifyStatsChanged();
        }

        /// <summary>
        /// Diskteki JSON kaydından çözümlenen bir ticaret malı yuvasını envantere geri yükler.
        /// </summary>
        public void RestoreInventorySlot(ItemDataSO item, int quantity, int averagePurchasePrice)
        {
            if (item == null || quantity <= 0) return;

            int safeAvgPrice = Mathf.Max(1, averagePurchasePrice);
            var existing = FindSlot(item.ItemId);
            if (existing != null)
            {
                existing.Quantity += quantity;
                existing.AveragePurchasePrice = safeAvgPrice;
            }
            else
            {
                _inventory.Add(new CaravanItemSlot(item, quantity, safeAvgPrice));
            }

            NotifyStatsChanged();
        }

        /// <summary>
        /// Belirtilen ASCII emtia kimliğine (<paramref name="itemId"/>) sahip envanter yuvasını döndürür.
        /// </summary>
        public CaravanItemSlot FindSlot(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;

            for (int i = 0; i < _inventory.Count; i++)
            {
                var slot = _inventory[i];
                if (slot != null && slot.Item != null &&
                    string.Equals(slot.Item.ItemId, itemId, StringComparison.Ordinal))
                {
                    return slot;
                }
            }

            return null;
        }

        public bool AddGoods(ItemDataSO item, int count, int unitPrice)
        {
            if (item == null || count <= 0) return false;
            float addedWeight = item.UnitWeightKg * count;
            if (!CanCarry(addedWeight)) return false;

            var existing = FindSlot(item.ItemId);
            if (existing != null)
            {
                long totalOldValue = (long)existing.AveragePurchasePrice * existing.Quantity;
                long totalNewValue = (long)unitPrice * count;
                existing.Quantity += count;
                existing.AveragePurchasePrice = (int)((totalOldValue + totalNewValue) / existing.Quantity);
            }
            else
            {
                _inventory.Add(new CaravanItemSlot(item, count, unitPrice));
            }

            NotifyStatsChanged();
            return true;
        }

        public bool RemoveGoods(string itemId, int count)
        {
            var existing = FindSlot(itemId);
            if (existing == null || existing.Quantity < count) return false;

            existing.Quantity -= count;
            if (existing.Quantity <= 0)
            {
                _inventory.Remove(existing);
            }

            NotifyStatsChanged();
            return true;
        }

        public void RecruitGuard(UnitDataSO unitData)
        {
            if (unitData == null) return;
            _guards.Add(new CaravanGuardRecord(unitData));
        }

        public void RemoveGuard(string guardId)
        {
            _guards.RemoveAll(g => g.GuardId == guardId);
        }

        public void HealAllGuards(int healAmount)
        {
            foreach (var guard in _guards)
            {
                if (guard.UnitData != null)
                {
                    guard.CurrentHealth = Mathf.Min(guard.UnitData.MaxHealth, guard.CurrentHealth + healAmount);
                }
            }
        }

        private void NotifyStatsChanged()
        {
            EventBus.Publish(new CaravanStatsChangedEvent(
                _currentAkce,
                _currentFood,
                CalculateCurrentWeight(),
                _maxWeightCapacity
            ));
        }
    }
}
