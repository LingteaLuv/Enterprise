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

    /// <summary>
    /// 가챠 결과로 나온 아이템의 정보를 받아 UI를 설정합니다.
    /// </summary>
    /// <param name="weapon">표시할 아이템 데이터</param>
    /// <param name="enhancementPoints">함께 획득한 강화 포인트</param>
    public void SetUp(WeaponObject weapon, int enhancementPoints)
    {
        if (weapon != null)
        {
            // 아이템 이름과 아이콘 설정
            if (nameText != null) this.nameText.text = weapon.itemName;
            if (itemImage != null) this.itemImage.sprite = weapon.itemIcon;
        }

        // 강화 포인트에 따라 배경색 설정
        SetBackgroundColor(enhancementPoints);
    }

    /// <summary>
    /// 강화 포인트에 따라 배경색을 변경합니다.
    /// </summary>
    private void SetBackgroundColor(int points)
    {
        if (backgroundImage == null) return;

        // 강화 포인트에 따라 색상 결정 (수치는 임의로 정함, 필요시 수정)
        if (points == 100)
        {
            backgroundImage.color = Color.yellow;
        }
        else if (points == 50)
        {
            backgroundImage.color = Color.blue;
        }
        else
        {
            backgroundImage.color = Color.white;
        }
    }
}