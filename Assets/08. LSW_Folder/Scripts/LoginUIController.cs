using System.Collections.Generic;
using UnityEngine;

public class LoginUIController : UIController<LoginUIController.LoginUIType>
{
    public enum LoginUIType
    {
        StartPanel,
        LoginPanel,
        InfoPanel,
        LoadingPanel
    }

    private void Start()
    {
        foreach (var ui in _uiList)
        {
            if (ui is StartPanel startPanel)
            {
                // 팝업 닫기 버튼
                startPanel.OnTouchStartBtn = () =>
                {
                    HideUI(LoginUIType.StartPanel);
                    ShowUI(LoginUIType.LoginPanel);
                };
            }
        }
    }
}
