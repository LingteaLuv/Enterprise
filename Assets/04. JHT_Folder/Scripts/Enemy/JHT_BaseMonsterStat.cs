using System;
using UnityEngine;

namespace JHT
{
    public class JHT_BaseMonsterStat : MonoBehaviour
    {
        public JHT_MonsterDataSO curSO;

        public GameObject enemyCharacter;

        public float maxHp;
        public float attackPower;
        public float defense;
        public float attackSpeed;
        public float attackDelay;

        public float attackRange;
        public float chaseRange;
        public float moveSpeed;

        public Sprite projectileSprite;

        public MonsterType monsterType;
        public MonsterRarity monsterRarity;
        public CrewRole monsterCrewRole;
        public AnimatorOverrideController animatorOverride;
        //애니메이션 R&D 후 추가 animator override controller OR animator

        private float curHp;
        public float CurHp { get { return curHp; } set { curHp = value; OnChangeHp?.Invoke(curHp); } }
        public Action<float> OnChangeHp;

        public void Init(JHT_MonsterDataSO so)
        {
            // start Setting
            curSO = so;
            maxHp = curSO.maxHp;

            enemyCharacter = so.enemyCharacter;

            // Stat Setting
            attackPower = curSO.attackPower;// * GlobalStageManager.Instance.currentStageIndex;
            defense = curSO.defense;// * GlobalStageManager.Instance.currentStageIndex;
            attackRange = curSO.attackRange;
            chaseRange = curSO.chaseRange;
            moveSpeed = curSO.moveSpeed;
            attackSpeed = curSO.attackSpeed;
            attackDelay = curSO.attackDelay;

            //sprite
            projectileSprite = curSO.projectileSprite;

            // Enum
            monsterType = curSO.monsterType;
            monsterRarity = curSO.monsterRarity;
            monsterCrewRole = curSO.monsterCrewRole;

            if (curSO.animatorOverrideController != null)
            {
                animatorOverride = curSO.animatorOverrideController;
            }
        }

    }
}
