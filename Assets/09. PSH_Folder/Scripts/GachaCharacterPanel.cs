using UnityEngine;
using UnityEngine.UI;

public class GachaCharacterPanel : MonoBehaviour
{
    [Header("기본 UI 요소")]
    public Image characterImage;
    public Image[] starImages; // 별 이미지 배열
    public Image bg;

    /// <summary>
    /// 캐릭터 데이터로 이 패널의 UI를 설정합니다.
    /// </summary>
    public void Setup(PlayerCharacterData data)
    {
        // 캐릭터 기본 정보 설정
        characterImage.sprite = data.characterdata.characterSprite;

        // 성급(별) UI 업데이트
        int star = (int)data.characterdata.rarity;
        UpdateStarUI(star);

        // 테두리 색 변경
        switch (star)
        {
            case 1:
                bg.color = Color.white;
                break;
            case 2:
                bg.color = Color.blue;
                break;
            case 3:
                bg.color = Color.yellow;
                break;
            default:
                break;
        }


    }

    /// <summary>
    /// 성급(별) UI를 현재 성급에 맞게 갱신합니다.
    /// </summary>
    private void UpdateStarUI(int currentStars)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].color = (i < currentStars) ? Color.yellow : Color.grey;
        }
    }
}
