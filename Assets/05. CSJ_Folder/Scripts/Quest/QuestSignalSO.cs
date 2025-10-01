using System;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    [CreateAssetMenu(menuName = "Quest/Signal")]
    public class QuestSignalSO : ScriptableObject
    {
        // 신호를 보내는 이벤트
        public Action<QuestType_Enum, string, int> OnSignal;
        
        public Action<string> OnSignalComplete; 
        
        public Action<string> OnSignalFailed;

        // Type을 받아서 해당 신호의 Count를 올리는 함수
        public void Raise(QuestType_Enum questType, string signalName, int signalNum) => OnSignal?.Invoke(questType, signalName, signalNum);
        
        public void OnComplete(string QuestId) => OnSignalComplete?.Invoke(QuestId);
        
        public void OnFailed(string QuestId) => OnSignalFailed?.Invoke(QuestId);
    }
}