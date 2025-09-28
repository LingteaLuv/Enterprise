using JHT;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHT
{
    // 모두 IPointer 인터페이스로
    public class ItemPanelPrefab : MonoBehaviour
    {
        public ItemObject itemObject;
        public string itemName;
        public int itemLevel;

        [Header("UI Components")]
        public Image itemImage;
        public Button itemDetail;
        //public Image[] starImages;
        //public TextMeshProUGUI nameText;
        //public TextMeshProUGUI levelText;

        private WeaponObject weaponObject; // 표시할 무기 데이터
        private WeaponStatPanel weaponStatPanel; // 캐싱하여 사용

        [Header("Current Click Item")]
        public GameObject curClickItem;
        #region Weapon

        public TextMeshProUGUI itemCountText;
        public Image itemStarImage;

        #endregion

        #region Relics
        public Image itemRarityImage;
        #endregion


        public void Init(ItemObject item)
        {
            if (item.itemSO.itemType == ItemType.Equip)
            {
                itemObject = (WeaponObject)item;
                WeaponObject obj = (WeaponObject)itemObject;
                SetWeapon(obj);
                obj.OnChangeLevel += UpCountAction;
                obj.OnChangeStar += UpStar;
                itemDetail.onClick.AddListener(ShowItem);
                UpStar(obj.ItemStar);
            }
        }

        #region Weapon
        public void SetUp(WeaponObject weapon)
        {
            // 이전에 연결된 이벤트가 있다면 해제
            if (weaponObject != null)
            {
                weaponObject.OnChangeLevel -= RefreshUI;
                weaponObject.OnChangeStar -= RefreshUI;
            }

            this.weaponObject = weapon;
            this.itemObject = weapon;

            // 새 무기 데이터의 이벤트에 UI 갱신 함수를 등록
            weaponObject.OnChangeLevel += RefreshUI;
            weaponObject.OnChangeStar += RefreshUI;

            // 상세 정보 보기 버튼 클릭 이벤트 설정 (한 번만 설정)
            itemDetail.onClick.RemoveAllListeners();
            itemDetail.onClick.AddListener(ShowDetails);

            // UI 즉시 갱신
            RefreshUI();
        }
        private void OnEnable()
        {
            ItemEventManager.Instance.OnClickItem += HandleSelected;
        }

        private void OnDisable()
        {
            ItemEventManager.Instance.OnClickItem -= HandleSelected;
        }
        private void RefreshUI(int dummy = 0) // 이벤트 콜백을 위해 파라미터 추가
        {
            if (weaponObject == null) return;

           // nameText.text = weaponObject.itemName;
            if (weaponObject.ItemStar >= 5)
            {
                //levelText.text = "Lv. MAX";
            }
            else
            {
                //levelText.text = "Lv." + weaponObject.ItemLevel;
            }

            if (weaponObject.itemIcon != null)
            {
                itemImage.sprite = weaponObject.itemIcon;
            }

            UpdateStarDisplay(weaponObject.ItemStar);
        }
        private void ShowDetails()
        {
            if (weaponObject == null) return;

            ItemEventManager.Instance.ClickItem(weaponObject);

            Debug.Log(weaponObject.itemName + " (레벨 " + weaponObject.ItemLevel + ") 상세 정보 보기");
            weaponStatPanel = FindAnyObjectByType<WeaponStatPanel>(); 
            if (weaponStatPanel != null)
            {
                weaponStatPanel.ShowStats(weaponObject);
            }
            else
            {
                Debug.LogError("WeaponStatPanel을 찾을 수 없습니다!");
            }
        }

        private void UpdateStarDisplay(int currentStars)
        {
            //if (starImages == null) return;

            //for (int i = 0; i < starImages.Length; i++)
            //{
            //    var img = starImages[i];
            //    if (img == null) continue;  // Destroy되었거나 참조 끊긴 경우 스킵

            //    img.color = (i < currentStars) ? Color.yellow : Color.grey;
            //}
        }
        private void OnDestroy()
        {
            if (weaponObject != null)
            {
                weaponObject.OnChangeLevel -= RefreshUI;
                weaponObject.OnChangeStar -= RefreshUI;
            }
        }
        #endregion

        private void ShowItem()
        {
            ItemEventManager.Instance.ClickItem(itemObject);
        }


        #region Weapon

        private void SetWeapon(WeaponObject item)
        {
            if (item.itemNum != itemObject.itemNum)
                return;

            itemName = item.itemName;
            itemLevel = item.ItemLevel;
            itemImage.sprite = item.itemIcon;
            itemCountText.text = item.ItemLevel.ToString();

        }


        private void UpCountAction(int value)
        {
            WeaponObject obj = (WeaponObject)itemObject;
            itemCountText.text = value.ToString();
        }

        private void UpStar(int value)
        {
            ItemWeaponSO so = (ItemWeaponSO)itemObject.itemSO;
            WeaponObject obj = (WeaponObject)itemObject;
            itemStarImage.sprite = so.starImage[value];
        }
        #endregion
        private void HandleSelected(ItemObject clicked)
        {
            if (clicked == null || clicked is RelicsObject)
                return;

            WeaponObject obj = (WeaponObject)itemObject;
            bool value = ReferenceEquals(clicked, obj)
                        || (clicked != null && clicked.itemNum == obj.itemNum);
            curClickItem.SetActive(value);
        }
    }
}
