using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.UI
{
    public class QuestUIBinder : MonoBehaviour
    {
        [SerializeField] GameObject questUI;
        [SerializeField] TemporaryQuestController temporaryQuestController;

        void Awake()
        {
            if(QuestManager.Instance is null) return;
            QuestManager.Instance.BindUI(questUI, temporaryQuestController);
        }

        private void OnDestroy()
        {
            if(QuestManager.Instance is null) return;
            QuestManager.Instance.UnBindUI();
        }
    }
}
