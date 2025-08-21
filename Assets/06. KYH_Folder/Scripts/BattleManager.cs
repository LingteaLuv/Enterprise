using UnityEngine;
using System.Collections;
public class BattleManager : MonoBehaviour
{
    // 싱글톤 인스턴스 (전역 접근용)
    public static BattleManager Instance { get; private set; }

    private Coroutine battleRoutine;

    private void Awake()
    {
        Debug.Log(" BattleManager.Awake 호출됨");
        Instance = this; // 싱글톤 초기화
    }

    /// <summary>
    /// 외부에서 호출되는 전투 시작 메서드
    /// </summary>
    public void StartBattle()
    {
      //  Debug.Log(" StartBattle 진입함");
      //  Debug.Log($"battleRoutine == null? {(battleRoutine == null ? "YES" : "NO")}");
      //
      //  if (battleRoutine != null)
      //  {
      //      StopCoroutine(battleRoutine);
      //      Debug.LogWarning(" 이전 루틴 중지 후 battleRoutine = null 처리");
      //      battleRoutine = null;
      //  }

        battleRoutine = StartCoroutine(BattleRoutine());
        Debug.Log(" battleRoutine 시작됨");
    }

    /// <summary>
    /// 전투의 전체 흐름을 담당하는 메인 코루틴
    /// </summary>
    private IEnumerator BattleRoutine()
    {
        // 1. 전투 UI 표시 (Lobby → Battle 전환)
        //  UIManager.Instance.ShowBattleUI();

        Debug.Log("전투시작");
        // 2. 약간의 대기 시간 (적 활성화 등 준비 시간)
        yield return new WaitForSeconds(0.5f);

      //  // 3. 자동 전투 루프
      //  while (true)
      //  {
      //      // 모든 적이 제거되었는지 확인
      //      if (AllEnemiesDefeated())
      //          break;
      //
      //      // 유닛이 적을 찾아 이동 및 공격
      //      AutoControlUnits();
      //
      //      // 다음 프레임까지 대기
      //      yield return null;
      //  }
      //
        // 4. 전투 종료 후 잠깐 대기 (연출 여유)
        yield return new WaitForSeconds(1f);

        // 5. 보상 UI를 띄우고, 끝나면 다음 섬으로 이동
        //  UIManager.Instance.ShowRewardUI(() =>
        //  {
        //      IslandStageManager.Instance.OnBattleComplete(); // 보상 끝난 후 다음 섬 로드
        //  });

        battleRoutine = null;

        IslandStageManager.Instance.OnBattleComplete();
    }

    /// <summary>
    /// 아군 유닛의 자동 제어 루틴
    /// (예: 가장 가까운 적 찾아서 이동하고 공격하기)
    /// 실제 구현은 별도 유닛 AI에서 작성 예정
    /// </summary>
    private void AutoControlUnits()
    {
        // TODO: 유닛 개별 AI 호출 또는 통합 컨트롤러 구현 예정
        // 예: 각 유닛.FindNearestEnemy(), unit.MoveAndAttack()
    }

    /// <summary>
    /// 전투 중 모든 적이 제거되었는지 확인
    /// </summary>
    /// <returns>적이 하나도 없으면 true</returns>
    private bool AllEnemiesDefeated()
    {
        // "Enemy" 태그가 붙은 오브젝트가 없으면 true
        return GameObject.FindGameObjectsWithTag("Enemy").Length == 0;
    }
}
