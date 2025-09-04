using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GachaProbabilityUI : MonoBehaviour
{
   /* [Header("연결")]
    [Tooltip("확률 정보를 가져올 캐릭터 가챠 매니저")]
    public CharacterGachaManager characterGachaManager;
    [Tooltip("확률 정보 아이템 프리팹 (ProbabilityItemUI 스크립트 필요)")]
    public GameObject probabilityItemPrefab;
    [Tooltip("프리팹이 생성될 부모 트랜스폼")]
    public Transform contentParent;
    [Tooltip("닫기 버튼")]
    public Button closeButton;

    void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }

    void OnEnable()
    {
        PopulateProbabilityTable();
    }

    public void PopulateProbabilityTable()
    {
        if (characterGachaManager == null || probabilityItemPrefab == null || contentParent == null)
        {
            Debug.LogError("GachaProbabilityUI에 필요한 컴포넌트가 연결되지 않았습니다.");
            return;
        }

        // 기존 목록 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 1. 등급별 확률 계산 및 표시
        float totalRarityChance = characterGachaManager.rarityChances.Sum(rc => rc.chance);
        if (totalRarityChance <= 0) return;

        // 모든 캐릭터 목록 가져오기
        var allCharacters = characterGachaManager.allCharacters;

        // 2. 개별 아이템 확률 계산 및 표시
        foreach (var rarityChance in characterGachaManager.rarityChances.OrderByDescending(r => r.rarity))
        {
            // 해당 등급의 캐릭터들
            var charactersInRarity = allCharacters.Where(c => c.rarity == rarityChance.rarity).ToList();
            if (charactersInRarity.Count == 0) continue;

            // 등급 헤더 표시
            InstantiateHeader($"-- {rarityChance.rarity} 등급 (전체 {rarityChance.chance / totalRarityChance:P2}) --");

            // 이 등급 내의 가중치 총합 계산
            float totalWeightInRarity = charactersInRarity.Sum(c => (c.crewRole == CrewRole.Captain ? 1.0f * characterGachaManager.captainProbabilityMultiplier : 1.0f));
            if (totalWeightInRarity <= 0) continue;

            foreach (var character in charactersInRarity)
            {
                // 이 캐릭터의 가중치
                float characterWeight = (character.crewRole == CrewRole.Captain ? 1.0f * characterGachaManager.captainProbabilityMultiplier : 1.0f);

                // [디버그] 계산에 사용된 값을 확인하기 위한 로그입니다.
                Debug.Log($"캐릭터: {character.characterName}, 역할: {character.crewRole}, 선장인가? {character.crewRole == CrewRole.Captain}, 적용된 배수: {characterGachaManager.captainProbabilityMultiplier}, 최종 가중치: {characterWeight}");

                // 최종 확률 계산: (등급 확률) * (등급 내에서의 가중치 비율)
                float finalProbability = (rarityChance.chance / totalRarityChance) * (characterWeight / totalWeightInRarity);

                // UI 아이템 생성 및 설정
                GameObject itemGO = Instantiate(probabilityItemPrefab, contentParent);
                ProbabilityItemUI itemUI = itemGO.GetComponent<ProbabilityItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(character.characterName, character.rarity.ToString(), finalProbability);
                }
            }
        }
    }

    private void InstantiateHeader(string headerText)
    {
        GameObject itemGO = Instantiate(probabilityItemPrefab, contentParent);
        ProbabilityItemUI itemUI = itemGO.GetComponent<ProbabilityItemUI>();
        if (itemUI != null)
        {
            itemUI.SetupAsHeader(headerText);
        }
    }*/
}
