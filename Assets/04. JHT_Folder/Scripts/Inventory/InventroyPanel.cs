using JHT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class InventroyPanel : MonoBehaviour
    {
        [SerializeField] private Transform itemPanelParent;
        [SerializeField] private ItemPanelPrefab itemPanelPrefab;
        [SerializeField] private Button levelSort;

        [SerializeField] private GameObject weaponInventory;
        [SerializeField] private GameObject relicsInventory;

        [SerializeField] private Button weaponInventoryButton;
        [SerializeField] private Button relicsInventoryButton;

        [SerializeField] private WeaponStatPanel weaponStatPanel;

        private InventoryManager inventoryManager;
        private bool isLevelSort;

        private List<ItemPanelPrefab> itemPanelPool = new List<ItemPanelPrefab>();

        private void OnEnable()
        {
            inventoryManager = InventoryManager.Instance;
            inventoryManager.OnAddInventory += ReSetItemPanel;
            inventoryManager.OnRemoveInventory += ReSetItemPanel;
            levelSort.onClick.AddListener(SortByLevel);

            weaponInventoryButton.onClick.AddListener(ChangeWeaponMode);
            relicsInventoryButton.onClick.AddListener(ChangeRelicsMode);
            ItemEventManager.Instance.OnClickItem += ShowWeaponStat;

            ReSetItemPanel(null);
        }

        private void OnDisable()
        {
            inventoryManager.OnAddInventory -= ReSetItemPanel;
            inventoryManager.OnRemoveInventory -= ReSetItemPanel;
            levelSort.onClick.RemoveListener(SortByLevel);

            weaponInventoryButton.onClick.RemoveListener(ChangeWeaponMode);
            relicsInventoryButton.onClick.RemoveListener(ChangeRelicsMode);
            ItemEventManager.Instance.OnClickItem -= ShowWeaponStat;
        }

        private void SortByLevel()
        {
            isLevelSort = !isLevelSort;
            inventoryManager.WeaponLevelSort(isLevelSort);
            ReSetItemPanel(null);
        }

        private void ShowWeaponStat(ItemObject obj)
        {
            if (obj is WeaponObject)
            {
                WeaponObject inst = (WeaponObject)InventoryManager.Instance.GetItemData(obj);

                if (!weaponStatPanel.gameObject.activeSelf)
                    weaponStatPanel.gameObject.SetActive(true);

                weaponStatPanel.ShowStats(inst);
            }

        }

        private void ReSetItemPanel(ItemObject changedItem)
        {
            // 1. 모든 풀링된 패널을 비활성화
            foreach (ItemPanelPrefab panel in itemPanelPool)
            {
                panel.gameObject.SetActive(false);
            }

            // 2. 인벤토리 리스트를 기반으로 패널을 재사용/생성하여 업데이트
            for (int i = 0; i < inventoryManager.weaponList.Count; i++)
            {
                ItemPanelPrefab currentPanel;
                if (i < itemPanelPool.Count)
                {
                    // 풀에 사용 가능한 패널이 있으면 재사용
                    currentPanel = itemPanelPool[i];
                }
                else
                {
                    // 풀이 부족하면 새로 생성하고 풀에 추가
                    currentPanel = Instantiate(itemPanelPrefab, itemPanelParent);
                    itemPanelPool.Add(currentPanel);
                }

                WeaponObject weapon = inventoryManager.weaponList[i];
                currentPanel.SetUp(weapon);
                currentPanel.gameObject.SetActive(true);
            }
        }


        private void ChangeWeaponMode()
        {
            InventoryManager.Instance.currentMode = InventoryMode.Weapon;
            if (!weaponInventory.activeSelf)
                weaponInventory.SetActive(true);

            if (weaponInventory.activeSelf)
                relicsInventory.SetActive(false);
        }

        private void ChangeRelicsMode()
        {
            InventoryManager.Instance.currentMode = InventoryMode.Relics;
            if (weaponInventory.activeSelf)
                weaponInventory.SetActive(false);

            if (!weaponInventory.activeSelf)
                relicsInventory.SetActive(true);
        }
    }
}
