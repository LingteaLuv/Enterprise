using JHT;
using UnityEngine;

namespace JHT
{
    public class DemoAddWeapon : MonoBehaviour
    {
        [SerializeField] private ItemObject item1;
        [SerializeField] private ItemObject item2;
        [SerializeField] private ItemObject item3;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddWeapon(item1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AddWeapon(item2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AddWeapon(item3);
            }
        }

        private void AddWeapon(ItemObject item)
        {
            InventoryManager.Instance.AddItem(item);
        }
    }
}
