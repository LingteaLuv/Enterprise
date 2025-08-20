using System.Collections;
using TMPro;
using UnityEngine;
using BigInt = System.Numerics.BigInteger;

public class BattlePowerPopup : MonoBehaviour
{
    public TextMeshProUGUI popupText;
    private Vector3 startPos;
    private Coroutine popupCoroutine; // 현재 실행 중인 코루틴 저장

    private void Start()
    {
        startPos = popupText.transform.position; // 초기 위치 기억
    }
    private void OnEnable()
    {
        // 팀 전투력 변경 이벤트를 구독하도록 수정
        StatEvents.OnTeamBattlePowerChanged += ShowTeamBattlePowerChange;
    }

    private void OnDisable()
    {
        // 팀 전투력 변경 이벤트 구독을 해제하도록 수정
        StatEvents.OnTeamBattlePowerChanged -= ShowTeamBattlePowerChange;
    }

    private void ShowTeamBattlePowerChange(BigInt oldPower, BigInt newPower)
    {
        // 이미 실행 중인 코루틴이 있으면 중단
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
        }
        // 새 코루틴 실행
        popupCoroutine = StartCoroutine(PopupCoroutine(oldPower, newPower));
    }

    private IEnumerator PopupCoroutine(BigInt oldPower, BigInt newPower)
    {
        float duration = 1.0f;
        float elapsed = 0f;

        // 텍스트를 '팀 전투력'으로 변경하고, 증가량을 더 명확하게 표시
        popupText.text = $"팀 전투력 : {DataUtility.FormatNumber(newPower)} (+{DataUtility.FormatNumber(newPower - oldPower)})";
        popupText.transform.position = startPos;

        Vector3 endPos = startPos + new Vector3(0, 50f, 0); // 위로 50만큼 이동

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 위치 부드럽게 보간
            popupText.transform.position = Vector3.Lerp(startPos, endPos, t);

            // 투명도 페이드 아웃 (선택)
            var color = popupText.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            popupText.color = color;

            yield return null;
        }

        popupText.text = "";
        popupText.color = Color.white; // 알파 초기화
    }
}
