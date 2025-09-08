using UnityEngine;
using UnityEngine.UI;

public class GachaCharacterPanel : MonoBehaviour
{
    [Header("기본 UI 요소")]
    public Image characterImage;
    public Image[] starImages; // 별 이미지 배열
    public Image bg;

    [Header("직업, 속성 아이콘")]
    public Image crewRoleIcon;
    public Image factionIcon;

    /// <summary>
    /// 캐릭터 데이터와 '뽑힌 등급'으로 이 패널의 UI를 설정합니다.
    /// </summary>
    public void Setup(PlayerCharacterData data, GachaGrade grade)
    {
        // 캐릭터 기본 정보 설정
        characterImage.sprite = data.characterdata.characterSprite;

        // '이번에 뽑힌 등급'을 기준으로 별 UI 업데이트
        int star = (int)grade;
        UpdateStarUI(star);

        // '이번에 뽑힌 등급'을 기준으로 테두리 색 변경
        switch (star)
        {
            case 1:
                bg.color = Color.white; // 1성
                break;
            case 2:
                bg.color = Color.blue; // 2성
                break;
            case 3:
                bg.color = Color.magenta; // 3성
                break;
            default:
                bg.color = Color.white; // 예외 처리
                break;
        }

        // 아이콘 업데이트
        UpdateIcon(data);
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
    public void UpdateIcon(PlayerCharacterData data)
    {
        crewRoleIcon.sprite = data.crewRoleIcon;
        factionIcon.sprite = data.factionIcon;
    }
}