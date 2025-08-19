
using UnityEngine;

public class GachaUIHandler : MonoBehaviour
{
    // 인스펙터에서 GachaManager를 연결해줘야 합니다.
    public GachaManager gachaManager;

    /// <summary>
    /// 1회 뽑기 버튼에 연결할 함수입니다.
    /// </summary>
    public void OnGachaButtonPressed()
    {
        if (gachaManager != null)
        {
            gachaManager.PerformSingleGacha();
            Debug.Log("UI 버튼 클릭으로 1회 뽑기를 실행했습니다.");
        }
        else
        {
            Debug.LogError("GachaUIHandler에 GachaManager가 연결되지 않았습니다!");
        }
    }

    /// <summary>
    /// 10연차 버튼에 연결할 함수입니다.
    /// </summary>
    public void OnGachaButtonPressed_10_Times()
    {
        if (gachaManager != null)
        {
            gachaManager.PerformMultipleGacha(10);
            Debug.Log("UI 버튼 클릭으로 10회 뽑기를 실행했습니다.");
        }
        else
        {
            Debug.LogError("GachaUIHandler에 GachaManager가 연결되지 않았습니다!");
        }
    }
}
