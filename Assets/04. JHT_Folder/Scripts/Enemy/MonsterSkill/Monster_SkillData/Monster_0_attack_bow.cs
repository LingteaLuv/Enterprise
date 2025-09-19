using JHT;
using UnityEngine;

public class Monster_0_attack_bow : MonsterSkillSet
{
    public Monster_0_attack_bow(JHT_BaseMonsterStat stat, MonsterSkillSO _skillSO) : base(stat, _skillSO) { }

    public override void UseSkill(JHT_BaseMonsterFSM fsm)
    {
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
        obj.Init(fsm, targetPos, startPos, projectileSpeed, fsm.monsterStat.totalAttackPower, fsm.monsterStat.projectileSprite);
        Debug.LogError("BowAttack Skill Set");
    }
}
