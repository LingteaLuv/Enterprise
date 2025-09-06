using System;
using UnityEngine;

namespace JHT
{
    public class JHT_BaseMonsterStat : MonoBehaviour
    {
        public JHT_MonsterDataSO curSO;

        public float curHp;
        public float maxHp;
        public float attackPower;
        public float defense;
        public float attackRange;
        public float attackSpeed;
        public float chaseRange;
        public float moveSpeed;
        public float attackDelay;
        public JHT_MonsterProjectile projectile;
        public GameObject particle;

        public MonsterType type;
        public MonsterRarity rarity;

        //애니메이션 R&D 후 추가 animator override controller OR animator

        public void Init(JHT_MonsterDataSO so)
        {
            curSO = so;
            maxHp = curSO.maxHp;
            curHp = maxHp;

            attackPower = curSO.upAttackPower;// * GlobalStageManager.Instance.currentStageIndex;
            defense = curSO.upDefense;// * GlobalStageManager.Instance.currentStageIndex;
            attackRange = curSO.attackRange;
            chaseRange = curSO.chaseRange;
            moveSpeed = curSO.upMoveSpeed;
            attackDelay = curSO.attackDelay;
            attackSpeed = curSO.attackSpeed;
            particle = curSO.particle;
            type = curSO.monsterType;
            rarity = curSO.monsterRarity;
        }

    }
}
