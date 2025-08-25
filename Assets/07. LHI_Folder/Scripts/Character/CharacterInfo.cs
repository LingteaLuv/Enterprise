using UnityEngine;

namespace LHI
{
    [System.Serializable]
    public class CharacterInfo
    {
        public int id;
        public CharacterData characterData;  // ScriptableObject

        // 나중에 인스펙터에서 숨기기
        [HideInInspector]
        public int strengthenLevel = 1;
        [HideInInspector]
        public int evolveLevel = 1;
        [HideInInspector]
        public bool isOwned = false;
    }
}