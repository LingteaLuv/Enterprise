using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 배 이동만 담당하는 컨트롤러
/// - 특정 위치로 이동 (MoveToPosition)
/// - 대기(정박) 처리 (WaitBeforeReturn)
/// 
/// IslandStageManager가 이 ShipController를 호출해서
/// 배 이동 → 섬 도착 → 전투 시작 의 흐름을 구성한다.
/// </summary>
public class ShipController : MonoBehaviour
{
    [SerializeField] private Transform shipTransform; // 실제로 움직일 배 오브젝트(Transform)

    /// <summary>
    /// 배를 targetPos까지 duration 동안 이동시킨다.
    /// 이동이 완료되면 onArrive 콜백 실행.
    /// 
    /// - start: 현재 배 위치
    /// - end: 도착해야 할 목표 위치
    /// - duration: 이동에 걸리는 시간
    /// </summary>
    public IEnumerator MoveToPosition(Vector3 targetPos, float duration, Action onArrive)
    {
        Vector3 start = shipTransform.position; // 시작 위치
        float t = 0f;                           // 시간 누적값

        // duration 동안 Lerp로 선형 보간
        while (t < duration)
        {
            shipTransform.position = Vector3.Lerp(start, targetPos, t / duration);
            t += Time.deltaTime;
            yield return null; // 프레임마다 진행
        }

        // 마지막 위치 보정
        shipTransform.position = targetPos;

        // 도착 후 콜백 실행 (예: 섬 전투 시작)
        onArrive?.Invoke();
    }

    /// <summary>
    /// 배가 섬 앞에서 대기하는 시간 연출용
    /// (예: 전투 종료 후 잠깐 정박했다가 다음 섬으로 이동)
    /// </summary>
    public IEnumerator WaitBeforeReturn(float delay = 1f)
    {
        yield return new WaitForSeconds(delay);
    }
}
