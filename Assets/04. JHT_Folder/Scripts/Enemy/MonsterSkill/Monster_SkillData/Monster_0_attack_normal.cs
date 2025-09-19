using System.Xml.Xsl;
using JHT;
using UnityEngine;

public class Monster_0_attack_normal : MonsterSkillSet
{
    public Monster_0_attack_normal(JHT_BaseMonsterStat stat, MonsterSkillSO _skillSO) : base(stat, _skillSO) { }

    public override void UseSkill(JHT_BaseMonsterFSM fsm)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(fsm.transform.position, fsm.monsterStat.attackRange, fsm.targetLayer);

        foreach (var c in cols)
        {
            IDamageable targetDamageable = c.GetComponent<IDamageable>();
            if (targetDamageable != null)
            {
                //Pool 파티클 사용
                //hs.TakeDamage(monsterStat.totalAttackPower);
                targetDamageable.TakeDamage(fsm, 1f);

            }
        }
        Debug.LogError("NormalAttack Skill Set");
    }
}
