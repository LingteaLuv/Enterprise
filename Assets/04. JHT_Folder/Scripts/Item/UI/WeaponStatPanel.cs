using NUnit.Framework.Interfaces;
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
        [SerializeField] private Image starImage;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI upLevelText;
        [SerializeField] private TextMeshProUGUI shieldText;

        public void Init(WeaponObject weapon)
        {
            curWeapon = weapon;
            ItemWeaponSO so = (ItemWeaponSO)curWeapon.itemSO;

            curWeapon.OnUpgrade += UpStarAction;
            curWeapon.OnChangeStar += UpdateStarView;
            curWeapon.OnChangeLevel += UpdateLevelView;
            curWeapon.OnChangePower += UpdatePowerView;
            upGradeButton.onClick.AddListener(() => { curWeapon.OnAddStar?.Invoke(); });

            upGradeButton.interactable = curWeapon.IsUpgrade;
            weaponImage.sprite = so.icon;

            UpdateLevelView(curWeapon.ItemLevel);
            UpdateStarView(curWeapon.ItemStar);
            UpdatePowerView(curWeapon.ItemPower);
        }

        public void UpdateLevelView(int value)
        {
            ItemWeaponSO so = (ItemWeaponSO)curWeapon.itemSO;

            upLevelText.text = $"{value}/{so.maxLevelInCurStar[curWeapon.itemStar]}";
        }

        public void UpdatePowerView(float value)
        {
            ItemWeaponSO so = (ItemWeaponSO)curWeapon.itemSO;

            powerText.text = curWeapon.ItemPower.ToString();
        }

        public void UpdateStarView(int value)
        {
            ItemWeaponSO so = (ItemWeaponSO)curWeapon.itemSO;
            starImage.sprite = so.starImage[value];
        }

        // 내 돈보다 클때 추가해야함
        private void UpStarAction(bool value)
        {
            upGradeButton.interactable = value;
        }

    }
}