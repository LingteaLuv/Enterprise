using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace JHT
{
    public class JHT_MonsterSetManager : MonoBehaviour
    {
        [SerializeField] private Transform[] setTransform;

        public Dictionary<CrewRole, int> monsterCrewCountDic = new()
        {
            { CrewRole.Cook,  0 },
            { CrewRole.Sailor,   2 }, 
            { CrewRole.Deckhand, 4 }, 
        };

        public List<CrewRole> checkList;

        public Transform SetPos(JHT_MonsterDataSO so)
        {
            if (so == null)
            {
                Debug.LogError("[SetPos] : so없음");
                return null;
            }

            if (SetPosIndex(so) == -1)
            {
                Debug.LogError("[SetPos] : index값 없음");
                return null;
            }

            Transform pos = setTransform[SetPosIndex(so)];
            return pos;
        }

        public int SetPosIndex(JHT_MonsterDataSO so)
        {
            int index = -1;
            if (monsterCrewCountDic.TryGetValue(so.monsterCrewRole, out int value))
            {
                if (checkList.Contains(so.monsterCrewRole))
                    index = monsterCrewCountDic[so.monsterCrewRole] + 1;
                else
                {
                    checkList.Add(so.monsterCrewRole);
                    index = monsterCrewCountDic[so.monsterCrewRole];
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