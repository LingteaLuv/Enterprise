using _05._CSJ_Folder.Scripts.Quest;
using UnityEngine;

public class EncyclopediaSignalManager : Singleton<EncyclopediaSignalManager>
{
    [SerializeField] private QuestSignalSO signal;

    protected override void Awake()
    {
        base.Awake();
    }
}
