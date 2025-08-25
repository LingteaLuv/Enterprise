using System.Collections;
using TMPro;
using UnityEngine;
using BigInt = System.Numerics.BigInteger;

public class BattlePowerPopup : MonoBehaviour
{
    [Header("UI 연결")]
    [Tooltip("실제 움직이며 표시될 텍스트 UI 요소")]
    public TextMeshProUGUI popupText;
    [Tooltip("팝업이 나타나기 시작할 위치를 제공하는 UI 요소 (예: 팀 전투력 텍스트)")]
    public Transform spawnTarget;

    private Coroutine popupCoroutine; // 현재 실행 중인 코루틴 저장

    private void OnEnable()
    {
        StatEvents.OnTeamBattlePowerChanged += ShowTeamBattlePowerChange;
    }

    private void OnDisable()
    {
        StatEvents.OnTeamBattlePowerChanged -= ShowTeamBattlePowerChange;
    }

    private void ShowTeamBattlePowerChange(BigInt oldPower, BigInt newPower)
    {
        if (spawnTarget == null)
        {
            Debug.LogError("BattlePowerPopup에 spawnTarget이 연결되지 않았습니다! 팝업을 표시할 수 없습니다.");
            return;
        }

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

        // 텍스트를 '팀 전투력'으로 변경하고, 증감에 따라 텍스트 표시
        bool sgn = newPower > oldPower;
        BigInt battlePoint = sgn ? newPower - oldPower : oldPower - newPower;
        Color diffColor = sgn ? Color.blue : Color.red;

        popupText.text = $"팀 전투력 : {DataUtility.FormatNumber(newPower)} " +
                         $"\n(<color=#{ColorUtility.ToHtmlStringRGB(diffColor)}>{(sgn ? "+" : "-")}{DataUtility.FormatNumber(battlePoint)}</color>)";

        // 애니메이션 시작 위치를 spawnTarget의 현재 위치로 설정
        Vector3 animationStartPos = spawnTarget.position;
        popupText.transform.position = animationStartPos;

        // 위로 50픽셀만큼 이동 (캔버스 설정에 따라 거리가 달라 보일 수 있음)
        Vector3 endPos = animationStartPos + new Vector3(0, 50f, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 위치 부드럽게 보간
            popupText.transform.position = Vector3.Lerp(animationStartPos, endPos, t);

            // 투명도 페이드 아웃
            var color = popupText.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            popupText.color = color;

            yield return null;
        }

        // 애니메이션이 끝나면 텍스트를 비우고 알파값을 복원
        popupText.text = "";
        var finalColor = popupText.color;
        finalColor.a = 1f;
        popupText.color = finalColor;
    }
}
