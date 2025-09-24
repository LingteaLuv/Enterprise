using System;
using System.Collections;
using System.Collections.Generic;
using JHT;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MonsterDataManager : Singleton<MonsterDataManager>
{
    private const string MONSTERTABLE_LABEL = "MonsterSpawnRoundTable";
    private const string MONSTERDATA_LABEL = "MonsterData";
    private const string MONSTERPREFAB_LABEL = "MonsterPrefab";
    private const string MONSTERSKILL_LABEL = "MonsterSkill"; 

    public List<JHT_MonsterDataTable> monsterTableList;
    public List<JHT_MonsterDataSO> monsterDataList;
    public List<GameObject> monsterPrefabList;
    public List<MonsterSkillSO> monsterSkillList;
    public RuntimeAnimatorController animatorController;

    public Dictionary<int, JHT_MonsterDataTable> monsterTableDic;
    public Dictionary<string, JHT_MonsterDataSO> monsterDataDic;
    public Dictionary<string, GameObject> monsterPrefabDic;
    public Dictionary<string,  MonsterSkillSO> monsterSkillDic;
    

    private AsyncOperationHandle<IList<JHT_MonsterDataTable>> monsterTableHandle;
    private AsyncOperationHandle<IList<JHT_MonsterDataSO>> monsterDataHandle;
    private AsyncOperationHandle<IList<GameObject>> monsterPrefabHandle;
    private AsyncOperationHandle<IList<MonsterSkillSO>> monsterSkillHandle;

    public JHT_DataDownLoader dataLoader;
    public MonsterSkillSet skillManager;

    public bool isDataLoaded { get; private set; }
    public bool isTableLoaded { get; private set; }
    public bool IsPrefabLoadedFinish;
    public bool isSkillListFinish;

    private bool isDataLodedFinish;
    private bool isTableLoadedFinish;
    
    public Action OnMonsterDataLoadFinish;
    public Action OnMonsterTableLoadFinish;
    public Action OnMonsterPrefabLoadFinish;
    public Action OnMonsterSkillLoadFinish;

    protected override void Awake()
    {
        base.Awake();
        dataLoader = new JHT_DataDownLoader();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        dataLoader.OnMonsterDataTableSetCompleted -= AllDataLoaded;
        dataLoader.OnSkillDataSetCompleted -= LoadMonsterSkillFinish;
    }

    private IEnumerator Start()
    {
        yield return null;

        dataLoader.OnMonsterDataTableSetCompleted += AllDataLoaded;
        dataLoader.OnSkillDataSetCompleted += LoadMonsterSkillFinish;

        isDataLodedFinish = false;
        isTableLoadedFinish = false;
        IsPrefabLoadedFinish = false;

        monsterTableList = new();
        monsterDataList = new();
        monsterPrefabList = new();
        monsterSkillList = new();

        monsterTableDic = new();
        monsterDataDic = new();
        monsterPrefabDic = new();
        monsterSkillDic = new();

        var handle = Addressables.LoadAssetAsync<RuntimeAnimatorController>("Assets/04. JHT_Folder/Animator/JHT_Monster_Animator.controller");
        monsterTableHandle = Addressables.LoadAssetsAsync<JHT_MonsterDataTable>("MonsterSpawnRoundTable");
        monsterDataHandle = Addressables.LoadAssetsAsync<JHT_MonsterDataSO>("MonsterData");
        monsterPrefabHandle = Addressables.LoadAssetsAsync<GameObject>("MonsterPrefab");
        monsterSkillHandle = Addressables.LoadAssetsAsync<MonsterSkillSO>("MonsterSkill");

        yield return handle;
        yield return monsterPrefabHandle;
        yield return monsterTableHandle;
        yield return monsterDataHandle;
        yield return monsterSkillHandle;

        animatorController = handle.Result;

        LoadMonsterTableList(monsterTableHandle);
        LoadMonsterDataList(monsterDataHandle);
        LoadMonsterPrefabList(monsterPrefabHandle);
        LoadMonsterSkillList(monsterSkillHandle);
    }

    #region MonsterSkill

    private void LoadMonsterSkillList(AsyncOperationHandle<IList<MonsterSkillSO>> objs)
    {
        List<MonsterSkillSO> list = new();
        foreach (var o in objs.Result)
        {
            monsterSkillList.Add(o);
        }
        monsterSkillList.Sort((a, b) => a.ID.CompareTo(b.ID));

        if (monsterSkillHandle.Status == AsyncOperationStatus.Succeeded)
        {
            StartCoroutine(DownLoadSkillData());
        }

    }

    private void LoadMonsterSkillFinish(List<MonsterSkillSO> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
            {
                Debug.LogError($"스킬 : 없음");
            }


            if (!monsterSkillDic.ContainsKey(list[i].skillName))
            {
                monsterSkillDic.Add(list[i].skillName, list[i]);
            }
        }

        OnMonsterSkillLoadFinish?.Invoke();
    }
    IEnumerator DownLoadSkillData()
    {
        yield return null;

        yield return dataLoader.DownLoadSkillTabledata();
    }
    #endregion


    #region MonsterPrefab

    private void LoadMonsterPrefabList(AsyncOperationHandle<IList<GameObject>> objs)
    {
        List<GameObject> list = new();
        foreach (var o in objs.Result)
        {
            list.Add(o);
        }

        for (int i = 0; i < list.Count; i++)
        {
            monsterPrefabList.Add(list[i]);
        }


        LoadMonsterPrefabFinish(list);
    }

    private void LoadMonsterPrefabFinish(List<GameObject> list)
    {
        monsterPrefabDic.Clear();

        for (int i = 0; i < list.Count; i++)
        {
            if (!monsterPrefabDic.ContainsKey(list[i].name))
            {
                monsterPrefabDic.Add(list[i].name, list[i]);
            }
        }

        if (monsterPrefabHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IsPrefabLoadedFinish = true;
            //StartCoroutine(MonsterPrefabEndInit());
        }
    }
    
    #endregion

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
        list.Sort((a,b) => a.ID.CompareTo(b.ID));
        monsterTableList = list;

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
            StartCoroutine(DownLoadTableCSV());
        }
        else
        {
            Debug.LogError($"테이블 실패 : {monsterTableHandle.Status}");
        }
    }

    IEnumerator DownLoadTableCSV()
    {
        yield return null;

        while (!(isDataLodedFinish && isTableLoadedFinish))
            yield return null;

        yield return dataLoader.DownLoadMonsterData();
    }

    private void AllDataLoaded()
    {
        OnMonsterTableLoadFinish?.Invoke();
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
        list.Sort((a,b) => a.ID.CompareTo(b.ID));
        monsterDataList = list;
        LoadMonsterDataFinish(list);
    }

    private void LoadMonsterDataFinish(List<JHT_MonsterDataSO> list)
    {
        monsterDataDic.Clear();

        for (int i = 0; i < list.Count; i++)
        {
            if (!monsterDataDic.ContainsKey(list[i].ID.ToString()))
            {
                monsterDataDic.Add(list[i].ID.ToString(), list[i]);
            }
        }

        if (monsterDataHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("bbb");
            isDataLodedFinish = true;
        }
        else
        {
            Debug.LogError($"데이터 실패 : {monsterDataHandle.Status}");
        }
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

    public Dictionary<string, JHT_MonsterDataSO> GetAllMonsterData()
    {
        if (monsterDataDic.Count <= 0)
        {
            return null;
        }

        return monsterDataDic;
    }

}
