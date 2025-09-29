using UnityEngine;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Codex.UI
{
    public class testButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private CodexSignalSO codexSiganlSO;
        
        public void Awake()
        {
            button.onClick.AddListener(() => CodexSiganlManager.Instance.RaiseLevelSignal(Faction.Pirate, 10));
            button.onClick.AddListener(() => CodexSiganlManager.Instance.RaiseRankSignal(Faction.Pirate, 2));
        }
    }
}