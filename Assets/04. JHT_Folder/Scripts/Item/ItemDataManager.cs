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
        private const string LOOTTABLE_LABEL = "RootTable";

        public List<ItemWeaponSO> weaponList;
        public Dictionary<string, ItemWeaponSO> weaponDataDic;
        
        public List<ItemRelicsSO> relicsList;
        public Dictionary<string, ItemRelicsSO> relicsDataDic;

        public List<RelicsGachaLootTable> lootTableList;
        public Dictionary<int, RelicsGachaLootTable> lootTableDic;

        private AsyncOperationHandle<IList<ItemWeaponSO>> weaponHandle;
        private AsyncOperationHandle<IList<ItemRelicsSO>> relicsHandle;
        private AsyncOperationHandle<IList<RelicsGachaLootTable>> lootTableHandler;

        public bool IsDataLoaded { get; private set; } = false;
        public bool IsRelicsDataLoaded { get; private set; } = false;
        public bool IsLootTableDataLoaded { get; private set; } = false;

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

            downLoader = new();

            weaponList = new();
            weaponDataDic = new();

            relicsList = new();
            relicsDataDic = new();

            lootTableList = new();
            lootTableDic = new();

            ItemWeaponSO[] loadedWeapons = Resources.LoadAll<ItemWeaponSO>("EquipData");
            LoadWeaponList(loadedWeapons);

            //weaponHandle = Addressables.LoadAssetsAsync<ItemWeaponSO>(WEAPON_LABEL);
            relicsHandle = Addressables.LoadAssetsAsync<ItemRelicsSO>("ItemRelics");
            lootTableHandler = Addressables.LoadAssetsAsync<RelicsGachaLootTable>("RootTable");

            //yield return weaponHandle;
            yield return relicsHandle;
            yield return lootTableHandler;

            //LoadWeaponList(weaponHandle);
            LoadRelicsList(relicsHandle);
            LoadLootTableList(lootTableHandler);
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
                IsRelicsDataLoaded = true;
                StartCoroutine(RelicsEndInit());
            }
        }

        private IEnumerator RelicsEndInit()
        {
            OnRelicInit?.Invoke();

            yield return DownRelicsCSV();
        }

        private IEnumerator DownRelicsCSV()
        {
            while (!IsRelicsDataLoaded)
                yield return null;
            Debug.LogError($"유물 csv");
            //downLoader = new JHT_DataDownLoader();
            yield return downLoader.DownRelicsData();

            OnRelicsDataLoadFinish?.Invoke(true);
        }

        #endregion

        #region 가챠 테이블
        private void LoadLootTableList(AsyncOperationHandle<IList<RelicsGachaLootTable>> table)
        {
            List<RelicsGachaLootTable> list = new();

            foreach (var r in table.Result)
            {
                list.Add(r);
            }

            for (int i = 0; i < list.Count; i++)
            {
                lootTableList.Add(list[i]);
            }

            LoadLootTableFinish(lootTableList);
        }

        private void LoadLootTableFinish(List<RelicsGachaLootTable> list)
        {
            lootTableDic.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                if (!lootTableDic.ContainsKey(list[i].tableNum))
                    lootTableDic.Add(list[i].tableNum, list[i]);
            }

            if (lootTableHandler.Status == AsyncOperationStatus.Succeeded)
            {
                //if (encyclopediaPanel == null)
                //    encyclopediaPanel = FindObjectOfType<EncyclopediaPanel>();
                IsLootTableDataLoaded = true;
                StartCoroutine(LootTableEndit());
            }
        }

        private IEnumerator LootTableEndit()
        {

            while (!IsLootTableDataLoaded)
                yield return null;

            //downLoader = new JHT_DataDownLoader();
            Debug.LogError($"루트테이블 csv");
            yield return downLoader.DownLootTableData();
            OnRelicsDataLoadFinish?.Invoke(true);
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
