using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.SO.Tutorial;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public enum TutorialTrigger {OnAppReady, OnQuestActivated}
    
    [System.Serializable]
    public class TutorialQuestLink
    {
        public TutorialQuestDefinitionSO quest;
        public TutorialTrigger trigger = TutorialTrigger.OnQuestActivated;
        public TutorialArcSO arc;
    }
}