using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JHT;

public class GachaItemPanel : MonoBehaviour
{
    [Header("UI Components")]
    public Image itemImage;
    public TextMeshProUGUI nameText;
    public Image backgroundImage;

    [Header("등급별 색상 설정")]
    [Tooltip("PointTier의 Low, Mid, High 순서에 맞게 색상을 지정합니다.")]
    public Color[] tierColors = new Color[3];

    /// <summary>
    /// 가챠 결과로 나온 아이템의 정보를 받아 UI를 설정합니다.
    /// </summary>
    /// <param name="weapon">표시할 아이템 데이터</param>
    /// <param name="tier">함께 획득한 강화 포인트의 등급</param>
    public void SetUp(WeaponObject weapon, PointTier tier)
    {
        if (weapon != null)
        {
            // 아이템 이름과 아이콘 설정
            if (nameText != null) this.nameText.text = weapon.itemName;
            if (itemImage != null) this.itemImage.sprite = weapon.itemIcon;
        }

        // 강화 포인트 등급에 따라 배경색 설정
        if (backgroundImage != null && tierColors.Length > (int)tier)
        {
            backgroundImage.color = tierColors[(int)tier];
        }
    }
}
