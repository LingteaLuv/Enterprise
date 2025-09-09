using System;
using System.Text;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/TempoGoals")]
    public class TempoGoalDefinitionSO : ScriptableObject
    {
        // 퀘스트 목표의 키 ex) "kill, GachaPull"
        [Header("퀘스트의 목표 키")]
        [SerializeField] 
        private KeyFunc keyValue;

        [Header("퀘스트 세부 항목")]
        // 퀘스트 세부 항목 : => ItemType.Relic, Monster.All, UpgradeType.Atk; 
        [SerializeField] 
        public TypedEnumKey enumKey;
        
        // 비 직렬화 항목 : 캐시 키
        [NonSerialized] private string _cachedKey; 
        
        // 키의 경우 BuildKey로 만든 키 값을 캐시화 하여 제공 
        public string Key => _cachedKey ??= BuildKey();

        // QuestKeys.Compose를 통해서 Key 값을 만들고 정규화를 진행
        private string BuildKey() => Normalize(QuestKeys.Compose(keyValue, enumKey));

        // 입력받은 키와 현재 목표의 키가 동일한지 판별하여 참 거짓을 반환
        public bool Matches(string signalKey) => string.Equals(Normalize(signalKey), Key, StringComparison.Ordinal);
        

        // 목표와 키가 동일하다면 마릿수 반환, 아니라면 진행 안된 사실 반환
        public int ProgressDeltaFrom(string signalKey, int delta) => Matches(signalKey) ? delta : 0;

        // 문자열을 받아 공백 삭제, 소문자화 진행
        private static string Normalize(string s) => (s ?? "").Trim().ToLowerInvariant();
        
#if UNITY_EDITOR
        private void OnValidate() { _cachedKey = null; }
#endif
        private void OnEnable() { _cachedKey = null; }
    } 
}