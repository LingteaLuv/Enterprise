using System.Collections.Generic;
using UnityEngine;
public enum Rarity
{
    C = 1, B, A
}

public enum CrewRole
{
    Deckhand,   // 갑판원
    Sailor,    // 선원
    Cook,      // 요리사
    Captain  // 선장
}
public enum Faction
{
    Navy,     // 해군
    Pirate,   // 해적
    Monster   // 괴물
}

// 에셋 메뉴에서 CharacterData를 생성할 수 있도록 메뉴를 추가합니다.
[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
    // CSV의 각 열(column)에 해당하는 변수들을 선언합니다.
    public int characterID;
    public string characterName;
    public string description;
    public Sprite characterSprite; // 캐릭터 이미지
    public Rarity rarity; // 1, 2, 3성 등급
    public CrewRole crewRole; // 진형(전중후최후)
    public Faction faction; // 속성(불물풀)

    [Header("스탯 정보")]
    public List<StatData> baseStats; // 이 캐릭터의 기본 스탯 목록
}
