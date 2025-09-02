using System;
using JHT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

namespace JHT
{
    public class ItemDataManager : Singleton<ItemDataManager>
    {
        private const string WEAPON_LABEL = "ItemWeapon";
        private const string RELICS_LABEL = "ItemRelics";

        public List<ItemWeaponSO> weaponList;
        public Dictionary<string, ItemWeaponSO> weaponDataDic;
        
        public List<ItemRelicsSO> relicsList;
        public Dictionary<string, ItemRelicsSO> relicsDataDic;

        private AsyncOperationHandle<IList<ItemWeaponSO>> weaponHandle;
        private AsyncOperationHandle<IList<ItemRelicsSO>> relicsHandle;

        public bool IsDataLoaded { get; private set; } = false;

        
        public JHT_DataDownLoader downLoader;
        
        //데이터 로딩 완료시 호출
        public Action<bool> OnRelicsDataLoadFinish;
        public Action OnRelicInit;
        public Action OnWeaponInit;

        // 수정필요 : csv에서 데이터 받아온 후 초기화 할 수 있도록 설정 해야함 
        protected override void Awake()
        {
            base.Awake();

            //StartCoroutine(WeaponDataSetting());
        }

        private IEnumerator Start()
        {
            yield return null;

            OnRelicsDataLoadFinish?.Invoke(false);

            weaponList = new();
            weaponDataDic = new();

            relicsList = new();
            relicsDataDic = new();

            ItemWeaponSO[] loadedWeapons = Resources.LoadAll<ItemWeaponSO>("EquipData");
            LoadWeaponList(loadedWeapons);
            //weaponHandle = Addressables.LoadAssetsAsync<ItemWeaponSO>(WEAPON_LABEL);
            relicsHandle = Addressables.LoadAssetsAsync<ItemRelicsSO>(RELICS_LABEL);

            //yield return weaponHandle;
            yield return relicsHandle;

            //LoadWeaponList(weaponHandle);
            LoadRelicsList(relicsHandle);
        }
        #region 장비
        private void LoadWeaponList(ItemWeaponSO[] objs)
        {
            List<ItemWeaponSO> list = new();
            foreach (var w in objs)
            {
                list.Add(w);
            }

            //List<ItemWeaponSO> sortList = list.OrderBy(w => w.itemNum).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                weaponList.Add(list[i]);
            }

            LoadWeaponFinish(weaponList);
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
                if (!weaponDataDic.ContainsKey(list[i].itemName))
                    weaponDataDic.Add(list[i].itemName, list[i]);
            }

            IsDataLoaded = true; // 데이터 로딩 완료!
            StartCoroutine(WeaponEndInit());
            /*if (weaponHandle.Status == AsyncOperationStatus.Succeeded)
            {
                //if (encyclopediaPanel == null)
                //    encyclopediaPanel = FindObjectOfType<EncyclopediaPanel>();
                StartCoroutine(WeaponEndInit());
            }*/
        }
        private IEnumerator WeaponEndInit()
        {
            yield return new WaitForEndOfFrame();
            OnWeaponInit?.Invoke();
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
                if (!relicsDataDic.ContainsKey(list[i].itemName))
                    relicsDataDic.Add(list[i].itemName, list[i]);
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
            OnRelicInit?.Invoke();

            yield return DownLoadCSV();
        }

        private IEnumerator DownLoadCSV()
        {
            while (!IsDataLoaded)
                yield return null;

            downLoader = new JHT_DataDownLoader();

            OnRelicsDataLoadFinish?.Invoke(true);
            yield return downLoader.DownloadData();
        }

        #endregion


        public Dictionary<string, ItemWeaponSO> GetAllWeaponData()
        {
            if (weaponDataDic.Count <= 0)
                return null;

            return weaponDataDic;
        }

        public Dictionary<string, ItemRelicsSO> GetAllRelicsData()
        {
            if (relicsDataDic.Count <= 0)
                return null;

            return relicsDataDic;
        }

    }
}
