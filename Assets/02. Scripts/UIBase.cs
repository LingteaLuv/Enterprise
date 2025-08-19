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

    public virtual void RefreshUI() { }
}
