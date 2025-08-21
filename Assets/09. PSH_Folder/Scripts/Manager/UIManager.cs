using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : Singleton<UIManager>
{
    [Header("Popup Objects")]
    [SerializeField] private GameObject warningPopup;
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private GameObject toastPopup;

    [Header("Popup UI Elements")]
    [SerializeField] private TextMeshProUGUI warningMessageText;
    [SerializeField] private TextMeshProUGUI confirmMessageText;
    [SerializeField] private TextMeshProUGUI toastMessageText;

    [SerializeField] private Button warningCloseButton;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    private Coroutine toastCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // 시작 시 다 끄기
        if (warningPopup) warningPopup.SetActive(false);
        if (confirmPopup) confirmPopup.SetActive(false);
        if (toastPopup) toastPopup.SetActive(false);
    }

    /// <summary> 단순 경고 팝업 </summary>
    public void ShowWarning(string message)
    {
        if (warningPopup == null) return;

        warningMessageText.text = message;
        warningPopup.SetActive(true);

        warningCloseButton.onClick.RemoveAllListeners();
        warningCloseButton.onClick.AddListener(() =>
        {
            warningPopup.SetActive(false);
        });
    }

    /// <summary> 확인/취소 팝업 </summary>
    public void ShowConfirm(string message, System.Action onConfirm, System.Action onCancel = null)
    {
        if (confirmPopup == null) return;

        confirmMessageText.text = message;
        confirmPopup.SetActive(true);

        confirmYesButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.RemoveAllListeners();

        confirmYesButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            confirmPopup.SetActive(false);
        });

        confirmNoButton.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            confirmPopup.SetActive(false);
        });
    }

    /// <summary> 일정 시간 후 사라지는 토스트 메시지 </summary>
    public void ShowToast(string message, float duration = 2f)
    {
        if (toastPopup == null) return;

        toastMessageText.text = message;
        toastPopup.SetActive(true);

        // 이전 토스트가 진행중이면 취소
        if (toastCoroutine != null)
            StopCoroutine(toastCoroutine);

        toastCoroutine = StartCoroutine(HideToastAfterDelay(duration));
    }

    private IEnumerator HideToastAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        toastPopup.SetActive(false);
    }
}
