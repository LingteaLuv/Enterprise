using System;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    [CreateAssetMenu(menuName = "Quest/Sequence/Tutorial")]
    public class TutorialSequenceSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public TutorialQuestDefinitionSO tutorial;
        }
        
        public Entry[] entries;
    }
}