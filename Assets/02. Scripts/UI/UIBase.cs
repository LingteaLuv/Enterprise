using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public virtual void SetShow()
    {
        gameObject.SetActive(true);
        RefreshUI();
    }

    public virtual void SetHide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// UI의 내용을 새로고침할 때 호출됩니다. (예: 데이터 변경 시)
    /// </summary>
    public virtual void RefreshUI() { }

    /// <summary>
    /// 패널이 닫히고 상태를 초기화해야 할 때 호출됩니다.
    /// </summary>
    public virtual void ResetPanel()
    {
        // 각 패널에서 이 메서드를 오버라이드하여
        // 하위 패널을 닫거나 스크롤 위치를 초기화하는 등의 로직을 구현합니다.
    }
}
