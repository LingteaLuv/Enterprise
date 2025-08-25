using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace JHT
{
    public class EncyclopediaPanel : MonoBehaviour
    {
        public Dictionary<int, ItemWeaponSO> weaponDic;
        public Dictionary<int, ItemRelicsSO> relicsDic;

        [SerializeField] private EncyclopediaPanelItem encyclopediaPanelItem;
        [SerializeField] private Transform encyclopediaPanelWeaponParent;
        [SerializeField] private Transform encyclopediaPanelRelicsParent;

        public void WeaponInit()
        {
            weaponDic = new();

            foreach (var k in ItemDataManager.Instance.GetAllWeaponData())
                weaponDic[k.Key] = k.Value;

            ShowAllWeapon();
        }

        public void RelicsInit()
        {
            relicsDic = new();

            foreach (var k in ItemDataManager.Instance.GetAllRelicsData())
                relicsDic[k.Key] = k.Value;

            ShowAllRelics();
        }

        private void ShowAllWeapon()
        {
            for (int i = 0; i < ItemDataManager.Instance.GetAllWeaponData().Count; i++)
            {
                EncyclopediaPanelItem obj = Instantiate(encyclopediaPanelItem);
                obj.transform.SetParent(encyclopediaPanelWeaponParent);
                obj.Init(ItemDataManager.Instance.GetAllWeaponData()[i]);
            }
        }

        private void ShowAllRelics()
        {
            for (int i = 0; i < ItemDataManager.Instance.GetAllRelicsData().Count; i++)
            {
                EncyclopediaPanelItem obj = Instantiate(encyclopediaPanelItem);
                obj.transform.SetParent(encyclopediaPanelRelicsParent);
                obj.Init(ItemDataManager.Instance.GetAllRelicsData()[i]);
            }
        }
    }
}
