using JHT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventroyPanel : MonoBehaviour
{
    [SerializeField] private Transform itemPanelParent;
    [SerializeField] private ItemPanelPrefab itemPanelPrefab;
    [SerializeField] private Button levelSort;

    private InventoryManager inventoryManager;
    private bool isLevelSort;

    private List<ItemPanelPrefab> items;
    private void OnEnable()
    {
        inventoryManager = InventoryManager.Instance;
        inventoryManager.OnAddInventory += AddItem;
        inventoryManager.OnRemoveInventory += RemoveItem;
        levelSort.onClick.AddListener(() => { isLevelSort = !isLevelSort; inventoryManager.ItemLevelSort(isLevelSort); ReSetItemPanel(); });
    }

    private void OnDisable()
    {
        inventoryManager.OnAddInventory -= AddItem;
        inventoryManager.OnRemoveInventory -= RemoveItem;
        levelSort.onClick.RemoveListener(() => { isLevelSort = !isLevelSort; inventoryManager.ItemLevelSort(isLevelSort); ReSetItemPanel(); });
    }

    private void Start()
    {
        items = new();
    }

    private void AddItem(ItemObject item)
    {
        ItemPanelPrefab obj = Instantiate(itemPanelPrefab);
        obj.transform.SetParent(itemPanelParent);
        obj.Init(item);
        items.Add(obj);
    }

    private void RemoveItem(ItemObject item)
    {
        int idx = items.FindIndex(p => p != null && p.itemObject == item);
        if (idx < 0) return;

        var panel = items[idx];
        items.RemoveAt(idx);
        if (panel != null) Destroy(panel.gameObject);
    }


    private void ReSetItemPanel()
    {
        foreach (Transform child in itemPanelParent)
        {
            Destroy(child.gameObject);
            items.Clear();
        }

        for (int i = 0; i < inventoryManager.itemList.Count; i++)
        {
            ItemPanelPrefab obj = Instantiate(itemPanelPrefab);
            obj.transform.SetParent(itemPanelParent);
            obj.Init(inventoryManager.itemList[i]);
            items.Add(obj);
        }
    }
}
