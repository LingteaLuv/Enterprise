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

            // 스테이지 클리어 퀘스트 기록
            QuestSignalManager.Instance.StageClear(GlobalStageManager.Instance.CurrentStageIndex.Value);

            if (GlobalStageManager.Instance.CurrentStageIndex.Value < 4)
            {
                // 1~3스테이지는 정상적으로 증가
                GlobalStageManager.Instance.CurrentStageIndex.Value++;
            }
            else
            {
                // 4스테이지는 증가시키지 않고 4로 고정
                GlobalStageManager.Instance.CurrentStageIndex.Value = 4;
            }

            // 다음 루프는 항상 첫 섬부터 시작
            GlobalStageManager.Instance.CurrentIslandIndex.Value = 0;

            // 보스전 플래그 초기화
            GlobalStageManager.Instance.bossBattleTriggered = false;

            QuestSignalManager.Instance.KillEnemy(MonsterId.Boss);

            StartCoroutine(Delay1());
        });

        _failBtn.onClick.AddListener(() =>
        {
            Debug.Log(" 보스전 패배!");
            GlobalStageManager.Instance.CurrentIslandIndex.Value = 0;
            GlobalStageManager.Instance.bossBattleTriggered = false; // 추가
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
        BossBattleManager.IsBossBattle = false;
    }
    
    private IEnumerator Delay1()
    {
        _successBtn.gameObject.SetActive(false);
        _failBtn.gameObject.SetActive(false);
        yield return new WaitForSeconds(8f);
        SceneManager.LoadScene(_returnSceneName);
        BossBattleManager.IsBossBattle = false;
    }
    
    private IEnumerator Delay2()
    {
        _successBtn.gameObject.SetActive(false);
        _failBtn.gameObject.SetActive(false);
        yield return new WaitForSeconds(8f);
        SceneManager.LoadScene(_returnSceneName);
        BossBattleManager.IsBossBattle = false;
    }
}
