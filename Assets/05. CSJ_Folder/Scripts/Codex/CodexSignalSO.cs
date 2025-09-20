using System;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Codex
{
    [CreateAssetMenu(menuName = "Codex/Signal")]
    public class CodexSignalSO : ScriptableObject
    {
        public Action<Faction, CodexStd_Enum, int> OnSignal;
        public Action<CodexInstance> OnSignalComplete;

        // Type을 받아서 해당 신호의 Count를 올리는 함수
        public void Raise(Faction faction, CodexStd_Enum codexStd, int signalNum) => OnSignal?.Invoke(faction, codexStd, signalNum);
        
        public void OnComplete(CodexInstance inst) => OnSignalComplete?.Invoke(inst);
    }
}