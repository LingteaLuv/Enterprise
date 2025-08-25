using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class WeaponStatPanel : MonoBehaviour
    {
        private WeaponObject curWeapon;

        [SerializeField] private Button upGradeButton;
        [SerializeField] private Image weaponImage;
        [SerializeField] private Image starImage; // 별을 표시할 이미지
        [SerializeField] private TextMeshProUGUI powerText; // 주 스탯 (공격력 등)
        [SerializeField] private TextMeshProUGUI upLevelText; // 레벨
        [SerializeField] private TextMeshProUGUI shieldText; // 보조 스탯 (방어력 등)

        /// <summary>
        /// 외부에서 호출하여 무기 정보로 UI를 채우고 패널을 활성화하는 함수
        /// </summary>
        public void ShowStats(WeaponObject weapon)
        {
            this.curWeapon = weapon;

            // --- 1. UI 내용 채우기 ---
            if (weapon.itemIcon != null)
            {
                weaponImage.sprite = weapon.itemIcon;
            }
            upLevelText.text = "Lv." + weapon.ItemLevel;

            // 별 등급 표시 (Rarity나 ItemStar 값에 따라 이미지 변경)
            // 예: starImage.sprite = starSprites[weapon.ItemStar];
            Debug.Log("별 표시 로직 필요: " + weapon.ItemStar + "개");

            // 스탯 정보 표시
            // 실제 스탯 값(공격력 등)은 ItemWeaponSO나 WeaponObject에 변수로 존재해야 합니다.
            if (weapon.itemSO is ItemWeaponSO weaponSO)
            {
                // 현재는 스탯 값이 없으므로 statType을 대신 표시합니다.
                // 추후 weaponSO.attackPower 같은 변수를 만들어 연결해야 합니다.
                powerText.text = "주요 능력치: " + weaponSO.statType;
                shieldText.text = ""; // 보조 스탯이 있다면 여기에 표시
            }

            // --- 2. 버튼 이벤트 연결 ---
            upGradeButton.onClick.RemoveAllListeners();
            upGradeButton.onClick.AddListener(OnUpgradeButtonClick);

            // --- 4. 패널 활성화 ---
            gameObject.SetActive(true);
        }

        // 업그레이드 버튼 클릭 시 호출될 함수
        private void OnUpgradeButtonClick()
        {
            Debug.Log(curWeapon.itemName + " 업그레이드를 시도합니다.");
            // 여기에 InventoryManager 등을 통해 실제 무기 업그레이드를 처리하는 로직을 호출합니다.
            // 예: InventoryManager.Instance.UpgradeWeapon(curWeapon);
        }

        // 패널을 닫는 함수
        public void ClosePanel()
        {
            // 패널이 닫힐 때, 메모리 누수 방지를 위해 구독했던 이벤트를 해제합니다.
            if (curWeapon != null)
            {
                curWeapon.OnUpgrade -= UpStarAction;
            }
            gameObject.SetActive(false);
        }

        // 업그레이드 가능 여부에 따라 버튼을 켜고 끄는 함수
        private void UpStarAction(bool isPossible)
        {
            upGradeButton.interactable = isPossible;
        }
    }
}