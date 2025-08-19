using UnityEngine;

namespace LHI
{
    public class Charactertest : MonoBehaviour
    {
        [SerializeField]
        public CharacterData[] characterData;

        void Start()
        {
            foreach (var character in characterData)
            {
                Debug.Log($"Character Name: {character.characterName}");
                Debug.Log($"Role: {character.role}");
                Debug.Log($"Affiliation: {character.affiliation}");
                Debug.Log($"Attack: {character.attack}");
                Debug.Log($"Health: {character.health}");
                Debug.Log($"Defense: {character.defense}");
                Debug.Log($"Critical Chance: {character.criticalChance}%");
                Debug.Log($"Critical Damage: {character.criticalDamage}%");
                Debug.Log($"Speed: {character.speed}");
            }

        }



        // Update is called once per frame
        void Update()
        {

        }
    }
}
