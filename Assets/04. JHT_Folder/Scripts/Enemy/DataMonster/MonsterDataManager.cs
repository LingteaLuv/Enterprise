using System;
using System.Collections;
using System.Collections.Generic;
using JHT;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MonsterDataManager : Singleton<MonsterDataManager>
{
    private const string MONSTERTABLE_LABEL = "MonsterTable";
    private const string MONSTERDATA_LABEL = "MonsterData";

    private List<JHT_MonsterDataTable> monsterTableList;
    private List<JHT_MonsterDataSO> monsterDataList;

    public Dictionary<int, JHT_MonsterDataTable> monsterTableDic;
    public Dictionary<int, JHT_MonsterDataSO> monsterDataDic;

    private AsyncOperationHandle<IList<JHT_MonsterDataTable>> monsterTableHandle;
    private AsyncOperationHandle<IList<JHT_MonsterDataSO>> monsterDataHandle;

    public JHT_DataDownLoader downLoader;

    public bool isDataLoaded { get; private set; } = false;
    public bool isTableLoaded { get; private set; } = false;

    private bool isDataLodedFinish;
    private bool isTableLoadedFinish;

    public Action OnMonsterDataLoadFinish;
    public Action OnMonsterTableLoadFinish;

    protected override void Awake()
    {
        base.Awake();
    }

    private IEnumerator Start()
    {
        yield return null;

        isDataLodedFinish = false;
        isTableLoadedFinish = false;

        monsterTableList = new();
        monsterDataList = new();

        monsterTableDic = new();
        monsterDataDic = new();

        monsterTableHandle = Addressables.LoadAssetsAsync<JHT_MonsterDataTable>(MONSTERTABLE_LABEL);
        monsterDataHandle = Addressables.LoadAssetsAsync<JHT_MonsterDataSO>(MONSTERDATA_LABEL);

        yield return monsterTableHandle;
        yield return monsterDataHandle;

        LoadMonsterTableList(monsterTableHandle);
        LoadMonsterDataList(monsterDataHandle);

    }

    #region MonsterTable
    private void LoadMonsterTableList(AsyncOperationHandle<IList<JHT_MonsterDataTable>> objs)
    {
        List<JHT_MonsterDataTable> list = new();
        foreach (var o in objs.Result)
        {
            list.Add(o);
        }

        for (int i = 0; i < list.Count; i++)
        {
            monsterTableList.Add(list[i]);
        }

        LoadMonsterTableFinish(list);
    }

    private void LoadMonsterTableFinish(List<JHT_MonsterDataTable> list)
    {
        monsterTableDic.Clear();

        for (int i = 0; i < list.Count; i++)
        {
            if (!monsterTableDic.ContainsKey(list[i].ID))
            {
                monsterTableDic.Add(list[i].ID, list[i]);
            }
        }

        if (monsterTableHandle.Status == AsyncOperationStatus.Succeeded)
        {
            isTableLoadedFinish = true;
            StartCoroutine(MonsterTableEndInit());
        }
    }

    IEnumerator MonsterTableEndInit()
    {
        yield return new WaitForEndOfFrame();
        OnMonsterTableLoadFinish?.Invoke();

        yield return DownLoadCSV();
    }

    #endregion

    #region MonsterData
    private void LoadMonsterDataList(AsyncOperationHandle<IList<JHT_MonsterDataSO>> objs)
    {
        List<JHT_MonsterDataSO> list = new();
        foreach (var o in objs.Result)
        {
            list.Add(o);
        }

        for (int i = 0; i < list.Count; i++)
        {
            monsterDataList.Add(list[i]);
        }

        LoadMonsterDataFinish(list);
    }

    private void LoadMonsterDataFinish(List<JHT_MonsterDataSO> list)
    {
        monsterDataDic.Clear();

        for (int i = 0; i < list.Count; i++)
        {
            if (!monsterDataDic.ContainsKey(list[i].ID))
            {
                monsterDataDic.Add(list[i].ID, list[i]);
            }
        }

        if (monsterDataHandle.Status == AsyncOperationStatus.Succeeded)
        {
            isDataLodedFinish = true;
            StartCoroutine(MonsterDataEndInit());
        }
    }

    private IEnumerator MonsterDataEndInit()
    {
        yield return new WaitForEndOfFrame();
        
        OnMonsterDataLoadFinish?.Invoke();

        yield return DownLoadCSV();
    }

    private IEnumerator DownLoadCSV()
    {
        while (!isDataLodedFinish && !isTableLoaded)
            yield return null;

        yield return downLoader.DownLoadMonsterData();
    }

    #endregion

    public Dictionary<int, JHT_MonsterDataTable> GetAllMoneterTableData()
    {
        if (monsterTableDic.Count <= 0)
        {
            return null;
        }

        return monsterTableDic;
    }

    public Dictionary<int, JHT_MonsterDataSO> GetAllMonsterData()
    {
        if (monsterDataDic.Count <= 0)
        {
            return null;
        }

        return monsterDataDic;
    }
}
