using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class BossBattleTestManager : MonoBehaviour
{
    [SerializeField] private string returnSceneName = "0828Demo";
    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("❌ 보스전 패배!");
            SceneManager.LoadScene(returnSceneName);
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log("✅ 보스전 승리!");
            GlobalStageManager.Instance.currentStageIndex++;
            GlobalStageManager.Instance.bossBattleTriggered = false;

            SceneManager.LoadScene(returnSceneName);
        }
    }
}
