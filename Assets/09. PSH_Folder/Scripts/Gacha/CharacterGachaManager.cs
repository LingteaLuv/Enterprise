using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JHT;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterGachaManager : BaseGachaManager<PlayerCharacterData>
{
    [Header("캐릭터 데이터 (전용)")]
    [Tooltip("캐릭터 SO 에셋들이 저장된 폴더 경로")]
    public string characterDataFolderPath = "CharacterData";
    [Tooltip("게임에 존재하는 모든 캐릭터 SO 목록")]
    public List<CharacterData> allCharacters;

    [Header("캐릭터 확률 조정 (전용)")]
    [Tooltip("선장(Captain) 역할 캐릭터의 기본 확률(1)에 곱해지는 값. 0.5로 설정하면 선장의 등장 확률이 절반이 됩니다.")]
    public float captainProbabilityMultiplier = 1.0f;

    // 등급별로 캐릭터를 미리 분류해 놓은 딕셔너리
    private Dictionary<Rarity, List<CharacterData>> charactersByRarity;

    private void Start()
    {
        StartCoroutine(InitializeGachaPool());
    }

    private System.Collections.IEnumerator InitializeGachaPool()
    {
        while (ItemDataManager.Instance == null || !ItemDataManager.Instance.IsDataLoaded)
        {
            yield return null;
        }
        allCharacters = Resources.LoadAll<CharacterData>(characterDataFolderPath).ToList();
        Debug.Log($"[CharacterGachaManager] {allCharacters.Count}명의 캐릭터를 뽑기 풀에 추가했습니다.");

        InitializeCharacterDictionary();
    }

    private void InitializeCharacterDictionary()
    {
        charactersByRarity = new Dictionary<Rarity, List<CharacterData>>();
        foreach (Rarity r in System.Enum.GetValues(typeof(Rarity)))
        {
            var charsOfRarity = allCharacters.Where(c => c.rarity == r).ToList();
            charactersByRarity.Add(r, charsOfRarity);
        }
    }

    /// <summary>
    /// BaseGachaManager의 추상 메소드를 구현합니다.
    /// 캐릭터 데이터(SO)를 뽑고, 이를 플레이어 데이터로 변환하여 반환합니다.
    /// </summary>
    protected override PlayerCharacterData DrawItem(Rarity rarity)
    {
        List<CharacterData> availableCharacters = charactersByRarity[rarity];

        if (availableCharacters == null || availableCharacters.Count == 0)
        {
            Debug.LogWarning($"{rarity} 등급의 캐릭터가 목록에 없습니다!");
            return null;
        }

        // 가중치 기반으로 캐릭터 SO 뽑기
        CharacterData drawnCharacterSO = GetWeightedCharacter(availableCharacters);
        if (drawnCharacterSO == null) return null;

        // 뽑은 SO를 기반으로 실제 플레이어 캐릭터 데이터 생성 및 추가
        PlayerCharacterData newCharacterInstance = PlayerDataManager.Instance.AddCharacter(drawnCharacterSO);

        Debug.Log($"캐릭터 뽑기 결과: [{newCharacterInstance.characterdata.rarity}] {newCharacterInstance.characterdata.characterName}");
        return newCharacterInstance;
    }

    /// <summary>
    /// 역할(CrewRole)에 따라 가중치를 적용하여 캐릭터를 뽑는 내부 함수
    /// </summary>
    private CharacterData GetWeightedCharacter(List<CharacterData> characters)
    {
        var weightedCharacters = characters.Select(c => new {
            Character = c,
            Weight = (c.crewRole == CrewRole.Captain ? 1.0f * captainProbabilityMultiplier : 1.0f)
        }).Where(c => c.Weight > 0).ToList();

        float totalWeight = weightedCharacters.Sum(c => c.Weight);

        if (totalWeight <= 0)
        {
            Debug.LogWarning("뽑을 수 있는(가중치가 0보다 큰) 캐릭터가 없습니다.");
            return characters.FirstOrDefault(); // 비중이 없으면 그냥 첫번째 캐릭터 반환
        }

        float randomPoint = Random.Range(0, totalWeight);

        foreach (var entry in weightedCharacters)
        {
            if (randomPoint < entry.Weight)
            {
                return entry.Character;
            }
            else
            {
                randomPoint -= entry.Weight;
            }
        }
        return weightedCharacters.Last().Character; // 예외 상황 방지
    }
}
