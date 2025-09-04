using JHT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterGachaManager : BaseGachaManager<PlayerCharacterData>
{
    public const int GACHA_CEILING_COUNT = 20; // 천장 횟수
    public int gachaPityCounter = 0;

    [Header("캐릭터 데이터 (전용)")]
    [Tooltip("캐릭터 SO 에셋들이 저장된 폴더 경로")]
    public string characterDataFolderPath = "CharacterData";
    [Tooltip("게임에 존재하는 모든 캐릭터 SO 목록")]
    public List<CharacterData> allCharacters;

    [Header("캐릭터 가챠 전용 확률")]
    [Range(0, 100)]
    public float captainChance = 10f; // 선장이 나올 확률

    [Tooltip("선장이 아닐 경우, 등급별로 뽑힐 확률")]
    public List<GradeChance> crewGradeChances;

    [Tooltip("선장일 경우, 등급별로 뽑힐 확률")]
    public List<GradeChance> captainGradeChances;

    private List<CharacterData> captainPool;
    private List<CharacterData> crewPool;

    protected override void Start()
    {
        base.Start(); // 부모 클래스의 Start()를 먼저 실행
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

        captainPool = allCharacters.Where(c => c.crewRole == CrewRole.Captain).ToList();
        crewPool = allCharacters.Where(c => c.crewRole != CrewRole.Captain).ToList();

        Debug.Log($"[CharacterGachaManager] 선장 풀: {captainPool.Count}명, 선원 풀: {crewPool.Count}명");

        gachaPityCounter = PlayerDataManager.Instance.GachaPityCounter;
        Debug.Log($"[CharacterGachaManager] 현재 천장 카운트: {gachaPityCounter}");
    }

    public override bool PerformMultipleGacha(int count)
    {
        System.Numerics.BigInteger totalCost = singleGachaCost * count;
        if (!CurrencyManager.Instance.SpendCurrency(currencyType, totalCost))
        {
            Debug.Log($"가챠 실패: 재화({currencyType})가 부족합니다.");
            return false;
        }

        LastGachaResults = new List<PlayerCharacterData>();
        List<GachaGrade> resultGrades = new List<GachaGrade>();

        for (int i = 0; i < count; i++)
        {
            gachaPityCounter++;

            GachaGrade grade;
            bool isCaptain;

            if (gachaPityCounter >= GACHA_CEILING_COUNT)
            {
                Debug.Log($"<color=yellow>천장 시스템 발동! 3성 캐릭터를 확정적으로 뽑습니다. (카운트: {gachaPityCounter})</color>");
                grade = GachaGrade.ThreeStar; // 천장: 등급만 3성으로 고정
                isCaptain = Random.Range(0, 100f) < captainChance; // 선장 여부는 일반 확률 따름
                gachaPityCounter = 0;
            }
            else
            {
                isCaptain = Random.Range(0, 100f) < captainChance;
                List<GradeChance> gradeChances = isCaptain ? captainGradeChances : crewGradeChances;
                grade = GetRandomGrade(gradeChances);
            }

            PlayerCharacterData drawnCharacter = GetCharacterFromPool(isCaptain, grade);

            if (drawnCharacter != null)
            {
                LastGachaResults.Add(drawnCharacter);
                resultGrades.Add(grade);
            }
        }

        Debug.Log($"{count}회 뽑기 완료! {LastGachaResults.Count}개의 아이템 획득.");
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            GachaListUI resultUI = resultPanel.GetComponent<GachaListUI>();
            if (resultUI != null)
            {
                resultUI.DisplayCharacterResults(LastGachaResults, resultGrades);
            }
        }
        CurrencyManager.Instance.UpdateCurrencyUI();

        PlayerDataManager.Instance.GachaPityCounter = gachaPityCounter;

        return true;
    }

    private PlayerCharacterData GetCharacterFromPool(bool isCaptain, GachaGrade grade)
    {
        List<CharacterData> characterPool = isCaptain ? captainPool : crewPool;
        if (characterPool == null || characterPool.Count == 0)
        {
            Debug.LogWarning($"뽑기 풀이 비어있습니다: {(isCaptain ? "선장" : "선원")}");
            return null;
        }

        CharacterData drawnCharacterSO = characterPool[Random.Range(0, characterPool.Count)];
        PlayerCharacterData newCharacterInstance = PlayerDataManager.Instance.AddCharacter(drawnCharacterSO, grade);

        Debug.Log($"캐릭터 뽑기 결과: [{(isCaptain ? "선장" : "선원")}, {grade}] {newCharacterInstance.characterdata.characterName}");
        return newCharacterInstance;
    }

    private GachaGrade GetRandomGrade(List<GradeChance> chances)
    {
        float totalChance = chances.Sum(gc => gc.chance);
        if (totalChance <= 0)
        {
            Debug.LogError("등급 확률의 총합이 0보다 작거나 같습니다!");
            return chances.FirstOrDefault()?.grade ?? GachaGrade.OneStar;
        }

        float randomPoint = Random.Range(0, totalChance);

        foreach (var gradeInfo in chances)
        {
            if (randomPoint < gradeInfo.chance)
            {
                return gradeInfo.grade;
            }
            else
            {
                randomPoint -= gradeInfo.chance;
            }
        }
        return chances.Last().grade; // 예외 상황 방지
    }

    // BaseGachaManager의 DrawItem은 이제 사용되지 않으므로, 빈 구현을 제공합니다.
    protected override PlayerCharacterData DrawItem()
    {
        return null;
    }
}
