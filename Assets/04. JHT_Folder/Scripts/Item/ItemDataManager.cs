using JHT;
using System.Collections.Generic;
using UnityEngine;

namespace JHT
{
    public class ItemDataManager : Singleton<ItemDataManager>
    {
        private Dictionary<int, ItemObject> itemDataDic;

        private void Awake()
        {
            base.Awake();
        }
    }
}
