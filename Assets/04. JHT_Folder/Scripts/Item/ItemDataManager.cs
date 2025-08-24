using JHT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;
using System.Collections;
using Unity.VisualScripting;
using System.Linq;

namespace JHT
{
    public class ItemDataManager : Singleton<ItemDataManager>
    {
        private const string WEAPON_LABEL = "ItemWeapon";
        public List<ItemWeaponSO> weaponList;
        public Dictionary<int, ItemWeaponSO> weaponDataDic;
        public EncyclopediaPanel encyclopediaPanel;
        //public List<RelicsObject> relicsList;
        //public Dictionary<int, List<RelicsObject>> relicsDataDic;

        private AsyncOperationHandle<IList<ItemWeaponSO>> weaponHandle;

        // 수정필요 : csv에서 데이터 받아온 후 초기화 할 수 있도록 설정 해야함 
        protected override void Awake()
        {
            base.Awake();

            //StartCoroutine(WeaponDataSetting());
        }

        private IEnumerator Start()
        {
            yield return null;

            weaponList = new();
            weaponDataDic = new();

            weaponHandle = Addressables.LoadAssetsAsync<ItemWeaponSO>(WEAPON_LABEL);

            yield return weaponHandle;

            LoadWeaponList(weaponHandle);

        }
        
        private void LoadWeaponList(AsyncOperationHandle<IList<ItemWeaponSO>> objs)
        {
            List<ItemWeaponSO> list = new();
            foreach(var w in objs.Result)
            {
                list.Add(w);
            }

            //List<ItemWeaponSO> sortList = list.OrderBy(w => w.itemNum).ToList();

            for (int i =0; i < list.Count; i ++)
            {
                weaponList.Add(list[i]);
            }

            LoadWeaponFinish(weaponList);
        }


        private void LoadWeaponFinish(List<ItemWeaponSO> list)
        {
            weaponDataDic.Clear();

            for (int i =0; i < list.Count; i++)
            {
                if (!weaponDataDic.ContainsKey(list[i].itemNum))
                    weaponDataDic.Add(list[i].itemNum, list[i]);
            }

            if (weaponHandle.Status == AsyncOperationStatus.Succeeded)
            {
                //if (encyclopediaPanel == null)
                //    encyclopediaPanel = FindObjectOfType<EncyclopediaPanel>();
                StartCoroutine(EndInit());
            }
        }

        private IEnumerator EndInit()
        {
            yield return new WaitForEndOfFrame();
            encyclopediaPanel.Init();
        }

        public Dictionary<int, ItemWeaponSO> GetAllWeaponData()
        {
            return weaponDataDic;
        }
    }
}
