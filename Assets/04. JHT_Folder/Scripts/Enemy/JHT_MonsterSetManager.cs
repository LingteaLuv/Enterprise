using System.Collections.Generic;
using UnityEngine;

namespace JHT
{
    public class JHT_MonsterSetManager : MonoBehaviour
    {
        [SerializeField] private Transform[] setTransform;

        public Dictionary<CrewRole, int> monsterCrewCountDic = new()
        {
            { CrewRole.Deckhand, 0 },
            { CrewRole.Sailor,   2 },
            { CrewRole.Cook,  4 },
            { CrewRole.Captain, 8}
        };

        public List<CrewRole> checkList;

        public Transform SetPos(JHT_BaseMonsterStat stat)
        {
            int idx = -1;
            //if (stat.monsterRarity == MonsterRarity.Elite)
            //{
            //    idx = 6;
            //}
            //else
            //{
            //    idx = SetPosIndex(stat);
            //}
            idx = SetPosIndex(stat);
            if (stat == null)
            {
                Debug.LogError("[SetPos] : so없음");
                return null;
            }

            if (idx == -1)
            {
                Debug.LogError("[SetPos] : index값 없음");
                return null;
            }
            Transform pos = setTransform[idx];
            return pos;
        }

        public int SetPosIndex(JHT_BaseMonsterStat stat)
        {
            int index = -1;
            if (monsterCrewCountDic.TryGetValue(stat.monsterCrewRole, out int value))
            {
                if (checkList.Contains(stat.monsterCrewRole))
                {
                    index = value + 1;
                }
                else if (stat.monsterCrewRole == CrewRole.Captain)
                {
                    if (!checkList.Contains(stat.monsterCrewRole))
                    {

                        checkList.Add(stat.monsterCrewRole);
                        index = 6;
                    }
                    else
                    {
                        index = -1;
                    }
                }
                else
                {
                    checkList.Add(stat.monsterCrewRole);
                    index = value;
                }
            }
            else
            {
                index = -1;
                Debug.LogError("존재하지 않는 값타입");
            }

            return index;
        }

    }

}