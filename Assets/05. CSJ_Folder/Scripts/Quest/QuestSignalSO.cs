using System;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    [CreateAssetMenu(menuName = "Quest/Signal")]
    public class QuestSignalSO : ScriptableObject
    {
        public Action<QuestType_Enum, string, int> OnSignal;

        public void Raise(QuestType_Enum questType, string signalName, int signalNum) => OnSignal?.Invoke(questType, signalName, signalNum);
    }
}