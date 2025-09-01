using DG.Tweening.Core.Easing;
using UnityEngine;

public class GlobalStageManager : MonoBehaviour
{
    public static GlobalStageManager Instance;

    public int currentStageIndex = 1;
    public bool bossBattleTriggered = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
    }
}
