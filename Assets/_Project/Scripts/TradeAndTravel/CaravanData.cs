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
    /// Kervanin seyahat, ticaret ve savas boyunca sahip oldugu canli verileri
    /// (Akce, Erzak, Envanter, Muhafizlar ve Yuk) yoneten cekirdek veri modeli.
    /// </summary>
    [Serializable]
    public class CaravanData
    {
        [Header("Temel Kaynaklar")]
        [SerializeField] private int _currentAkce = 500;
        [SerializeField] private int _currentFood = 20; // 20 gunluk temel erzak
        [SerializeField] private float _maxWeightCapacity = 300f; // 300 Kg temel yuk kapasitesi

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

        public bool AddGoods(ItemDataSO item, int count, int unitPrice)
        {
            if (item == null || count <= 0) return false;
            float addedWeight = item.UnitWeightKg * count;
            if (!CanCarry(addedWeight)) return false;

            var existing = _inventory.Find(s => s.Item != null && s.Item.ItemId == item.ItemId);
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
            var existing = _inventory.Find(s => s.Item != null && s.Item.ItemId == itemId);
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
