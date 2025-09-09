using UnityEngine;

public class SkillTest : MonoBehaviour
{
    private CombatCharacter character;

    void Awake()
    {
        character = GetComponent<CombatCharacter>();
    }

    public void A() { character.UseSkill(0, character); }
}