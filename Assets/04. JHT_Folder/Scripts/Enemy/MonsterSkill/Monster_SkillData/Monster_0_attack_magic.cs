using JHT;
using UnityEngine;

public class Monster_0_attack_magic : MonsterSkillSet
{
    public Monster_0_attack_magic(MonsterSkillSO _skillSO) : base(_skillSO) { }

    public override void UseSkill(IAttacker caster, IDamageable primaryTarget = null)
    {
        JHT_BaseMonsterFSM fsm = caster as JHT_BaseMonsterFSM;

        if (fsm.target == null) return;

        IDamageable targetDamageable = fsm.target.GetComponent<IDamageable>();
        if (targetDamageable == null)
            return;

        JHT_MonsterProjectile obj = JHT_MonsterSpawnManager.Instance.projectilePool.GetPooled() as JHT_MonsterProjectile;

        if (obj == null)
            return;

        Vector2 startPos = fsm.transform.position;
        Vector2 targetPos = (Vector2)fsm.target.transform.position;


        //if (this != null)
        //    obj.Init(targetPos, startPos, monsterStat.totalAttackPower, monsterStat.totalAttackPower,monsterStat.projectileSprite);

        float projectileSpeed = 1f;
        obj.Init(fsm, targetPos, startPos, projectileSpeed, fsm.monsterStat.monsterStats[Stat.Attack], fsm.monsterStat.projectileSprite);
        
    }
}
