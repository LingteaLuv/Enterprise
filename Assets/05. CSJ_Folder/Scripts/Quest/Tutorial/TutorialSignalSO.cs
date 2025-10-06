

using System;
using _05._CSJ_Folder.Scripts.Quest;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using UnityEngine;

[CreateAssetMenu (menuName = "Quest/TutorialSignal")]
public class TutorialSignalSO : ScriptableObject
{
    
    public Action<TutorialQuestDefinitionSO> OnSignal;

    public void OnComplete(string tutorialName) => QuestSignalManager.Instance.Tutorial(tutorialName);
    
    public void OnStart(TutorialQuestDefinitionSO tutorial) => OnSignal?.Invoke(tutorial);
}