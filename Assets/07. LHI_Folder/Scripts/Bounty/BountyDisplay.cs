using System.Numerics;
using UnityEngine;

/// <summary>
/// 현상금(전투력) UI 표시를 담당하는 MonoBehaviour 클래스
/// </summary>
public class BountyDisplay : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TMPro.TextMeshProUGUI teamBountyText;   // 팀 전체 현상금 표시 텍스트
    [SerializeField] private TMPro.TextMeshProUGUI bountyChangeText; // 현상금 변경량 표시 텍스트
    [SerializeField] private float changeTextDisplayDuration = 2f;   // 변경량 텍스트 표시 시간

    private Coroutine hideChangeTextCoroutine;

    private void OnEnable()
    {
        // 이벤트 구독
        StatEvents.OnTeamBattlePowerChanged += OnTeamBountyChanged;
        StatEvents.OnCharacterBattlePowerChanged += OnCharacterBountyChanged;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        StatEvents.OnTeamBattlePowerChanged -= OnTeamBountyChanged;
        StatEvents.OnCharacterBattlePowerChanged -= OnCharacterBountyChanged;
    }

    private void Start()
    {
        // 초기 현상금 표시
        if (PlayerDataManager.Instance != null)
        {
            UpdateBountyDisplay(PlayerDataManager.Instance.teamBattlePower);
        }
    }

    /// <summary>
    /// 팀 전체 현상금이 변경되었을 때 호출
    /// </summary>
    private void OnTeamBountyChanged(BigInteger oldPower, BigInteger newPower)
    {
        UpdateBountyDisplay(newPower);
        ShowBountyChange(oldPower, newPower);
    }

    /// <summary>
    /// 개별 캐릭터의 현상금이 변경되었을 때 호출
    /// </summary>
    private void OnCharacterBountyChanged(PlayerCharacterData character, BigInteger oldPower, BigInteger newPower)
    {
        // 개별 캐릭터 변경 시 특별한 UI 효과를 추가하고 싶다면 여기에 구현
        // 예: 현상금 상승 축하 애니메이션 등
        Debug.Log($"[현상금 변경] {character.characterdata.characterName}: {oldPower} → {newPower}");
    }

    /// <summary>
    /// 현상금 UI를 업데이트
    /// </summary>
    private void UpdateBountyDisplay(BigInteger bounty)
    {
        if (teamBountyText != null)
        {
            teamBountyText.text = $"{bounty}";
        }
    }

    /// <summary>
    /// 현상금 변경량을 표시
    /// </summary>
    private void ShowBountyChange(BigInteger oldValue, BigInteger newValue)
    {
        if (bountyChangeText == null) return;

        BigInteger change = newValue - oldValue;

        if (change > 0)
        {
            bountyChangeText.text = $"{change}";
            bountyChangeText.color = Color.green;
        }
        else if (change < 0)
        {
            bountyChangeText.text = $"{change}";
            bountyChangeText.color = Color.red;
        }
        else
        {
            return; // 변경 없음
        }

        bountyChangeText.gameObject.SetActive(true);

        // 이전 코루틴 중지
        if (hideChangeTextCoroutine != null)
        {
            StopCoroutine(hideChangeTextCoroutine);
        }

        // 일정 시간 후 텍스트 숨기기
        hideChangeTextCoroutine = StartCoroutine(HideChangeTextAfterDelay());
    }

    private System.Collections.IEnumerator HideChangeTextAfterDelay()
    {
        yield return new WaitForSeconds(changeTextDisplayDuration);
        if (bountyChangeText != null)
        {
            bountyChangeText.gameObject.SetActive(false);
        }
    }
    
}
