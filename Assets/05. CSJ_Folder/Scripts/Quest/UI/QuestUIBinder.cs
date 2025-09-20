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

        void Awake()
        {
            if(QuestManager.Instance is null) return;
            if (questUI is null || temporaryQuestController is null) 
                QuestManager.Instance.BindUI(questUI, temporaryQuestController);

            if (CodexManager.Instance is null) return;
            if (codexUIController is null) return;
            CodexManager.Instance.BindUI(codexUIController);
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
