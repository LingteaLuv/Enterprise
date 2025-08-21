using JHT;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    //구조 바꿔야함 상속으로 전체 아이템 담을거임
    
    public class ItemObject
    {
        public ItemSO itemSO { get; set; }
        public int itemNum { get; set; }

        public Sprite itemIcon { get; set; }

        public string itemName { get; set; }

        public virtual void Init()
        {
            itemName = itemSO.itemName;
        }
    }
}
