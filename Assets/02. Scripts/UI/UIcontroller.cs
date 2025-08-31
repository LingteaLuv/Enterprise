using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController<T> : MonoBehaviour where T : Enum
{
    [SerializeField] protected List<UIBase> _uiList;
    
    /// <summary>
    /// 패널을 여는 메서드
    /// </summary>
    /// <param name="type"></param>
    protected void ShowUI(T type)
    {
        int index = Convert.ToInt32(type);
        if (index >= 0 && index < _uiList.Count)
        {
            _uiList[index].SetShow();
        }
        else
        {
            Debug.LogWarning($"ShowUI : {type} 설정 안됨");
        }
    }

    /// <summary>
    /// 패널을 숨기는 메서드
    /// </summary>
    /// <param name="type"></param>
    protected void HideUI(T type)
    {
        int index = Convert.ToInt32(type);
        if (index >= 0 && index < _uiList.Count)
        {
            _uiList[index].SetHide();
        }
        else
        {
            Debug.LogWarning($"HideUI : {type} 설정 안됨");
        }
    }
}
