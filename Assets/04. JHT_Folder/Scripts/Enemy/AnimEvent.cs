using JHT;
using UnityEngine;

public class AnimEvent : MonoBehaviour
{
    private JHT_BaseMonsterFSM fsm;
    private JHT_NormalMonster normalFSM;
    private void Awake()
    {
        fsm = GetComponentInParent<JHT_BaseMonsterFSM>();

        if(fsm.monsterStat.monsterRarity == MonsterRarity.Normal || fsm.monsterStat.monsterRarity == MonsterRarity.Elite)
            normalFSM = fsm as JHT_NormalMonster;
    }

    public void AE_AttackHit() { normalFSM?.NormalMonsterAttack(); }
    public void SKILL1_AttackHit() { normalFSM.Skill1MonsterAttack(); }
    public void SKILL2_AttackHit() { normalFSM.Skill2MonsterAttack(); }
}
