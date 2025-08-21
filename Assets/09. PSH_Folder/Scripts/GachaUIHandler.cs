using UnityEngine;

public class GachaUIHandler : MonoBehaviour
{
    public GachaManager gachaManager;
    public GachaListUI gachaListUI;

    /// <summary>
    /// 1회 뽑기 버튼
    /// </summary>
    public void OnGachaButtonPressed()
    {
        if (gachaManager == null)
        {
            Debug.LogError("GachaUIHandler에 GachaManager가 연결되지 않았습니다!");
            return;
        }

        // 확인창 호출
        UIManager.Instance.ShowConfirm(
            "정말 1회 뽑기를 진행하시겠습니까?",
            onConfirm: () =>
            {
                if (gachaManager.PerformSingleGacha())
                {
                    gachaListUI.gameObject.SetActive(true);
                    Debug.Log("UI 버튼 클릭으로 1회 뽑기를 실행했습니다.");
                }
                else
                {
                    UIManager.Instance.ShowWarning("젬이 부족합니다.");
                }
            },
            onCancel: () =>
            {
                UIManager.Instance.ShowWarning("젬이 부족합니다.");
                Debug.Log("1회 뽑기 취소됨");
            }
        );
    }

    /// <summary>
    /// 10연차 버튼
    /// </summary>
    public void OnGachaButtonPressed_10_Times()
    {
        if (gachaManager == null)
        {
            Debug.LogError("GachaUIHandler에 GachaManager가 연결되지 않았습니다!");
            return;
        }

        // 확인창 호출
        UIManager.Instance.ShowConfirm(
            "정말 10회 연속 뽑기를 진행하시겠습니까?",
            onConfirm: () =>
            {
                if (gachaManager.PerformMultipleGacha(10))
                {
                    gachaListUI.gameObject.SetActive(true);
                    Debug.Log("UI 버튼 클릭으로 10회 뽑기를 실행했습니다.");
                }
                else
                {
                    UIManager.Instance.ShowWarning("젬이 부족합니다.");
                }
            },
            onCancel: () =>
            {
                Debug.Log("10연차 뽑기 취소됨");
            }
        );
    }
}
