using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class ItemPanelPrefab : MonoBehaviour
    {
        [Header("UI Components")]
        public Image itemImage;
        public Button itemDetail;
        public Image[] starImages;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;

        private WeaponObject weaponObject; // 표시할 무기 데이터
        private WeaponStatPanel weaponStatPanel; // 캐싱하여 사용

        private void Awake()
        {
            // StatPanel은 비활성화 상태일 수 있으므로 Inactive 포함해서 찾음
            weaponStatPanel = FindAnyObjectByType<WeaponStatPanel>(FindObjectsInactive.Include);
        }

        private void OnDisable()
        {
            // 패널이 비활성화될 때 (풀에 반환될 때) 이벤트 구독 해제
            if (weaponObject != null)
            {
                weaponObject.OnChangeLevel -= RefreshUI;
                weaponObject.OnChangeStar -= RefreshUI;
            }
        }

        public void SetUp(WeaponObject weapon)
        {
            // 이전에 연결된 이벤트가 있다면 해제
            if (weaponObject != null)
            {
                weaponObject.OnChangeLevel -= RefreshUI;
                weaponObject.OnChangeStar -= RefreshUI;
            }

            this.weaponObject = weapon;

            // 새 무기 데이터의 이벤트에 UI 갱신 함수를 등록
            weaponObject.OnChangeLevel += RefreshUI;
            weaponObject.OnChangeStar += RefreshUI;

            // 상세 정보 보기 버튼 클릭 이벤트 설정 (한 번만 설정)
            itemDetail.onClick.RemoveAllListeners();
            itemDetail.onClick.AddListener(ShowDetails);

            // UI 즉시 갱신
            RefreshUI();
        }

        private void RefreshUI(int dummy = 0) // 이벤트 콜백을 위해 파라미터 추가
        {
            if (weaponObject == null) return;

            nameText.text = weaponObject.itemName;
            levelText.text = "Lv." + weaponObject.ItemLevel;

            if (weaponObject.itemIcon != null)
            {
                itemImage.sprite = weaponObject.itemIcon;
            }

            UpdateStarDisplay(weaponObject.ItemStar);
        }

        private void ShowDetails()
        {
            if (weaponObject == null) return;

            Debug.Log(weaponObject.itemName + " (레벨 " + weaponObject.ItemLevel + ") 상세 정보 보기");
            if (weaponStatPanel != null)
            {
                weaponStatPanel.gameObject.SetActive(true);
                weaponStatPanel.ShowStats(weaponObject);
            }
            else
            {
                Debug.LogError("WeaponStatPanel을 찾을 수 없습니다!");
            }
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