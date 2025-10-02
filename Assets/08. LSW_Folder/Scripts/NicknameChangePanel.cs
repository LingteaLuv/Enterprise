using Firebase.Auth;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameChangePanel : UIBase
{
    [SerializeField] private Button _closePopupButton;
    [SerializeField] private Button _nicknameChangeButton;
    [SerializeField] private TMP_InputField _nicknameField;
    
    private string _currentNickname;

    public Action OnClickClosePopup;
    public Action OnClickNicknameChange;

    private void Start()
    {
        //_nicknameField.characterLimit = 7;

        _closePopupButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());
        _nicknameChangeButton.onClick.AddListener(ChanegeNickname);
    }

    private void OnEnable()
    {
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;

        _currentNickname = LoginManager.Instance.Nickname;
        
        _nicknameField.placeholder.GetComponent<TMP_Text>().text = _currentNickname;
    }

    /// <summary>
    /// 안내 메세지 팝업을 띄우고 닫는 메서드
    /// </summary>
    /// <param name="message">팝업에 표시할 안내 메세지</param>
    private void ShowPopup(string message)
    {
        PopManager.Instance.ShowOKPopup(message, "OK", () => PopManager.Instance.HidePopup());
    }

    private async void ChanegeNickname()
    {
        if (string.IsNullOrEmpty(_nicknameField.text.Trim()))
        {
            ShowPopup("닉네임을 입력해주세요.");
            return;
        }
        Debug.LogError($"{_nicknameField.text.Length} vs {_nicknameField.characterLimit}");
        // 닉네임 글자 수 체크
        if (_nicknameField.text.Length > 8)
        {
            ShowPopup("닉네임은 8글자 이내로 입력해 주세요.");
            return;
        }

        // 기존 닉네임 일치 여부 체크
        if (_currentNickname == _nicknameField.text)
        {
            ShowPopup("기존에 사용 중인 닉네임과 동일합니다.\r\n다른 닉네임을 입력해 주세요.");
            return;
        }

        //NicknameCheck();
        
        // 닉네임 재설정 및 데이터베이스에 저장
        await DatabaseManager.Instance.SetNickname(_nicknameField.text);
        LoginManager.Instance.SetNickname();
        
        ShowPopup("닉네임 변경 성공");

        PopManager.Instance.ShowOKPopup("닉네임 변경 성공", "OK", () =>
        {
            PopManager.Instance.HidePopup();
            
            OnClickNicknameChange?.Invoke();
        });
    }
}
