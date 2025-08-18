using UnityEngine;
using System.Collections.Generic; // List<>를 사용하기 위해 추가합니다.

[System.Serializable] // 인스펙터에서 보이기 위해 추가
public class PlayerCharacterData
{
    public CharacterData characterdata; // 어떤 캐릭터인지에 대한 원본 데이터
    public int level;
    public int stars; // 성급

    // Key: 스탯 이름(string), Value: 현재 스탯 레벨(int)
    public Dictionary<string, int> statLevels = new Dictionary<string, int>();

    public PlayerCharacterData(CharacterData so)
    {
        characterdata = so;
        level = 1;
        stars = (int)so.rarity; // CharacterData의 rarity는 Enum이므로 int로 캐스팅

        // CharacterData에 정의된 기본 스탯들을 기반으로, 각 스탯의 초기 레벨을 1로 설정합니다.
        statLevels = new Dictionary<string, int>();
        foreach (var stat in so.baseStats)
        {
            if (!statLevels.ContainsKey(stat.statName))
            {
                statLevels.Add(stat.statName, 1);
            }
        }
    }
}