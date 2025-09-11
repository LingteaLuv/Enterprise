
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스킬의 기본 데이터를 정의하는 ScriptableObject입니다.
/// 하나의 스킬은 여러 개의 스킬 효과(SkillEffectSO)를 가질 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("스킬 정보")]
    public int SkillID;
    public string skillName;
    [TextArea] public string skillDescription;
    public Sprite skillIcon;
    public float cooldown;

    [Header("스킬 효과 목록")]
    // [SerializeReference]를 사용하면 SkillEffectSO를 상속받은 모든 SO를 직접 할당할 수 있습니다.
    // 하지만 유니티 버전에 따라 UI가 불편할 수 있으므로, 여기서는 직접 List에 SO를 넣는 방식을 사용합니다.
    public List<SkillEffectSO> effects = new List<SkillEffectSO>();

    /// <param name="caster">스킬 시전자</param>
    /// <param name="targets">스킬 대상 리스트</param>
    public void Use(IAttacker caster, List<IDamageable> targets)
    {
        // IAttacker를 CombatCharacter로 캐스팅 시도
        CombatCharacter casterCharacter = caster as CombatCharacter;
        if (casterCharacter == null)
        {
            Debug.LogError("스킬 시전자가 CombatCharacter가 아닙니다!");
            return;
        }

        Debug.Log($"'{casterCharacter.name}'이(가) '{skillName}' 스킬을 사용!");
        foreach (var target in targets)
        {
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    effect.ApplyEffect(caster, target);
                }
            }
        }
    }
}
