using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JHT
{
    public class ItemPanelPrefab : MonoBehaviour
    {
        // --- Inspector에서 연결할 UI 컴포넌트들 ---
        [Header("UI Components")]
        public Image itemImage;
        public Button itemDetail;
        public Image[] starImages; // 별 이미지 배열
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;


        // --- 데이터 ---
        private ItemObject itemObject; // 원본 데이터를 저장해 둘 변수

        private WeaponStatPanel weaponStatPanel;

        /// <summary>
        /// WeaponObject를 받아 UI 내용을 채우는 함수
        /// </summary>
        public void SetUp(WeaponObject weapon)
        {
            this.itemObject = weapon;

            this.nameText.text = weapon.itemName;
            this.levelText.text = "Lv." + weapon.ItemLevel;

            if (weapon.itemIcon != null)
            {
                this.itemImage.sprite = weapon.itemIcon;
            }

            UpdateStarDisplay(weapon.ItemStar);

            // 상세 정보 보기 버튼 클릭 이벤트 설정
            itemDetail.onClick.RemoveAllListeners();
            itemDetail.onClick.AddListener(() => {
                Debug.Log(weapon.itemName + " (레벨 " + weapon.ItemLevel + ") 상세 정보 보기");
                weaponStatPanel = FindAnyObjectByType<WeaponStatPanel>(FindObjectsInactive.Include)?.gameObject.GetComponent<WeaponStatPanel>();
                weaponStatPanel.ShowStats(weapon);
            });
        }

        // 별 UI를 업데이트하는 함수
        private void UpdateStarDisplay(int currentStars)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                // 현재 별 개수보다 인덱스가 작으면 노란색, 아니면 회색으로 표시
                starImages[i].color = (i < currentStars) ? Color.yellow : Color.grey;
            }
        }
    }
}