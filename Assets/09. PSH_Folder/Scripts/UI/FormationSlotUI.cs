using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormationSlotUI : MonoBehaviour
{
    public CrewRole assignedPosition; // 이 슬롯이 담당하는 포지션 (Deckhand, Sailor 등)
    public Image[] characterImages; // 캐릭터 이미지를 표시할 Image 컴포넌트 배열 (최대 2개)
    public RectTransform[] characterImageRects; // 캐릭터 이미지의 RectTransform 배열 (위치 조정을 위해)

    void Awake()
    {
        // characterImages 배열의 크기에 맞춰 characterImageRects 배열을 초기화합니다.
        characterImageRects = new RectTransform[characterImages.Length];
        for (int i = 0; i < characterImages.Length; i++)
        {
            if (characterImages[i] != null)
            {
                characterImageRects[i] = characterImages[i].GetComponent<RectTransform>();
            }
        }
    }

    /// <summary>
    /// 이 슬롯에 캐릭터 데이터를 설정하여 UI를 갱신합니다.
    /// </summary>
    /// <param name="charactersInSlot">이 포지션에 배치된 캐릭터 리스트</param>
    public void Setup(List<PlayerCharacterData> charactersInSlot)
    {
        Debug.Log($"[FSU] Setup 호출됨 (포지션: {assignedPosition}, 캐릭터 수: {charactersInSlot.Count})");
        // 모든 이미지와 텍스트를 초기화 (비활성화)
        foreach (var img in characterImages)
        {
            img.gameObject.SetActive(false);
        }

        // 캐릭터 수에 따라 UI 업데이트
        for (int i = 0; i < charactersInSlot.Count; i++)
        {
            if (i < characterImages.Length)
            {
                characterImages[i].sprite = charactersInSlot[i].characterdata.characterSprite;
                characterImages[i].gameObject.SetActive(true);
            }
        }

        // 시각적 배치 조정
        if (charactersInSlot.Count == 1)
        {
            // 1명일 때: 첫 번째 캐릭터를 중앙에 배치
            if (characterImageRects.Length > 0)
            {
                characterImageRects[0].anchoredPosition = Vector2.zero; // 중앙 (0,0)으로 설정
                // 필요하다면 크기 조정
                // characterImageRects[0].sizeDelta = new Vector2(originalWidth, originalHeight);
            }
        }
        else if (charactersInSlot.Count == 2)
        {
            // 2명일 때: 두 캐릭터를 나란히 배치 (예시: 왼쪽, 오른쪽으로 이동)
            // 이 위치 값은 UI 디자인에 따라 조절해야 합니다.
            if (characterImageRects.Length >= 2)
            {
                characterImageRects[0].anchoredPosition = new Vector2(-25f, 50f);
                characterImageRects[1].anchoredPosition = new Vector2(25f, -50f);
                // 필요하다면 크기 조정
                // characterImageRects[0].sizeDelta = new Vector2(smallerWidth, smallerHeight);
                // characterImageRects[1].sizeDelta = new Vector2(smallerWidth, smallerHeight);
            }
        }
    }
}