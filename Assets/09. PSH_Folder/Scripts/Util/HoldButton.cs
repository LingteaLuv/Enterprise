using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 버튼을 꾹 누르면 연결된 함수를 반복적으로 호출하는 기능을 제공합니다.
/// 터치할 때 한 번, 딜레이 후 반복적으로 실행.
/// </summary>
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("반복 실행을 시작하기 전까지의 대기 시간(초)")]
    public float holdDelay = 0.5f;

    [Tooltip("반복 실행 사이의 시간 간격(초)")]
    public float repeatInterval = 0.1f;

    public UnityEvent onHoldAction;

    private bool isPointerDown = false;
    private Coroutine holdCoroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }
        holdCoroutine = StartCoroutine(HoldActionCoroutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }
    }

    private IEnumerator HoldActionCoroutine()
    {
        if (onHoldAction != null)
        {
            onHoldAction.Invoke();
        }

        yield return new WaitForSeconds(holdDelay);

        while (isPointerDown)
        {
            if (onHoldAction != null)
            {
                onHoldAction.Invoke();
            }

            yield return new WaitForSeconds(repeatInterval);
        }
    }

    private void OnDisable()
    {
        isPointerDown = false;
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }
    }
}