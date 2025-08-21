using System;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class DataItem : MonoBehaviour
    {
        public ItemSO itemSO;
        public string itemName;
        public int itemNum;
        public Image itemIcon;

        public void Init()
        {
            itemIcon.sprite = itemSO.icon;
            itemName = itemSO.itemName;
        }

    }
}
