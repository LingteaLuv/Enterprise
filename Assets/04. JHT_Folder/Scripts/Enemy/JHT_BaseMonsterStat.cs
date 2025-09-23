using System;
using System.Collections.Generic;
using UnityEngine;

namespace JHT
{
    [System.Serializable]
    public class JHT_BaseMonsterStat
    {
        public JHT_MonsterDataSO curSO;

        public Dictionary<Stat, float> monsterStats = new Dictionary<Stat, float>();

        public float attackRange;
        public float chaseRange;
        public float moveSpeed;

        public int cost;

        public Sprite projectileSprite;

        public AtkRangeType monsterAttackRangeType;
        public MonsterRarity monsterRarity;
        public CrewRole monsterCrewRole;
        public AnimatorOverrideController aoc;
        //애니메이션 R&D 후 추가 animator override controller OR animator

        public MonsterSkillSO normalSkill;
        public MonsterSkillSO skill1;
        public MonsterSkillSO skill2;

        public JHT_BaseMonsterStat(JHT_MonsterDataSO so, MonsterSkillSO normalSkill,
            MonsterSkillSO skill1, MonsterSkillSO skill2, float addStat)
        {
            // start Setting
            curSO = so;

            foreach(var m in curSO.monsterStat)
            {
                monsterStats[m.stat] = m.amount * addStat * AttackCalculate(curSO);
            }

            chaseRange = curSO.chaseRange * addStat;

            moveSpeed = curSO.moveSpeed * addStat;

            //sprite
            projectileSprite = curSO.projectileSprite;

            // Enum
            monsterAttackRangeType = curSO.monsterAttackType;
            attackRange = curSO.monsterAttackType == AtkRangeType.Melee_Attack? 1f : 2f;
            attackRange *= addStat;

            monsterCrewRole = curSO.monsterCrewRole;

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
                aoc["Monster_1_Skill_Normal"] = skill1.clip;

            if(skill2 != null)
                aoc["Monster_2_Skill_Normal"] = skill2.clip;
        }

        private float AttackCalculate(JHT_MonsterDataSO curSO)
        {
            float upPercent = 0;

            switch (monsterCrewRole)
            {
                case CrewRole.Captain:
                    upPercent = 2;
                    break;
                case CrewRole.Sailor:
                    upPercent = 1.5f;
                    break;
                case CrewRole.Deckhand:
                    upPercent = 1;
                    break;
                case CrewRole.Cook:
                    upPercent = 0.8f;
                    break;
            }
            return upPercent;
        }

        //public float GetCurrentStat(Stat stat)
        //{
        //    switch (stat)
        //    {
        //        case Stat.Attack:
        //            return characterStats[stat];
        //        case Stat.Defense:
        //            return this.defense;
        //        case Stat.CritChance:
        //            return 0; // 몬스터는 크리티컬이 없다고 가정
        //        case Stat.CritDamage:
        //            return 1; // 몬스터는 크리티컬이 없다고 가정
        //        case Stat.Health:
        //            return this.maxHp;
        //        case Stat.AttackSpeed:
        //            return this.attackSpeed;
        //        default:
        //            return 0;
        //    }

        //}
    }
}
