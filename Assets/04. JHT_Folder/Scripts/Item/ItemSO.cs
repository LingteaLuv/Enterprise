using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public abstract class ItemSO : ScriptableObject
    {
        [field: SerializeField] public int itemNum { get; private set; }
        [field: SerializeField] public string itemName { get; private set; }
        [field: SerializeField] public ItemType itemType { get; private set; }
        [field: SerializeField] public Sprite icon { get; private set; } = null;
        [field: SerializeField] public Sprite backImage { get; private set; }
        [field: SerializeField] public string desc { get; private set; }
        [field: SerializeField] public bool isStrengthen { get; private set; }

        public virtual void UseItem(ItemSO item) { }
    }

    public enum ItemType
    {
        Weapon,
        Relics,
        Crystal,
        NormalItem
    }
}