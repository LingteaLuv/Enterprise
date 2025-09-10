using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/Definition/QuestDefault")]
    public class QuestDefinitionSO : ScriptableObject
    {
        [Header("퀘스트 내용")] 
        // quest 이름
        public string questName;
        // 퀘스트의 보상
        public QuestRewardSO Reward;

        // 프로퍼티
        /// <summary>
        /// 일반 퀘스트인지 반환
        /// </summary>
        public bool isGeneral => this is GeneralQuestDefinitionSO;

        public bool isTemporary => this is TemporaryQuestDefinitionSO;
        
    }
}