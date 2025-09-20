using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Codex
{
    public class CodexSiganlManager : Singleton<CodexSiganlManager>
    {
        [SerializeField] private CodexSignalSO _signal;

        public void RaiseLevelSignal(Faction faction, int amount)
        {
            _signal.Raise(faction, CodexStd_Enum.Level, amount);
        }
        
        public void RaiseRankSignal(Faction faction, int amount)
        {
            _signal.Raise(faction, CodexStd_Enum.Rank, amount);
        }
        
        public void OnCompleteQuest(CodexInstance inst)
        {
            _signal.OnComplete(inst);
        }
    }
}