using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬(또는 특정 부모) 내의 PaperFlip들을 관리.
/// UI Button의 OnClick에 FlipAllRemaining() 연결해서
/// 아직 안 뒤집힌 카드만 일괄 뒤집기.
/// </summary>
public class PaperFlipGroupController : MonoBehaviour
{
    [Tooltip("비우면 씬의 모든 PaperFlip을 자동 수집")]
    public List<PaperFlip> flips;

    [Tooltip("특정 부모 밑의 카드만 관리하려면 지정")]
    public Transform collectUnderParent;

    void Reset() => AutoCollect();

    public void AutoCollect()
    {
        if (collectUnderParent)
            flips = new List<PaperFlip>(collectUnderParent.GetComponentsInChildren<PaperFlip>(true));
        else
            flips = new List<PaperFlip>(FindObjectsOfType<PaperFlip>(true));
    }

    /// <summary>아직 뒤집히지 않은 카드만 실행</summary>
    public void FlipAllRemaining()
    {
        if (flips == null || flips.Count == 0) AutoCollect();
        foreach (var f in flips)
        {
            if (!f) continue;
            f.TryPlayFlip(); // 진행 중/완료 카드는 내부 가드로 자동 무시
        }
    }

    /// <summary>모든 카드 상태 초기화(테스트/리플레이용)</summary>
    public void ResetAll()
    {
        if (flips == null || flips.Count == 0) AutoCollect();
        foreach (var f in flips) if (f) f.ResetFlipState();
    }
}
