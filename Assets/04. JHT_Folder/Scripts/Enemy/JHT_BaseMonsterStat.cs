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
        public Sprite projectileImage;
        public Sprite characterImage;

        public MonsterType monsterType;
        public MonsterRarity monsterRarity;

        //애니메이션 R&D 후 추가 animator override controller OR animator

        public void Init(JHT_MonsterDataSO so)
        {
            // start Setting
            curSO = so;
            maxHp = curSO.maxHp;
            curHp = maxHp;

            // Stat Setting
            attackPower = curSO.upAttackPower;// * GlobalStageManager.Instance.currentStageIndex;
            defense = curSO.upDefense;// * GlobalStageManager.Instance.currentStageIndex;
            attackRange = curSO.attackRange;
            chaseRange = curSO.chaseRange;
            moveSpeed = curSO.upMoveSpeed;
            attackDelay = curSO.attackDelay;
            attackSpeed = curSO.attackSpeed;

            // Enum
            monsterType = curSO.monsterType;
            monsterRarity = curSO.monsterRarity;

            // Sprite
            characterImage = curSO.enemyCharacter;
            projectileImage = curSO.projectileSprite;
        }

    }
}
