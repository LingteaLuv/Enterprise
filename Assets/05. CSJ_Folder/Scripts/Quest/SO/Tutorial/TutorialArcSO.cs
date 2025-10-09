using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.SO.Tutorial
{
    [CreateAssetMenu(menuName = "Tutorial/Arc")]
    public class TutorialArcSO : ScriptableObject
    {
        [Header("ID (저장용)")]
        public string arcId;
        
        [Header("해당 아크의 튜토리얼 시퀀스")]
        public TutorialStepSequenceSO Sequence;
        
        [Header("연결된 다음 아크")]
        public TutorialArcSO NextArc;
    }
}