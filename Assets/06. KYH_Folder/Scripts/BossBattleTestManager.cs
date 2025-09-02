using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BossBattleTestManager : MonoBehaviour
{
    [SerializeField] private Button _successBtn;
    [SerializeField] private Button _failBtn;
    
    private string returnSceneName = "Game";
        
    private void Update()
    {
        _successBtn.onClick.AddListener(() =>
        {
            Debug.Log(" 보스전 승리!");
            GlobalStageManager.Instance.currentStageIndex++;
            GlobalStageManager.Instance.bossBattleTriggered = false;

            SceneManager.LoadScene(returnSceneName);
        });
        
        _failBtn.onClick.AddListener(() =>
        {
            Debug.Log(" 보스전 패배!");
            SceneManager.LoadScene(returnSceneName);
        });
    }
}
