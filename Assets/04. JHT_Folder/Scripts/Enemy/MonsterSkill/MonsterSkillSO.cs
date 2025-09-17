using UnityEngine;

[CreateAssetMenu(fileName = "MonsterSkill", menuName = "MonsterSkillSO/MonsterSkillSO")]
public class MonsterSkillSO : ScriptableObject
{
    public int ID;
    public string skillName;
    public MonsterSkillType skillType;
    public int attackCount;
    public float damagePercent;
    public float buffCount;
    public AnimationClip clip;


    public void Use(MonsterSkillType type)
    {

    }
}

public enum MonsterSkillType
{
    Buff,
    Attack
}
