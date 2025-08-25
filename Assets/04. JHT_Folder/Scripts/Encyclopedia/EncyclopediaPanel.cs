using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace JHT
{
    public class EncyclopediaPanel : MonoBehaviour
    {
        public Dictionary<int, ItemWeaponSO> weaponDic;
        [SerializeField] private EncyclopediaPanelItem encyclopediaPanelItem;
        [SerializeField] private Transform encyclopediaPanelItemParent;

        public void Init()
        {
            weaponDic = new();

            foreach (var k in ItemDataManager.Instance.GetAllWeaponData())
                weaponDic[k.Key] = k.Value;

            ShowAllWeapon();
        }


        private void ShowAllWeapon()
        {
            /*for (int i = 0; i < ItemDataManager.Instance.GetAllWeaponData().Count; i++)
            {
                EncyclopediaPanelItem obj = Instantiate(encyclopediaPanelItem);
                obj.transform.SetParent(encyclopediaPanelItemParent);
                obj.Init(ItemDataManager.Instance.GetAllWeaponData()[i]);
            }*/
        }
    }
}
