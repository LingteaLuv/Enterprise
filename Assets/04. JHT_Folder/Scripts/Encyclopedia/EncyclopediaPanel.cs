using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace JHT
{
    public class EncyclopediaPanel : MonoBehaviour
    {
        // 모든 아이템 데이터 받아오기
        public Dictionary<string, ItemWeaponSO> weaponDic;
        public Dictionary<string, ItemRelicsSO> relicsDic;

        // 해제한 아이템 정보 저장
        public Dictionary<string, ItemWeaponSO> getWeaponDic;
        public Dictionary<string, ItemRelicsSO> getRelicsDic;

        [SerializeField] private EncyclopediaPanelItem encyclopediaPanelItem;

        // UI편성
        //[SerializeField] private Button encyclopediaButton;
        [SerializeField] private GameObject outPanel;
        [SerializeField] private GameObject encyclopediaList;
        [SerializeField] private Transform encyclopediaPanelWeaponParent;
        [SerializeField] private Transform encyclopediaPanelRelicsParent;

        public Action<ItemWeaponSO> OnGetWeapon;
        public Action<ItemRelicsSO> OnGetRelics;
        public Action OnOutPanel;

        InventoryManager inventoryManager;

        private void Awake()
        {
            weaponDic = new();
            relicsDic = new();

            getWeaponDic = new();
            getRelicsDic = new();
        }

        private void Start()
        {
            ItemDataManager.Instance.OnWeaponInit -= WeaponInit;
            ItemDataManager.Instance.OnWeaponInit += WeaponInit;
            ItemDataManager.Instance.OnRelicInit -= RelicsInit;
            ItemDataManager.Instance.OnRelicInit += RelicsInit;
            gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            OnOutPanel += OutPanel;
            inventoryManager = InventoryManager.Instance;
            
            inventoryManager.OnAddInventory += GetItem;
            //encyclopediaButton.onClick.AddListener(OpenPanel);
        }

        private void OnDisable()
        {
            OnOutPanel -= OutPanel;

            inventoryManager.OnAddInventory -= GetItem;
            //encyclopediaButton.onClick.RemoveListener(OpenPanel);
        }

        public void WeaponInit()
        {
            weaponDic.Clear();
            foreach (var k in ItemDataManager.Instance.GetAllWeaponData())
                weaponDic[k.Key] = k.Value;


            ShowAllWeapon();
        }

        public void RelicsInit()
        {
            relicsDic.Clear();
            foreach (var k in ItemDataManager.Instance.GetAllRelicsData())
                relicsDic[k.Key] = k.Value;

            ShowAllRelics();
        }

        private void ShowAllWeapon()
        {
            foreach (var n in ItemDataManager.Instance.GetAllWeaponData())
            {
                EncyclopediaPanelItem obj = Instantiate(encyclopediaPanelItem);
                obj.transform.SetParent(encyclopediaPanelWeaponParent);
                obj.Init(n.Value);
            }
        }

        private void ShowAllRelics()
        {
            foreach (var n in ItemDataManager.Instance.GetAllRelicsData())
            {
                EncyclopediaPanelItem obj = Instantiate(encyclopediaPanelItem);
                obj.transform.SetParent(encyclopediaPanelRelicsParent);
                obj.Init(n.Value);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (encyclopediaList.activeSelf)
            {
                OnOutPanel?.Invoke();
            }
        }

        private void GetItem(ItemObject obj)
        {
            //필요시 인벤토리에 저장해서 해당 데이터가 존재할경우에만 인게임에서 사용가능하도록 설정
            if (obj is WeaponObject)
            {
                getWeaponDic.Add(obj.itemName, (ItemWeaponSO)obj.itemSO);
            }
            else
            {
                getRelicsDic.Add(obj.itemName, (ItemRelicsSO)obj.itemSO);
            }
        }

        private void OpenPanel()
        {
            encyclopediaList.SetActive(true);
            outPanel.SetActive(true);
        }

        private void OutPanel()
        {
            encyclopediaList.SetActive(false);
            outPanel.SetActive(false);
        }
    }
}
