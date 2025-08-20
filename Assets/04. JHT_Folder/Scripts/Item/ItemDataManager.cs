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
        private const string WEAPON_PATH = "JHT_ItemWeapon";
        private const string WEAPON_LABEL = "ItemWeapon";
        public List<WeaponObject> weaponList;
        public Dictionary<int, WeaponObject> weaponDataDic;

        //public List<RelicsObject> relicsList;
        //public Dictionary<int, List<RelicsObject>> relicsDataDic;

        private AsyncOperationHandle<IList<GameObject>> weaponHandle;

        // 수정필요 : csv에서 데이터 받아온 후 초기화 할 수 있도록 설정 해야함 
        protected override void Awake()
        {
            base.Awake();

            StartCoroutine(WeaponDataSetting());
        }

        private IEnumerator WeaponDataSetting()
        {
            weaponList = new();
            weaponDataDic = new();

            weaponHandle = Addressables.LoadAssetsAsync<GameObject>(WEAPON_LABEL);

            yield return weaponHandle;
            Debug.Log($"aaaaaaaaaaa : {weaponHandle}");

            for (int i = 0; i < weaponHandle.Result.Count; i++)
            {
                Debug.Log($"bbbbbbbbb : {weaponHandle.Result[i]}");
            }
            weaponHandle.Completed += (w) => LoadWeaponList(weaponHandle);

        }
        
        private void LoadWeaponList(AsyncOperationHandle<IList<GameObject>> objs)
        {
            List<WeaponObject> list = new();
            foreach(var w in objs.Result)
            {
                list.Add(w.GetComponent<WeaponObject>());
            }

            List<WeaponObject> sortList = list.OrderBy(w => w.itemName).ToList();

            for (int i =0; i < sortList.Count; i ++)
            {
                weaponList.Add(sortList[i]);
            }

            LoadWeaponFinish(weaponList);
        }


        private void LoadWeaponFinish(List<WeaponObject> list)
        {
            weaponDataDic.Clear();

            for (int i =0; i < list.Count; i++)
            {
                if (!weaponDataDic.ContainsKey(list[i].itemNum))
                    weaponDataDic.Add(list[i].itemNum, list[i]);
            }

        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    foreach (var w in weaponDataDic)
            //    {
            //        Debug.Log($"weaponDic : {w.Value.itemName}");
            //    }
            //}
        }
    }
}
