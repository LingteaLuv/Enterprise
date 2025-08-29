using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolPanel : UIBase
{
    [SerializeField] private List<Button> _toolBtnZip;

    public Action<int> OnButtonTouched;

    private void Start()
    {
        for (int i = 0; i < _toolBtnZip.Count; i++)
        {
            int index = i;
            _toolBtnZip[i].onClick.AddListener(() =>
            {
                OnButtonTouched(index);
            });
        }
    }
}
