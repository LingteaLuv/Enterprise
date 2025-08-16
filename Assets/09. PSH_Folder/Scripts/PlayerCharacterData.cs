
using UnityEngine;

[System.Serializable] // 인스펙터에서 보이기 위해 추가
public class PlayerCharacterData
{
    public CharacterData characterdata; // 어떤 캐릭터인지에 대한 원본 데이터
    public int level;
    public int stars; // 성급
    public int fragments; // 보유 조각 수

    public PlayerCharacterData(CharacterData so)
    {
        characterdata = so;
        level = 1;
        stars = (int)so.rarity;
        fragments = 0;
    }
}
