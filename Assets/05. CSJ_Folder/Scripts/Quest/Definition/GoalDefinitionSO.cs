using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/Goals")]
    public class GoalDefinitionSO : ScriptableObject
    {
        // 퀘스트 목표의 키 ex) "kill : slime"
        [SerializeField] public string Key;
        // 퀘스트 설명 ex) 슬라임을 {RequireCount} 처치하세요.
        [TextArea] public string Description;
        // 퀘스트 목표 처치 수
        public int RequireCount = 1;
    }
}