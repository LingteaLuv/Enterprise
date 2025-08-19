using UnityEngine;
using System.Collections.Generic; // List<>를 사용하기 위해 추가합니다.

[System.Serializable] // 인스펙터에서 보이기 위해 추가
public class PlayerCharacterData
{
    [Header("캐릭터")]
    public CharacterData characterdata; // 어떤 캐릭터인지에 대한 원본 데이터
    public int characterLevel;
    public int stars; // 성급

    // Key: 스탯 이름(string), Value: 현재 스탯 양 캐릭터의 레벨에 따라서 변경
    public Dictionary<string, float> characterStats = new Dictionary<string, float>();

    //[Header("장비")]
    // 장비 코드 정보만 저장

    //[Header("유물")]
    // 적용되고 있는 유물 정보만 저장

    [Header("최종 스탯")]
    public Dictionary<string, float> finalStats = new Dictionary<string, float>();

    public PlayerCharacterData(CharacterData so)
    {
        characterdata = so;
        characterLevel = 1;
        stars = (int)so.rarity; // CharacterData의 rarity는 Enum이므로 int로 캐스팅

        characterStats = new Dictionary<string, float>();
        foreach (var stat in so.baseStats)
        {
            if (!characterStats.ContainsKey(stat.statName))
            {
                characterStats.Add(stat.statName, stat.value);
            }
        }
    }

    /// <summary>
    /// 캐릭터에 적용되는 모든 스탯을 계산합니다. (현재 기본스탯과 캐릭터 스탯 적용. 장비 유물 미적용)
    /// </summary>
    public void RecaculateStats()
    {
        finalStats.Clear();

        // 캐릭터 스탯 적용
        foreach (var stat in characterStats)
        {
            finalStats[stat.Key] = stat.Value;
        }

        // 기본 스탯 적용 
        finalStats["health"] += BasicStatManager.Instance.GetStatValue(BasicStatType.Health);
        finalStats["attackPower"] += BasicStatManager.Instance.GetStatValue(BasicStatType.Attack);
        finalStats["defensePower"] += BasicStatManager.Instance.GetStatValue(BasicStatType.Defense);


        // 장비 스탯 적용
        // 유물 스탯 적용
    }
}