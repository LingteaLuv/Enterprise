using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public abstract class ItemSO : ScriptableObject
    {
        [field: SerializeField] public string itemName { get; set; }
        [field: SerializeField] public ItemType itemType { get; set; }
        [field: SerializeField] public Sprite icon { get; set; }
        [field: SerializeField] public Image backImage { get; set; }
        [field: SerializeField] public string desc { get; set; }
        [field: SerializeField] public float attackRange { get; set; }
        [field: SerializeField] public bool isStrengthen { get; set; }

        public virtual void UseItem(ItemSO item) { }
    }

    public enum ItemType
    {
        Weapon,
        Crystal,
        NormalItem
    }
}