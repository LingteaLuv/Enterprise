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
        private const string RELICS_LABEL = "ItemRelics";

        public List<ItemWeaponSO> weaponList;
        public Dictionary<int, ItemWeaponSO> weaponDataDic;
        public EncyclopediaPanel encyclopediaPanel;
        
        public List<ItemRelicsSO> relicsList;
        public Dictionary<int, ItemRelicsSO> relicsDataDic;

        private AsyncOperationHandle<IList<ItemWeaponSO>> weaponHandle;
        private AsyncOperationHandle<IList<ItemRelicsSO>> relicsHandle;

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

            relicsList = new();
            relicsDataDic = new();

            weaponHandle = Addressables.LoadAssetsAsync<ItemWeaponSO>(WEAPON_LABEL);
            relicsHandle = Addressables.LoadAssetsAsync<ItemRelicsSO>(RELICS_LABEL);

            yield return weaponHandle;
            yield return relicsHandle;

            LoadWeaponList(weaponHandle);
            LoadRelicsList(relicsHandle);
        }
        #region 장비
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
                StartCoroutine(WeaponEndInit());
            }
        }
        private IEnumerator WeaponEndInit()
        {
            yield return new WaitForEndOfFrame();
            encyclopediaPanel.WeaponInit();
        }
        #endregion

        #region 유물
        private void LoadRelicsList(AsyncOperationHandle<IList<ItemRelicsSO>> objs)
        {
            List<ItemRelicsSO> list = new();
            foreach (var r in objs.Result)
            {
                list.Add(r);
            }

            for (int i = 0; i < list.Count; i++)
            {
                relicsList.Add(list[i]);
            }

            LoadRelicsFinish(relicsList);
        }

        private void LoadRelicsFinish(List<ItemRelicsSO> list)
        {
            relicsDataDic.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                if (!relicsDataDic.ContainsKey(list[i].itemNum))
                    relicsDataDic.Add(list[i].itemNum, list[i]);
            }

            if (relicsHandle.Status == AsyncOperationStatus.Succeeded)
            {
                //if (encyclopediaPanel == null)
                //    encyclopediaPanel = FindObjectOfType<EncyclopediaPanel>();
                StartCoroutine(RelicsEndInit());
            }
        }

        private IEnumerator RelicsEndInit()
        {
            yield return new WaitForEndOfFrame();
            encyclopediaPanel.RelicsInit();
        }
        #endregion


        public Dictionary<int, ItemWeaponSO> GetAllWeaponData()
        {
            return weaponDataDic;
        }

        public Dictionary<int, ItemRelicsSO> GetAllRelicsData()
        {
            return relicsDataDic;
        }
    }
}
