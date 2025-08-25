using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public abstract class ItemSO : ScriptableObject
    {
        [field: SerializeField] public int itemNum;
        [field: SerializeField] public string itemName;
        [field: SerializeField] public ItemType itemType;
        [field: SerializeField] public Sprite icon;
        [field: SerializeField] public Sprite backImage;
        [field: SerializeField] public string desc;
        [field: SerializeField] public bool isStrengthen;

        public virtual void UseItem(ItemSO item) { }
    }

    public enum ItemType
    {
        Equip,
        Relics,
        Crystal,
        NormalItem
    }

    public enum ItemRarity
    {
        Normal = 0,
        Rare,
        Epic,
        Unique,
        Legend
    }

}