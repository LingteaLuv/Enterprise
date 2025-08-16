using System.Numerics;
using UnityEngine;

public enum Rarity
{
    C = 1, B, A
}

[CreateAssetMenu(fileName = "New Character", menuName = "Test Character")]
public class CharacterData : ScriptableObject
{
    [Header("캐릭터 정보")]
    public string characterName;
    public Sprite characterSprite;
    [TextArea]
    public string description;

    [Header("가챠 정보")]
    public Rarity rarity; // 이 캐릭터의 등급

    [Header("능력치 (BigInteger)")]
    [SerializeField]
    private string maxHealthString = "100";
    public BigInteger MaxHealth { get; private set; }

    private void OnValidate()
    {
        // 인스펙터에서 입력한 체력(문자열)을 실제 BigInteger 값으로 변환합니다.
        if (!BigInteger.TryParse(maxHealthString, out BigInteger parsedValue))
        {
            parsedValue = 100;
        }
        MaxHealth = parsedValue;
        maxHealthString = MaxHealth.ToString();
    }
}
