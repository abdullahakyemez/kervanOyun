using UnityEngine;
using Kervan.Core;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Kervan ticaretindeki emtialari (Ipek, Demir, Bugday, Baharat vb.)
    /// tanimlayan ScriptableObject veri modeli.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData_New", menuName = "Kervan/Trade/Item Data", order = 1)]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Emtia Kimligi")]
        [SerializeField] private string _itemId;
        [SerializeField] private string _itemName;
        [SerializeField] private ItemCategory _category = ItemCategory.RawMaterial;
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Header("Ekonomik Parametreler")]
        [Tooltip("Standart taban alis-satis fiyati (Akce cinsinden)")]
        [SerializeField] private int _basePrice = 50;
        [Tooltip("Her bir birimin kervan yukune getirdigi agirlik (Kg)")]
        [SerializeField] private float _unitWeightKg = 1.0f;

        [Header("Gorsel Varliklar")]
        [SerializeField] private Sprite _icon;

        // Getters
        public string ItemId => _itemId;
        public string ItemName => _itemName;
        public ItemCategory Category => _category;
        public string Description => _description;
        public int BasePrice => _basePrice;
        public float UnitWeightKg => _unitWeightKg;
        public Sprite Icon => _icon;
    }
}
