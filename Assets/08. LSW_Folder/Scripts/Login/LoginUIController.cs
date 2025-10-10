using System.Collections;
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
                startPanel.OnTouchStartBtn = async () =>
                {
                    if (await AuthManager.Instance.AutoLogin())
                    {
                        HideUI(LoginUIType.StartPanel);
                        ShowUI(LoginUIType.InfoPanel);
                        Debug.Log("자동 로그인 성공");
                    }
                    else
                    {
                        if (FirebaseManager.Auth != null)
                        {
                            FirebaseManager.Auth.SignOut();
                        }
                        HideUI(LoginUIType.StartPanel);
                        ShowUI(LoginUIType.LoginPanel);
                        Debug.Log("자동 로그인 실패 : 계정x , 게스트 계정");
                    }
                };
            }
            
            else if (ui is LoginPanel loginPanel)
            {
                loginPanel.OnLoginTried = () =>
                {
                    ShowUI(LoginUIType.LoadingPanel);
                    Debug.Log("로그인 시도 중");
                };
                loginPanel.OnLoginCompleted = () =>
                {
                    HideUI(LoginUIType.LoginPanel);
                    HideUI(LoginUIType.LoadingPanel);
                    ShowUI(LoginUIType.InfoPanel);
                    Debug.Log("로그인 성공");
                };
                loginPanel.OnLoginFailed = () =>
                {
                    HideUI(LoginUIType.LoadingPanel);
                    Debug.Log("로그인 실패");
                };
            }
            
            else if (ui is InfoPanel infoPanel)
            {
                infoPanel.OnGameStart = () =>
                {
                    HideUI(LoginUIType.InfoPanel);
                    SceneTransitionManager.Instance.LoadSceneWithLoading("LoadingScene", "Game", 2f);
                };
                infoPanel.OnGameExit = () =>
                {
                    HideUI(LoginUIType.InfoPanel);
                    ShowUI(LoginUIType.LoginPanel);
                };
            }
        }
        StartCoroutine(ShowStartPanel());
        
        HideUI(LoginUIType.LoginPanel);
        HideUI(LoginUIType.InfoPanel);
    }

    IEnumerator ShowStartPanel()
    {
        yield return new WaitForSeconds(4f);
        ShowUI(LoginUIType.StartPanel);
    }
}
