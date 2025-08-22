using System;
using UnityEngine;

namespace JHT
{
    public class ItemEventManager : Singleton<ItemEventManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public Action<ItemObject> OnClickItem;

        public void ClickItem(ItemObject obj)
        {
            OnClickItem?.Invoke(obj);
        }
    }
}
