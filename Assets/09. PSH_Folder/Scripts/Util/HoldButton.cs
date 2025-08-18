using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI; // UnityEngine.UI 네임스페이스 추가
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
    private Button _button; // UnityEngine.UI.Button 컴포넌트 참조

    // interactable 속성 추가
    public bool interactable
    {
        get { return _button != null ? _button.interactable : true; }
        set
        {
            if (_button != null)
            {
                _button.interactable = value;
            }
        }
    }

    private void Awake()
    {
        _button = GetComponent<Button>(); // Awake에서 Button 컴포넌트 가져오기
        if (_button == null)
        {
            Debug.LogWarning("HoldButton: Button 컴포넌트를 찾을 수 없습니다. interactable 속성이 작동하지 않을 수 있습니다.");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return; // interactable이 false면 동작하지 않음

        isPointerDown = true;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
        }
        holdCoroutine = StartCoroutine(HoldActionCoroutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return; // interactable이 false면 동작하지 않음

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