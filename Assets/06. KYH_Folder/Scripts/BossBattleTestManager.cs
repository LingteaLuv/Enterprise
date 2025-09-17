using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossBattleTestManager : MonoBehaviour
{
    [SerializeField] private Button _successBtn;
    [SerializeField] private Button _failBtn;
    [SerializeField] private Button _startBtn;
    
    private readonly string _returnSceneName = "Game";
    
    
    private void Start()
    {
        _successBtn.onClick.AddListener(() =>
        {
            Debug.Log(" 보스전 승리!");

            QuestSignalManager.Instance.StageClear(GlobalStageManager.Instance.CurrentStageIndex.Value++);
            GlobalStageManager.Instance.CurrentIslandIndex.Value = 0;

            GlobalStageManager.Instance.bossBattleTriggered = false;
            QuestSignalManager.Instance.KillEnemy(MonsterId.Boss);
            StartCoroutine(Delay1());
        });
        
        _failBtn.onClick.AddListener(() =>
        {
            Debug.Log(" 보스전 패배!");
            GlobalStageManager.Instance.CurrentIslandIndex.Value = 0;
            StartCoroutine(Delay2());
        });
        
        _startBtn.onClick.AddListener(() =>
        {
            StartCoroutine(Delay());
        });
        
        _successBtn.gameObject.SetActive(false);
        _failBtn.gameObject.SetActive(false);
    }

    private IEnumerator Delay()
    {
        _startBtn.gameObject.SetActive(false);
        yield return new WaitForSeconds(8f);
        _successBtn.gameObject.SetActive(true);
        _failBtn.gameObject.SetActive(true);
    }
    
    private IEnumerator Delay1()
    {
        _successBtn.gameObject.SetActive(false);
        _failBtn.gameObject.SetActive(false);
        yield return new WaitForSeconds(8f);
        SceneManager.LoadScene(_returnSceneName);
    }
    
    private IEnumerator Delay2()
    {
        _successBtn.gameObject.SetActive(false);
        _failBtn.gameObject.SetActive(false);
        yield return new WaitForSeconds(8f);
        SceneManager.LoadScene(_returnSceneName);
    }
}
