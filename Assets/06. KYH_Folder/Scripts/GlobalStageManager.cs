using DG.Tweening.Core.Easing;
using UnityEngine;

public class GlobalStageManager : MonoBehaviour
{
    public static GlobalStageManager Instance;

    public Property<int> currentStageIndex;
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
               currentStageIndex = new Property<int>((int)value);
           }))
        {
            currentStageIndex = new Property<int>(1);
            await DatabaseManager.Instance.SaveFieldAsync("StageData/Stage", 1);
        }
        
        currentStageIndex.OnChanged += async (value) =>
        {
            await DatabaseManager.Instance.SaveFieldAsync("StageData/Stage", value);
        };
    }
}
