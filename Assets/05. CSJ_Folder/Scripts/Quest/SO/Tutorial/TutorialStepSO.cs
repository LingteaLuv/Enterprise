using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.SO.Tutorial
{
    [CreateAssetMenu(menuName = "Tutorial/step")]
    public class TutorialStepSO : ScriptableObject
    {
        [Header("Step Type")]
        public TutorialStepType type;

        [Header("contents")]
        [TextArea] public string text;
        
        [Header("딜레이 시간")]
        public int delay;
        
        [Header("필요한 target 키")]
        // 추후 enum으로 전환할 수도
        public string targetKey;
        
        [Header("튜토리얼 퀘스트 (연결되어 있을 시)")]
        public TutorialSignalSO signal;
        public TutorialQuestDefinitionSO quest;

        public void TutorialQuestClear()
        {
            signal.OnComplete(quest.Goal.enumKey.ToString());
        }

    }
}