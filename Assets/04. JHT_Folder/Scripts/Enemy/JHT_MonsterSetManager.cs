using System.Collections.Generic;
using UnityEngine;

namespace JHT
{
    public class JHT_MonsterSetManager : MonoBehaviour
    {

        [SerializeField] private Transform[] setTransform;

        public Transform SetPos(JHT_MonsterDataSO so)
        {
            Transform pos;
            switch (so.monsterCrewRole)
            {
                case CrewRole.Sailor:
                    break;
                case CrewRole.Captain:
                    break;
                case CrewRole.Deckhand:
                    break;
                case CrewRole.Cook:
                    break;
            }

            return null;
        }
    }

}