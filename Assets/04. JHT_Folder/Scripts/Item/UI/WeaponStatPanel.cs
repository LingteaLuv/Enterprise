using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

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
            weapon.OnChangeStar += UpdateStarview;
            upGradeButton.onClick.AddListener(() => { curWeapon.OnAddStar?.Invoke(); });

            upGradeButton.interactable = curWeapon.IsUpgrade;
            weaponImage.sprite = so.icon;

            UpdateLevelView();
            UpdateStarview(curWeapon.itemStar);
        }

        public void UpdateLevelView()
        {
            ItemWeaponSO so = (ItemWeaponSO)curWeapon.itemSO;

            powerText.text = curWeapon.weaponPower.ToString();
            upLevelText.text = $"{curWeapon.ItemLevel}/{so.maxLevelInCurStar[curWeapon.itemStar]}";
        }

        public void UpdateStarview(int value)
        {
            ItemWeaponSO so = (ItemWeaponSO)curWeapon.itemSO;
            Debug.Log($"1Value : {value}");
            Debug.Log($"1CurValue : {curWeapon.itemStar}");
            starImage.sprite = so.starImage[value];
        }


        // 내 돈보다 클때 추가해야함
        private void UpStarAction(bool value)
        {
            upGradeButton.interactable = value;
        }

    }
}
