using System;
using _05._CSJ_Folder.Scripts.Quest;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopManager : Singleton<PopManager>
{
    [SerializeField] private PopupPanel _popupPanel;
    /*[SerializeField] private PlayerInfoUI _playerInfoUI;
    [SerializeField] private PopupPassword _popupPassword;
    [SerializeField] private AccountPanel _accountPanel;*/
    
    public string CurrentPassword { get; set; } = string.Empty;

    private void Start()
    {
        SceneManager.sceneUnloaded += (scene) =>
        {
            HidePopup();
        };
        
        TutorialTargets.Register("PopUpPanel", _popupPanel.transform as RectTransform);
    }
    
    /// <summary>
    /// 확인 취소 버튼 두개가 있는 팝업을 보여줍니다.
    /// </summary>
    /// <param name="message">팝업 가운데에 띄울 메세지 내용입니다.</param>
    /// <param name="leftText">왼쪽 버튼에 띄울 텍스트입니다. 미작성 시 OK 가 출력됩니다.</param>
    /// <param name="onLeftClick">왼쪽 버튼을 눌렀을 때, 실행할 액션입니다. 매개변수가 없는 함수를 호출하셔도 되고, 화살표 함수를 사용하셔도 됩니다.</param>
    /// <param name="rightText">오른쪽 버튼에 띄울 텍스트입니다. 미작성 시 Cancel 이 출력됩니다.</param>
    /// <param name="onRightClick">오른쪽 버튼을 눌렀을 때, 실행할 액션입니다. 매개변수가 없는 함수를 호출하셔도 되고, 화살표 함수를 사용하셔도 됩니다.</param>
    public void ShowOKCancelPopup(string message,
                          string leftText = "네", Action onLeftClick = null,
                          string rightText = "아니오", Action onRightClick = null)
    {
        if (_popupPanel == null) {
            return;
        }

        _popupPanel.SetShow(message, leftText, onLeftClick, rightText, onRightClick);
    }

    /// <summary>
    /// 확인버튼이 가운데 하나만 있는 팝업을 보여줍니다.
    /// </summary>
    /// <param name="message">팝업 가운데에 띄울 메세지 내용입니다.</param>
    /// <param name="leftText">가운데 버튼에 띄울 텍스트입니다. 미작성 시 OK 가 출력됩니다.</param>
    /// <param name="onLeftClick">가운데 버튼을 눌렀을 때, 실행할 액션입니다. 매개변수가 없는 함수를 호출하셔도 되고, 화살표 함수를 사용하셔도 됩니다.</param>
    public void ShowOKPopup(string message,
                      string leftText = "OK", Action onLeftClick = null)
    {
        if (_popupPanel == null) {
            return;
        }

        _popupPanel.SetShow(message, leftText, onLeftClick);
    }

    /// <summary>
    /// 팝업을 숨깁니다.
    /// </summary>
    public void HidePopup()
    {
        _popupPanel?.SetHide();
    }

    /*public async Task ShowPlayerInfo(string uid)
    {
        _playerInfoUI.gameObject.SetActive(true);
        var info = await Database_RecordManager.Instance.LoadRankData(uid);
        _playerInfoUI.SetInfoText(info);
    }*/

    /*/// <summary>
    /// 비밀번호 입력 팝업을 보여줍니다.
    /// </summary>
    /// <param name="onPasswordEntered">비밀번호 입력 확인 시 실행할 함수</param>
    public void ShowPasswordPopup(Action onPasswordEntered)
    {
        if (_popupPassword == null) {
            return;
        }
        _popupPassword.SetShow(onPasswordEntered);
    }

    /// <summary>
    /// 어카운트 패널을 보여줍니다.
    /// </summary>
    public void ShowAccountPanel()
    {
        if (_accountPanel == null) {
            return;
        }
        _accountPanel.SetShow();
    }*/
}
