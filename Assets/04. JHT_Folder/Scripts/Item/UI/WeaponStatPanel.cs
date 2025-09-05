using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class WeaponStatPanel : MonoBehaviour
    {
        private WeaponObject curWeapon;

        // [SerializeField] private HoldButton levelUpBtn;
        [SerializeField] private Button starUpBtn;
        [SerializeField] private Image weaponImage;
        [SerializeField] private Image[] starImages;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI pointText;
        [SerializeField] private TextMeshProUGUI needPointText;
        [SerializeField] private TextMeshProUGUI levelText; // 레벨 표시용 텍스트
        [SerializeField] private TextMeshProUGUI statText;
        [SerializeField] private TextMeshProUGUI powerText;

        private void Awake()
        {
            // levelUpBtn.onHoldAction.AddListener(OnLevelUpButtonClick);
            starUpBtn.onClick.AddListener(OnStarUpButtonClick);
        }

        private void OnDisable()
        {
            // 패널이 비활성화될 때 이벤트 리스너 정리
            if (curWeapon != null)
            {
                curWeapon.OnChangeLevel -= OnWeaponDataChanged;
                curWeapon.OnChangeStar -= OnWeaponDataChanged;
                if(InventoryManager.Instance != null)
                    InventoryManager.Instance.OnEquipmentEnhancementPointsChanged -= OnEnhancementPointsChanged;
            }
        }

        public void ShowStats(WeaponObject weapon)
        {
            // 이전에 등록된 이벤트가 있다면 해제
            if (curWeapon != null)
            {
                curWeapon.OnChangeLevel -= OnWeaponDataChanged;
                curWeapon.OnChangeStar -= OnWeaponDataChanged;
                InventoryManager.Instance.OnEquipmentEnhancementPointsChanged -= OnEnhancementPointsChanged;
            }

            curWeapon = weapon;

            if (curWeapon == null)
            {
                // 무기 정보가 없으면 패널 비활성화 또는 초기 상태로
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // 새 무기의 데이터 변경 이벤트에 UI 업데이트 함수 연결
            curWeapon.OnChangeLevel += OnWeaponDataChanged;
            curWeapon.OnChangeStar += OnWeaponDataChanged;
            // 강화 포인트 변경 이벤트에 UI 업데이트 함수 연결
            InventoryManager.Instance.OnEquipmentEnhancementPointsChanged += OnEnhancementPointsChanged;

            UpdateUI();
        }

        private void OnLevelUpButtonClick()
        {
            if (curWeapon == null) return;
            InventoryManager.Instance.LevelUpEquipment(curWeapon.itemNum);
        }

        private void OnStarUpButtonClick()
        {
            if (curWeapon == null) return;
            InventoryManager.Instance.StarUpEquipment(curWeapon.itemNum);
        }

        private void OnWeaponDataChanged(int value)
        {
            UpdateUI();
        }

        private void OnEnhancementPointsChanged(int itemNum, int points)
        {
            // 현재 보고있는 무기의 강화포인트가 변경되었을 때만 UI 갱신
            if (curWeapon != null && curWeapon.itemNum == itemNum)
            {
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (curWeapon == null) return;
            
            int requiredPoints = InventoryManager.Instance.GetRequiredPointsForLevelUp(curWeapon);

            // 기본 정보 업데이트
            weaponImage.sprite = curWeapon.itemIcon;
            nameText.text = curWeapon.itemName;
            pointText.text = InventoryManager.Instance.GetEnhancementPoints(curWeapon.itemNum).ToString();
            needPointText.text = requiredPoints.ToString();
            levelText.text = $"Lv. {curWeapon.ItemLevel}";
            switch (curWeapon.statType)
            {
                case Stat.Attack:
                    statText.text = "공격력";
                    break;

                case Stat.Defense:
                    statText.text = "방어력";
                    break;

                case Stat.Health:
                    statText.text = "체력";
                    break;

                default:
                    statText.text = "알 수 없는 스탯";
                    break;
            }


            // 스탯 계산 및 표시
            float calculatedStat = InventoryManager.Instance.GetWeaponStat(curWeapon.itemNum);
            powerText.text = $"{calculatedStat:F1}% 증가";

            // 별 UI 업데이트
            UpdateStarDisplay(curWeapon.ItemStar);

            // 성급업 가능 여부 먼저 계산
            bool isMaxStars = curWeapon.ItemStar >= 5;
            bool needsStarUp = curWeapon.ItemLevel > 0 && curWeapon.ItemLevel % 10 == 0 && curWeapon.ItemStar < (curWeapon.ItemLevel / 10);
            bool canStarUp = !isMaxStars && needsStarUp;
            starUpBtn.interactable = canStarUp;

            // 레벨업 버튼 활성화 로직 (성급업이 가능하지 않을 때만 활성화)
            bool hasEnoughPoints = InventoryManager.Instance.GetEnhancementPoints(curWeapon.itemNum) >= requiredPoints;
            bool isMaxLevel = curWeapon.ItemLevel >= 50;
            // levelUpBtn.interactable = hasEnoughPoints && !isMaxLevel && !canStarUp;
        }

        private void UpdateStarDisplay(int currentStars)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].color = (i < currentStars) ? Color.yellow : Color.grey;
            }
        }
    }
}