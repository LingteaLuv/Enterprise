using JHT;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHT // 동일한 네임스페이스 사용
{
    public class ItemEquipPanel : MonoBehaviour
    {
        [Header("UI Components")]
        public Image itemImage;
        public Button itemDetailButton; // 명확성을 위해 이름 변경
        public Image[] starImages;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;

        [Header("Equipped Status")]
        [SerializeField] private GameObject equippedByPanel;
        [SerializeField] private TextMeshProUGUI equippedByText;

        private WeaponObject weaponObject; // 이 패널이 나타내는 무기
        public event Action<WeaponObject> OnPanelClicked;

        public void SetUp(WeaponObject weapon)
        {
            // 이전 무기의 이벤트가 있다면 구독 해제
            if (this.weaponObject != null)
            {
                this.weaponObject.OnChangeLevel -= RefreshUI;
                this.weaponObject.OnChangeStar -= RefreshUI;
            }

            this.weaponObject = weapon;

            // 새 무기 데이터의 이벤트에 UI 갱신 함수를 등록
            this.weaponObject.OnChangeLevel += RefreshUI;
            this.weaponObject.OnChangeStar += RefreshUI;

            // 클릭 리스너가 이벤트를 발생시키도록 설정
            itemDetailButton.onClick.RemoveAllListeners();
            itemDetailButton.onClick.AddListener(() => {
                OnPanelClicked?.Invoke(this.weaponObject);
            });

            RefreshUI();
            UpdateEquippedStatus();
        }

        private void RefreshUI(int dummy = 0)
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

        private void UpdateStarDisplay(int currentStars)
        {
            if (starImages == null) return;

            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                starImages[i].color = (i < currentStars) ? Color.white : Color.black;
            }
        }

        private void UpdateEquippedStatus()
        {
            if (equippedByPanel == null) return;

            if (!string.IsNullOrEmpty(weaponObject.EquippedByCharacterId.Value))
            {
                equippedByPanel.SetActive(true);
                if (int.TryParse(weaponObject.EquippedByCharacterId.Value, out int charId) && PlayerDataManager.Instance != null)
                {
                    if (PlayerDataManager.Instance.OwnedCharacters.TryGetValue(charId, out PlayerCharacterData owner))
                    {
                        equippedByText.text = $"{owner.characterdata.characterName}이/가\n장착 중입니다.";
                    }
                    else
                    {
                        equippedByText.text = "장착중 (정보 없음)";
                    }
                }
                else
                {
                    equippedByText.text = "장착중";
                }
            }
            else
            {
                equippedByPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 정리
            if (this.weaponObject != null)
            {
                this.weaponObject.OnChangeLevel -= RefreshUI;
                this.weaponObject.OnChangeStar -= RefreshUI;
            }
        }
    }
}