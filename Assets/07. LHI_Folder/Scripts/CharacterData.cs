using System;
using UnityEngine;


namespace LHI
{
    public enum Role
    {
        Captain,
        Boatswain,
        Sailor,
        Cook
    }

    public enum Affiliation
    {
        navy,
        pirate,
        monster
    }

    [CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("캐릭터 정보")]
        [Tooltip("캐릭터의 이름")]
        public string characterName;
        [Tooltip("캐릭터의 역할")]
        public Role role;
        [Tooltip("캐릭터의 소속")]
        public Affiliation affiliation;

        [Header("캐릭터 스탯")]
        [Tooltip("캐릭터의 공격력")]
        public float attack;
        [Tooltip("캐릭터의 체력")]
        public float health;
        [Tooltip("캐릭터의 방어력")]
        public float defense;

        [Range(0, 100)]
        [Tooltip("캐릭터의 치명타 확률 (%)")]
        public float criticalChance;
        [Tooltip("캐릭터의 치명타 배율 (%), ex)2배를 200로 작성할 것")]
        public float criticalDamage;
        [Tooltip("캐릭터가 자신의 턴을 가지는 속도를 결정하는 스탯 ex) 1.5는 1초에 1.5 번 공격, 2는 1초에 2번 공격을 의미함")]
        public float speed;

        private void OnValidate()
        {
            if (attack < 0f)
            {
                attack = 0f;
                Debug.Log($"[CharacterData.attack] '{attack}' 의 값이 잘못 되었습니다. (음수 X)");
            }
            if (health < 0f)
            {
                health = 0f;
                Debug.Log($"[CharacterData.health] '{health}' 의 값이 잘못 되었습니다. (음수 X)");
            }
            if (defense < 0f)
            {
                defense = 0f;
                Debug.Log($"[CharacterData.defense] '{defense}' 의 값이 잘못 되었습니다. (음수 X)");
            }
            if (criticalDamage < 0f)
            {
                criticalDamage = 0f;
                Debug.Log($"[CharacterData.criticalDamage] '{criticalDamage}' 의 값이 잘못 되었습니다. (음수 X)");
            }
            if (speed < 0f)
            {
                speed = 0f;
                Debug.Log($"[CharacterData.speed] '{speed}' 의 값이 잘못 되었습니다. (음수 X)");
            }
        }
    }
}