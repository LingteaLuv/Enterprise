using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    //구조 바꿔야함 상속으로 전체 아이템 담을거임
    public class ItemObject : MonoBehaviour
    {
        [field: SerializeField] public ItemSO itemSO { get; private set; }
        public Sprite itemIcon;
        public string itemName;

        public virtual void Init()
        {
            itemName = itemSO.itemName;
        }
    }
}
