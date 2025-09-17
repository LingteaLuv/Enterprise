using System;
using UnityEngine;

namespace JHT
{
    [System.Serializable]
    public class JHT_BaseMonsterStat
    {
        public JHT_MonsterDataSO curSO;

        public float maxHp;
        public float totalAttackPower;
        public float defense;
        //public float attackSpeed;
        public float attackDelay;
        public float attackSpeed;
        public float attackRange;
        public float chaseRange;
        public float moveSpeed;

        public int cost;

        public Sprite projectileSprite;

        public AtkRangeType monsterAttackType;
        public MonsterRarity monsterRarity;
        public CrewRole monsterCrewRole;
        public AnimatorOverrideController aoc;
        //애니메이션 R&D 후 추가 animator override controller OR animator

        public MonsterSkillSO normalSkill;
        public MonsterSkillSO skill1;
        public MonsterSkillSO skill2;

        public JHT_BaseMonsterStat(JHT_MonsterDataSO so, MonsterSkillSO normalSkill,
            MonsterSkillSO skill1, MonsterSkillSO skill2)
        {
            // start Setting
            curSO = so;
            maxHp = curSO.maxHp;

            // Stat Setting
            totalAttackPower = AttackCalculate(curSO);// * GlobalStageManager.Instance.currentStageIndex;
            defense = curSO.defense;// * GlobalStageManager.Instance.currentStageIndex;
            attackRange = curSO.attackRange;
            chaseRange = curSO.chaseRange;
            moveSpeed = curSO.moveSpeed;
            attackSpeed = curSO.attackSpeed;
            attackDelay = curSO.attackDelay;

            //sprite
            projectileSprite = curSO.projectileSprite;

            // Enum
            monsterAttackType = curSO.monsterAttackType;
            monsterCrewRole = curSO.monsterCrewRole;

            cost = curSO.cost;
            this.aoc = new AnimatorOverrideController(curSO.baseController);

            //Skill
            this.normalSkill = normalSkill == null ? null : normalSkill;
            this.skill1 = skill1 == null ? null : skill1;
            this.skill2 = skill2 == null ? null : skill2;

            AnimSetting();
        }

        private void AnimSetting()
        {
            if(normalSkill != null)
                aoc["Monster_ATTACK"] = normalSkill.clip;

            if (skill1 != null)
                aoc["SKILL1"] = skill1.clip;

            if(skill2 != null)
                aoc["SKILL2"] = skill2.clip;
        }

        private float AttackCalculate(JHT_MonsterDataSO curSO)
        {
            float total = 0;

            switch (monsterCrewRole)
            {
                case CrewRole.Captain:
                    total = curSO.attackPower;
                    break;
                case CrewRole.Sailor:
                    total = curSO.attackPower;
                    break;
                case CrewRole.Deckhand:
                    total = curSO.attackPower;
                    break;
                case CrewRole.Cook:
                    total = curSO.attackPower;
                    break;
            }
            return total;
        }

        public float GetCurrentStat(Stat stat)
        {
            switch (stat)
            {
                case Stat.Attack:
                    return this.totalAttackPower;
                case Stat.Defense:
                    return this.defense;
                case Stat.CritChance:
                    return 0; // 몬스터는 크리티컬이 없다고 가정
                case Stat.CritDamage:
                    return 1; // 몬스터는 크리티컬이 없다고 가정
                case Stat.Health:
                    return this.maxHp;
                case Stat.AttackSpeed:
                    return this.attackSpeed;
                default:
                    return 0;
            }

        }
    }
}
