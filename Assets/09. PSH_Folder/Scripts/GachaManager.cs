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

    [Header("캐릭터 SO 자동 로드")]
    [Tooltip("캐릭터 SO 에셋들이 저장된 폴더 경로입니다.")]
    public string characterDataFolderPath = "Assets/Resources/CharacterData"; // 기본 경로 예시

    [Header("전체 캐릭터 목록 (점3개 메뉴 클릭해서 불러오기)")]
    public List<CharacterData> allCharacters; // 모든 캐릭터 ScriptableObject를 여기에 연결

    // 가장 마지막에 실행된 가챠 결과 리스트
    public List<PlayerCharacterData> lastGachaResults { get; private set; }

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
    public void PerformSingleGacha()
    {
        // 1. 재화 소모 시도
        BigInteger cost = 100;
        if (!CurrencyManager.Instance.SpendCurrency(CurrencyType.Gem, cost))
        {
            Debug.Log("가챠 실패: 재화(Gem)가 부족합니다.");
            return; // 재화가 부족하면 함수 종료
        }

        // 2. 재화 소모 성공 시, 가챠 실행
        CharacterData drawnCharacterSO = DrawCharacter();
        PlayerCharacterData newCharacterInstance = PlayerDataManager.Instance.AddCharacter(drawnCharacterSO);

        // 마지막 결과 리스트를 새로 만들고, 이번에 뽑은 캐릭터 하나만 추가합니다.
        lastGachaResults = new List<PlayerCharacterData> { newCharacterInstance };

        // UI 갱신
        if (characterScrollViewUI != null)
        {
            characterScrollViewUI.RefreshDisplay();
        }
        else
        {
            Debug.LogWarning("GachaManager에 CharacterScrollViewUI가 연결되지 않았습니다!");
        }
        CurrencyManager.Instance.UpdateCurrencyUI();
    }

    /// <summary>
    /// 가챠를 여러 번 실행하고 결과들을 플레이어 데이터에 적용한 후 UI를 갱신합니다.
    /// </summary>
    /// <param name="count">뽑을 횟수</param>
    public void PerformMultipleGacha(int count)
    {
        // 1. 재화 소모 시도
        BigInteger cost = 100 * count;
        if (!CurrencyManager.Instance.SpendCurrency(CurrencyType.Gem, cost))
        {
            Debug.Log("가챠 실패: 재화(Gem)가 부족합니다.");
            return; // 재화가 부족하면 함수 종료
        }

        // 2. 재화 소모 성공 시, 가챠 실행
        List<CharacterData> drawnCharacters = DrawMultipleCharacters(count);
        lastGachaResults = new List<PlayerCharacterData>(); // 리스트 초기화

        foreach (var characterSO in drawnCharacters)
        {
            // PlayerDataManager에 캐릭터를 추가하고, 생성된 PlayerCharacterData를 반환받는다고 가정합니다.
            PlayerCharacterData newCharacterInstance = PlayerDataManager.Instance.AddCharacter(characterSO);
            if (newCharacterInstance != null)
            {
                lastGachaResults.Add(newCharacterInstance);
            }
        }

        // UI 갱신
        if (characterScrollViewUI != null)
        {
            characterScrollViewUI.RefreshDisplay();
        }
        else
        {
            Debug.LogWarning("GachaManager에 CharacterScrollViewUI가 연결되지 않았습니다!");
        }
        CurrencyManager.Instance.UpdateCurrencyUI();
    }


    /// <summary>
    /// 가챠를 1회 실행하여 랜덤한 캐릭터를 반환합니다. (내부 로직용)
    /// </summary>
    private CharacterData DrawCharacter()
    {
        // 1. 등급 뽑기
        Rarity chosenRarity = GetRandomRarity();

        // 2. 해당 등급 내에서 캐릭터 뽑기
        List<CharacterData> availableCharacters = charactersByRarity[chosenRarity];
        if (availableCharacters.Count == 0)
        {
            Debug.LogWarning($"{chosenRarity} 등급의 캐릭터가 목록에 없습니다!");
            // 예외 처리: B 등급 등 기본 등급의 캐릭터를 대신 반환할 수 있습니다.
            return charactersByRarity[Rarity.B][0];
        }

        int randomIndex = Random.Range(0, availableCharacters.Count);
        CharacterData drawnCharacter = availableCharacters[randomIndex];

        Debug.Log($"가챠 결과: [{drawnCharacter.rarity}] {drawnCharacter.characterName} 획득!");
        return drawnCharacter;
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
