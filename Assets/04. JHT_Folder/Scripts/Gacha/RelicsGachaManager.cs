using JHT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicsGachaManager : MonoBehaviour
{
    public ItemRelicsSO relicsResult;
    public ItemRarity rarityResult;
    public int levelResult;

    private List<ItemRelicsSO> gachaData;


    ItemDataManager dataManager;
    [SerializeField] private RelicsGachaLootTable rarityTable;
    [SerializeField] private RelicsGachaLootTable levelTable;
    [SerializeField] private RelicsGachaLootTable specialTable;

    private InventoryManager inventoryManager;

    public Action<RelicsObject, RelicsObject> OnChooseItem;

    public void Start()
    {
        inventoryManager = InventoryManager.Instance;
        dataManager = ItemDataManager.Instance;
        gachaData = new();
        dataManager.OnRelicsDataLoadFinish += DataSetting;
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

    public void GetGachaOneRelicsData(bool value)
    {
        StartCoroutine(GetData(value));
    }

    private IEnumerator GetData(bool value)
    {
        if (value)
        {
            GetRarity();
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
    }

    public void GetSO()
    {
        int randIndex = UnityEngine.Random.Range(0, gachaData.Count);
        ItemRelicsSO so = gachaData[randIndex];

        relicsResult = so;
    }

    public void GetRarity()
    {
        RelicsGacha picked = rarityTable.GetRandomRange();
        rarityResult = picked.rarity;
    }

    public void GetSpecialRarity()
    {
        RelicsGacha picked = specialTable.GetRandomRange();
        rarityResult = picked.rarity;
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
            InventoryManager.Instance.AddItem(so, rarity, level);
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
