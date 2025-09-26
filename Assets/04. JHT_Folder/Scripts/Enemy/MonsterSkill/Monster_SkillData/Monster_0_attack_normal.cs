using System.Linq;
using System.Xml.Xsl;
using JHT;
using UnityEngine;

public class Monster_0_attack_normal : MonsterSkillSet
{
    public Monster_0_attack_normal(MonsterSkillSO _skillSO) : base(_skillSO) { }

    public override void UseSkill(IAttacker caster, IDamageable primaryTarget = null)
    {
        base.UseSkill(caster);

        // base에서도 있는 작업인데 또 해야하는지
        JHT_BaseMonsterFSM fsm = caster as JHT_BaseMonsterFSM;

        // 적들 찾기
        CombatCharacter inst = FindAllEnemies(fsm).ToArray()[0];

        inst.GetComponent<CombatCharacter>().TakeDamage(fsm); //효과는 없고 데미지만

        



        //Collider2D[] cols = Physics2D.OverlapCircleAll(fsm.transform.position, fsm.monsterStat.attackRange, fsm.targetLayer);

        //foreach (var c in cols)
        //{
        //    IDamageable targetDamageable = c.GetComponent<IDamageable>();
        //    if (targetDamageable != null)
        //    {
        //        //Pool 파티클 사용
        //        //hs.TakeDamage(monsterStat.totalAttackPower);
        //        targetDamageable.TakeDamage(fsm, 1f);

        //    }
        //}

    }
}
