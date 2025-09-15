using DG.Tweening.Core.Easing;
using UnityEngine;

public class GlobalStageManager : MonoBehaviour
{
    public static GlobalStageManager Instance;

    public Property<int> CurrentStageIndex { get; private set; }
    public Property<int> CurrentIslandIndex { get; private set; }
    
    public bool IsChecked { get; private set; }

    public bool bossBattleTriggered = false;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환에도 유지

        if(!await DatabaseManager.Instance.CheckFieldAsync("StageData/Stage", (long value) =>
           {
               CurrentStageIndex = new Property<int>((int)value);
           }))
        {
            CurrentStageIndex = new Property<int>(1);
            await DatabaseManager.Instance.SaveFieldAsync("StageData/Stage", 1);
        }
        
        CurrentStageIndex.OnChanged += async (value) =>
        {
            await DatabaseManager.Instance.SaveFieldAsync("StageData/Stage", value);
        };
        
        if(!await DatabaseManager.Instance.CheckFieldAsync("StageData/Island", (long value) =>
           {
               CurrentIslandIndex = new Property<int>((int)value);
               IsChecked = true;
           }))
        {
            CurrentIslandIndex = new Property<int>(0);
            await DatabaseManager.Instance.SaveFieldAsync("StageData/Island", 0);
            IsChecked = true;
        }
        
        CurrentIslandIndex.OnChanged += async (value) =>
        {
            await DatabaseManager.Instance.SaveFieldAsync("StageData/Island", value);
        };
    }
}
