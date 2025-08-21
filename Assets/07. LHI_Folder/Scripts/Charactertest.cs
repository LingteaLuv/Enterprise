using UnityEngine;
using System;

namespace LHI
{
    public class Charactertest : MonoBehaviour
    {
        public CharacterData[] characterData;

        public static event Action OnClicked;

        public void DoThing()
        {
            OnClicked?.Invoke();
        }
        void Start()
        {


        }

        public void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 150, 30), "Click Me"))
            {
                OnClicked?.Invoke();
                Debug.Log("Button Clicked!");
            }
        }
        public void Debug_()
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
