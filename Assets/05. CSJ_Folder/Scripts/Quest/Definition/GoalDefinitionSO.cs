using System;
using System.Text;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/Goals")]
    public class GoalDefinitionSO : ScriptableObject
    {
        // 퀘스트 목표의 키 ex) "kill : slime"
        [SerializeField] private KeyFunc keyValue;

        [SerializeField]
        public TypedEnumKey enumKey;
        // 퀘스트 설명 ex) 슬라임을 {RequireCount} 처치하세요.
        [TextArea] private string description;
        // 퀘스트 목표 처치 수
        public int RequireCount = 1;

        private string key;
        public string Key => key;
        public string Description => description;

        public void Start()
        {
            if (keyValue == KeyFunc.StageClear)
            {
                key = keyValue.ToString();
            }
            else
            {       
                StringBuilder sb = new StringBuilder();
                key = sb.Append(keyValue.ToString()).Append(":").Append(enumKey.ToKeyString()).ToString();
            }
        }

        public bool Matches(string signalKey) => string.Equals(Normalize(signalKey), Normalize(key), StringComparison.Ordinal);

        public int ProgressDeltaFrom(string signalKey, int delta) => Matches(signalKey) ? delta : 0;

        private static string Normalize(string s) => (s ?? "").Trim();
    } 
}