using System;
using JHT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Linq;

namespace JHT
{
    public class ItemDataManager : Singleton<ItemDataManager>
    {
        private const string WEAPON_LABEL = "Equips";
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

            // 어드레서블로 모든 데이터 비동기 로드 시작
            weaponHandle = Addressables.LoadAssetsAsync<ItemWeaponSO>("Equips");
            relicsHandle = Addressables.LoadAssetsAsync<ItemRelicsSO>("ItemRelics");
            lootTableHandler = Addressables.LoadAssetsAsync<RelicsGachaLootTable>("RootTable");

            yield return weaponHandle;
            yield return relicsHandle;
            yield return lootTableHandler;

            // 로드 완료된 데이터 처리
            LoadWeaponList(weaponHandle);
            LoadRelicsList(relicsHandle);
            LoadLootTableList(lootTableHandler);
        }

        #region 장비
        private void LoadWeaponList(AsyncOperationHandle<IList<ItemWeaponSO>> objs)
        {
            if (objs.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("무기 데이터 로드에 실패했습니다!");
                return;
            }

            List<ItemWeaponSO> list = new List<ItemWeaponSO>(objs.Result);

            // 아이템 번호(itemNum) 기준으로 리스트 정렬
            List<ItemWeaponSO> sortList = list.OrderBy(w => w.itemNum).ToList();

            weaponList.Clear();
            weaponList.AddRange(sortList);

            LoadWeaponFinish(weaponList);
        }

        private void LoadWeaponFinish(List<ItemWeaponSO> list)
        {
            weaponDataDic.Clear();

            foreach (var item in list)
            {
                if (!weaponDataDic.ContainsKey(item.itemName))
                {
                    weaponDataDic.Add(item.itemName, item);
                }
            }

            if (weaponHandle.Status == AsyncOperationStatus.Succeeded)
            {
                //if (encyclopediaPanel == null)
                //    encyclopediaPanel = FindObjectOfType<EncyclopediaPanel>();
                IsDataLoaded = true; // 데이터 로딩 완료!
                StartCoroutine(WeaponEndInit());
            }
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
