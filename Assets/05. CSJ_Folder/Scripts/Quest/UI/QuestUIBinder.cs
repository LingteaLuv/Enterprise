using _05._CSJ_Folder.Scripts.Codex;
using _05._CSJ_Folder.Scripts.Codex.UI;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.UI
{
    public class QuestUIBinder : MonoBehaviour
    {
        [SerializeField] GameObject questUI;
        [SerializeField] TemporaryQuestController temporaryQuestController;
        [SerializeField] CodexUIController codexUIController;
        [SerializeField] TutorialDirector tutorialDirector;

        void Awake()
        {
            if(QuestManager.Instance is null) return;
            if (questUI is not null && temporaryQuestController is not null)
            {
                if( tutorialDirector is null)
                    QuestManager.Instance.BindUI(questUI, temporaryQuestController);
                else
                    QuestManager.Instance.BindUI(questUI, temporaryQuestController, tutorialDirector);
            }


            //if (CodexManager.Instance is null) return;
            if (codexUIController is not null)
            {
                CodexManager.Instance.BindUI(codexUIController);
            }
        }

        private void OnDestroy()
        {
            if(QuestManager.Instance is null) return;
            QuestManager.Instance.UnBindUI();
            
            if (CodexManager.Instance is null) return;
            CodexManager.Instance.UnBindUI();
        }
    }
}
