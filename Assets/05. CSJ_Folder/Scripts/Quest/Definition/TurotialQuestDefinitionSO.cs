using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Definition
{
    [CreateAssetMenu(menuName = "Quest/Definition/General/TutorialQuest")]
    public class TutorialQuestDefinitionSO : GeneralQuestDefinitionSO
    {
        public int targetStage;
        
        // public TutorialSignalSO signal;
        //
        // public string[] questText;
        //
        // public void StartTutorial()
        // {
        //     signal.OnStart(this);
        //
        // public void CompleteTutorial()
        // {
        //     signal.OnComplete(Goal.enumKey.ToString());
        // }
    }
}