using JHT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicsGachaManager : MonoBehaviour
{
    public ItemRelicsSO relicsResult { get; set; }
    public ItemRarity rarityResult { get; set; }
    public int levelResult { get; set; }

    public int relicsCouponCost;
    public int relicsSpecialCost;
    public int relicsUpgradeCost;
    public int relicsTablelevel { get; set; }

    public bool isChoose;

    private List<ItemRelicsSO> gachaData;


    ItemDataManager dataManager;
    public RelicsGachaLootTable curRarityTable;
    public RelicsGachaTableManager relicsGachaTableManager;
    [SerializeField] private RelicsGachaLootTable levelTable;
    [SerializeField] private RelicsGachaLootTable specialTable;

    private InventoryManager inventoryManager;

    public Action<RelicsObject, RelicsObject> OnChooseItem;
    
    private bool isSpecial = false;

    public void Start()
    {
        inventoryManager = InventoryManager.Instance;
        dataManager = ItemDataManager.Instance;
        gachaData = new();
        dataManager.OnRelicsDataLoadFinish += DataSetting;
        relicsCouponCost = 2;
        relicsSpecialCost = 200;

        //dtabase사용하게되면 변경
        relicsTablelevel = 0;
        DataSetting(true);
    }

    private void DataSetting(bool value)
    {
        if (value)
        {
            gachaData.Clear();
            foreach (var data in dataManager.relicsList)
            {
                gachaData.Add(data);
            }
        }
    }

    public void GetGachaOneRelicsData(int curTableLevel)
    {
        StartCoroutine(GetData(curTableLevel));
    }

    private IEnumerator GetData(int curTableLevel)
    {
        isSpecial = false;
        if (curTableLevel >= 0)
        {
            GetRarity(curTableLevel);
            while (rarityResult == ItemRarity.None)
                yield return null;
        }
        else
        {
            GetSpecialRarity();
            while (rarityResult == ItemRarity.None)
                yield return null;
        }

        GetSO();
        while (relicsResult == null)
            yield return null;

        GetLevel();
        yield return new WaitUntil(() => levelResult != -1);

        AddResult(relicsResult, rarityResult, levelResult);
        QuestSignalManager.Instance.GachaPull(ItemType.Relic,1);
        if(isSpecial) QuestSignalManager.Instance.GachaPull(ItemType.RareRelic,1);
    }

    public void GetSO()
    {
        if (gachaData.Count == 0)
        {
            Debug.Log($"[GetSO] : gachaData 데이터 없음");
        }
        Debug.Log($"[GetSO] : {gachaData.Count}");
        int randIndex = UnityEngine.Random.Range(0, gachaData.Count);
        ItemRelicsSO so = gachaData[randIndex];

        relicsResult = so;
    }

    public void GetRarity(int curTableLevel)
    {
        curRarityTable = relicsGachaTableManager.relicsGachaTables[curTableLevel];
        RelicsGacha picked = curRarityTable.GetRandomRange();
        rarityResult = picked.rarity;
    }

    public void GetSpecialRarity()
    {
        RelicsGacha picked = specialTable.GetRandomRange();
        rarityResult = picked.rarity;
        isSpecial = true;
    }

    public void GetLevel()
    {
        RelicsGacha picked = levelTable.GetRandomRange();
        switch (picked.rarity)
        {
            case ItemRarity.Normal:
                levelResult = UnityEngine.Random.Range(1, 41);
                break;
            case ItemRarity.Rare:
                levelResult = UnityEngine.Random.Range(41, 70);
                break;
            case ItemRarity.Epic:
                levelResult = UnityEngine.Random.Range(71, 85);
                break;
            case ItemRarity.Unique:
                levelResult = UnityEngine.Random.Range(85, 96);
                break;
            case ItemRarity.Legend:
                levelResult = UnityEngine.Random.Range(96, 101);
                break;
        }
    }

    private void AddResult(ItemRelicsSO so, ItemRarity rarity, int level)
    {
        RelicsObject obj = inventoryManager.relicsList.Find(x => x.itemNum == so.itemNum);
        
        if (obj == null)
        {
            OnChooseItem?.Invoke(null, new RelicsObject(so, rarity, level));
            //InventoryManager.Instance.AddItem(so, rarity, level);
        }
        else
        {
            OnChooseItem?.Invoke(obj, new RelicsObject(so, rarity, level));
        }

        //relicsResult = null;
        //rarityResult = ItemRarity.None;
        //level = -1;
    }

}
