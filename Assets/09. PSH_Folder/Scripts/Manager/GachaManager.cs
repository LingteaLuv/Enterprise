using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Numerics;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GachaManager : MonoBehaviour
{
    [System.Serializable]
    public class RarityChance
    {
        public Rarity rarity;
        [Range(0, 100)]
        public float chance; // 등급별 확률
    }

    [Header("UI 연결")]
    public CharacterScrollViewUI characterScrollViewUI; // 캐릭터 목록 UI

    [Header("가챠 확률 설정")]
    public List<RarityChance> rarityChances;

    [Header("역할별 확률 조정")]
    [Tooltip("선장(Captain) 역할 캐릭터의 기본 확률(1)에 곱해지는 값입니다. 0.5로 설정하면 선장의 등장 확률이 절반이 됩니다.")]
    public float captainProbabilityMultiplier = 1.0f;


    [Header("캐릭터 SO 자동 로드")]
    [Tooltip("캐릭터 SO 에셋들이 저장된 폴더 경로입니다.")]
    public string characterDataFolderPath = "Assets/Resources/CharacterData"; // 기본 경로 예시

    [Header("전체 캐릭터 목록 (점3개 메뉴 클릭해서 불러오기)")]
    public List<CharacterData> allCharacters; // 모든 캐릭터 ScriptableObject를 여기에 연결

    // 가장 마지막에 실행된 가챠 결과 리스트
    public List<PlayerCharacterData> LastGachaResults { get; private set; }

    // 등급별로 캐릭터를 미리 분류해 놓은 딕셔너리. 가챠 실행 속도를 높여줍니다.
    private Dictionary<Rarity, List<CharacterData>> charactersByRarity;

#if UNITY_EDITOR
    [ContextMenu("폴더에서 캐릭터 SO 모두 불러오기")]
    private void LoadAllCharacterSOsFromFolder()
    {
        if (string.IsNullOrEmpty(characterDataFolderPath))
        {
            Debug.LogError("캐릭터 SO 폴더 경로가 지정되지 않았습니다!");
            return;
        }

        // 기존 목록을 비웁니다.
        allCharacters.Clear();

        // 지정된 폴더 내에서 CharacterData 타입의 모든 에셋을 찾습니다.
        string[] guids = AssetDatabase.FindAssets("t:CharacterData", new[] { characterDataFolderPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            CharacterData character = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
            if (character != null)
            {
                allCharacters.Add(character);
            }
        }

        // 변경사항을 저장하도록 에디터에 알립니다.
        EditorUtility.SetDirty(this);
        Debug.Log($"{allCharacters.Count}개의 캐릭터 SO를 폴더에서 성공적으로 불러왔습니다.");
    }
#endif

    void Awake()
    {
        // 게임이 시작될 때, 모든 캐릭터를 등급별로 미리 분류해서 딕셔너리에 저장합니다.
        charactersByRarity = new Dictionary<Rarity, List<CharacterData>>();

        // Enum.GetValues를 사용해 모든 Rarity 값을 순회합니다.
        foreach (Rarity r in System.Enum.GetValues(typeof(Rarity)))
        {
            // 해당 등급(r)에 맞는 캐릭터들만 allCharacters 리스트에서 찾아서 리스트로 만듭니다.
            var charsOfRarity = allCharacters.Where(c => c.rarity == r).ToList();
            // 딕셔너리에 추가합니다. (예: key=Rarity.S, value=[S등급 캐릭터1, S등급 캐릭터2...])
            charactersByRarity.Add(r, charsOfRarity);
        }
    }

    /// <summary>
    /// 가챠를 1회 실행하고 결과를 플레이어 데이터에 적용한 후 UI를 갱신합니다.
    /// </summary>
    public bool PerformSingleGacha()
    {
        // 1. 재화 소모 시도
        BigInteger cost = 100;
        if (!CurrencyManager.Instance.SpendCurrency(CurrencyType.Gem, cost))
        {
            Debug.Log("가챠 실패: 재화(Gem)가 부족합니다.");
            return false; // 재화가 부족하면 함수 종료
        }

        // 2. 재화 소모 성공 시, 가챠 실행
        CharacterData drawnCharacterSO = DrawCharacter();
        PlayerCharacterData newCharacterInstance = PlayerDataManager.Instance.AddCharacter(drawnCharacterSO);

        // 마지막 결과 리스트를 새로 만들고, 이번에 뽑은 캐릭터 하나만 추가합니다.
        LastGachaResults = new List<PlayerCharacterData> { newCharacterInstance };

        // UI 갱신
        if (characterScrollViewUI != null)
        {
            characterScrollViewUI.RefreshUI();
        }
        else
        {
            Debug.LogWarning("GachaManager에 CharacterScrollViewUI가 연결되지 않았습니다!");
        }
        CurrencyManager.Instance.UpdateCurrencyUI();
        return true;
    }

    /// <summary>
    /// 가챠를 여러 번 실행하고 결과들을 플레이어 데이터에 적용한 후 UI를 갱신합니다.
    /// </summary>
    /// <param name="count">뽑을 횟수</param>
    public bool PerformMultipleGacha(int count)
    {
        // 1. 재화 소모 시도
        BigInteger cost = 100 * count;
        if (!CurrencyManager.Instance.SpendCurrency(CurrencyType.Gem, cost))
        {
            Debug.Log("가챠 실패: 재화(Gem)가 부족합니다.");
            return false; // 재화가 부족하면 함수 종료
        }

        // 2. 재화 소모 성공 시, 가챠 실행
        List<CharacterData> drawnCharacters = DrawMultipleCharacters(count);
        LastGachaResults = new List<PlayerCharacterData>(); // 리스트 초기화

        foreach (var characterSO in drawnCharacters)
        {
            // PlayerDataManager에 캐릭터를 추가하고, 생성된 PlayerCharacterData를 반환받는다고 가정합니다.
            PlayerCharacterData newCharacterInstance = PlayerDataManager.Instance.AddCharacter(characterSO);
            if (newCharacterInstance != null)
            {
                LastGachaResults.Add(newCharacterInstance);
            }
        }

        // UI 갱신
        if (characterScrollViewUI != null)
        {
            characterScrollViewUI.RefreshUI();
        }
        else
        {
            Debug.LogWarning("GachaManager에 CharacterScrollViewUI가 연결되지 않았습니다!");
        }
        CurrencyManager.Instance.UpdateCurrencyUI();
        return true;
    }


    /// <summary>
    /// 가챠를 1회 실행하여 랜덤한 캐릭터를 반환합니다. (내부 로직용)
    /// </summary>
    private CharacterData DrawCharacter()
    {
        // 1. 등급 뽑기
        Rarity chosenRarity = GetRandomRarity();

        // 2. 해당 등급의 캐릭터 목록 가져오기
        List<CharacterData> availableCharacters = charactersByRarity[chosenRarity];

        if (availableCharacters.Count == 0)
        {
            Debug.LogWarning($"{chosenRarity} 등급의 캐릭터가 목록에 없습니다! 기본 등급 캐릭터를 반환합니다.");
            if (charactersByRarity.ContainsKey(Rarity.B) && charactersByRarity[Rarity.B].Count > 0)
                return charactersByRarity[Rarity.B][0];
            else if (allCharacters.Count > 0)
                return allCharacters[0];
            else
            {
                Debug.LogError("뽑을 수 있는 캐릭터가 아무도 없습니다!");
                return null;
            }
        }

        // 3. 역할(CrewRole)에 따라 가중치를 적용하여 캐릭터 뽑기
        // 각 캐릭터의 가중치를 계산합니다. 기본 가중치는 1입니다.
        // '선장' 역할(CrewRole.Captain)일 경우, 설정된 확률 배수(captainProbabilityMultiplier)를 적용합니다.
        // 참고: CharacterData에 'public CrewRole crewRole;' 와 같이 역할 정보가 정의되어 있어야 합니다.
        //       CrewRole enum에 'Captain' 항목이 있어야 합니다.
        var weightedCharacters = availableCharacters.Select(c => new {
            Character = c,
            Weight = (c.crewRole == CrewRole.Captain ? 1.0f * captainProbabilityMultiplier : 1.0f)
        }).ToList();

        float totalWeight = weightedCharacters.Sum(c => c.Weight);

        if (totalWeight <= 0)
        {
            Debug.LogWarning($"{chosenRarity} 등급에 뽑을 수 있는(가중치가 0보다 큰) 캐릭터가 없습니다. 첫번째 캐릭터를 반환합니다.");
            return availableCharacters[0];
        }

        float randomPoint = Random.Range(0, totalWeight);

        foreach (var entry in weightedCharacters)
        {
            if (randomPoint < entry.Weight)
            {
                Debug.Log($"가챠 결과: [{entry.Character.rarity}] {entry.Character.characterName} (역할: {entry.Character.crewRole}, 가중치: {entry.Weight}) 획득!");
                return entry.Character;
            }
            else
            {
                randomPoint -= entry.Weight;
            }
        }

        // 예외 상황 방지: 루프를 빠져나온 경우 마지막 캐릭터 반환
        return weightedCharacters.Last().Character;
    }

    /// <summary>
    /// 가챠를 지정된 횟수만큼 실행하고 결과 목록을 반환합니다. (내부 로직용)
    /// </summary>
    /// <param name="count">뽑을 횟수</param>
    /// <returns>뽑힌 캐릭터들의 리스트</returns>
    private List<CharacterData> DrawMultipleCharacters(int count)
    {
        List<CharacterData> results = new List<CharacterData>();
        for (int i = 0; i < count; i++)
        {
            // 기존의 1회 뽑기 함수를 호출하여 결과에 추가합니다.
            results.Add(DrawCharacter());
        }

        Debug.Log($"{count}회 연속 뽑기 완료! {results.Count}개의 결과를 반환합니다.");
        return results;
    }

    // 설정된 확률에 따라 등급을 뽑는 함수
    private Rarity GetRandomRarity()
    {
        float totalChance = rarityChances.Sum(rc => rc.chance);
        float randomPoint = Random.Range(0, totalChance);

        foreach (var rarityInfo in rarityChances)
        {
            if (randomPoint < rarityInfo.chance)
            {
                return rarityInfo.rarity;
            }
            else
            {
                randomPoint -= rarityInfo.chance;
            }
        }
        // 만약 확률 합계가 100이 아니거나 하는 예외 상황을 대비해 기본 등급 반환
        return rarityChances[0].rarity;
    }
}
