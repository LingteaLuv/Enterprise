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
        StatEvents.OnBattlePowerChanged += ShowBattlePowerChange;
    }

    private void OnDisable()
    {
        StatEvents.OnBattlePowerChanged -= ShowBattlePowerChange;
    }

    private void ShowBattlePowerChange(BigInt oldPower, BigInt newPower)
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

        popupText.text = $"{DataUtility.FormatNumber(oldPower)} → {DataUtility.FormatNumber(newPower)}";
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
