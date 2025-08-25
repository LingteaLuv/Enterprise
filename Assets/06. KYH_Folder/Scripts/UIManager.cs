using UnityEngine;
using System.Collections;
using System;
public class IngameUIManager : MonoBehaviour
{
    // 싱글톤 인스턴스 설정 (전역 접근을 위한 static 인스턴스)
    public static IngameUIManager Instance { get; private set; }

    // 각 UI 패널들을 인스펙터에서 할당할 수 있도록 SerializeField로 설정
    [SerializeField] private GameObject lobbyUI;       // 로비(메인) UI 패널
    [SerializeField] private GameObject battleUI;      // 전투 UI 패널
    [SerializeField] private GameObject rewardUI;      // 보상 UI 패널
    [SerializeField] private GameObject stageClearUI;  // 스테이지 클리어 UI 패널

    // 초기화 시 싱글톤 인스턴스 지정
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 전투 화면 UI 전환 로직
    /// - 로비 UI 끄고, 전투 UI 켜고, 보상 UI 끔
    /// </summary>
    public void ShowBattleUI()
    {
       // lobbyUI.SetActive(false);   // 로비 UI 비활성화
       // battleUI.SetActive(true);   // 전투 UI 활성화
       // rewardUI.SetActive(false);  // 보상 UI 비활성화
    }

    /// <summary>
    /// 보상 UI를 보여주고 일정 시간 후 콜백을 실행하는 메서드
    /// </summary>
    /// <param name="onRewardComplete">보상 연출이 끝났을 때 실행할 콜백 함수</param>
    public void ShowRewardUI(Action onRewardComplete)
    {
        // 코루틴을 통해 연출 시간 동안 기다렸다가 콜백 실행
        StartCoroutine(RewardSequence(onRewardComplete));
    }

    /// <summary>
    /// 보상 연출 처리 코루틴
    /// </summary>
    /// <param name="onRewardComplete">연출 끝난 뒤 호출할 콜백</param>
    private IEnumerator RewardSequence(Action onRewardComplete)
    {
        battleUI.SetActive(false);   // 전투 UI 끔
        rewardUI.SetActive(true);    // 보상 UI 켬

        yield return new WaitForSeconds(2f); // 2초 동안 보상 UI 유지 (연출 시간)

        rewardUI.SetActive(false);   // 보상 UI 끔

        // 콜백이 null이 아닐 경우에만 실행 (Null-safe)
        onRewardComplete?.Invoke();
    }

    /// <summary>
    /// 스테이지 완료 시 보여줄 UI 처리
    /// </summary>
    public void ShowStageComplete()
    {
        stageClearUI.SetActive(true); // 스테이지 클리어 UI 켬
    }
}
