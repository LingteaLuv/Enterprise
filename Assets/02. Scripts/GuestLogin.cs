using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GuestLogin : MonoBehaviour
{
    [SerializeField] private Button _guestLoginButton;

    private bool _isClicked;
    public event Action LoginCompleted;

    private void Start()
    {
        _guestLoginButton.onClick.AddListener(() =>
        {
            if (!_isClicked)
            {
                OnClick_GuestLogin();
            }
        });
    }

    private void OnClick_GuestLogin()
    {
        _isClicked = true;
        // 게스트 로그인 가능 여부 체크
        if(FirebaseManager.Auth.CurrentUser != null)
        {
            //PopupManager.Instance.ShowOKPopup("게스트 로그인 불가", "OK", () => PopupManager.Instance.HidePopup());
            Debug.LogError($"유저 UID : {FirebaseManager.Auth.CurrentUser.UserId}  " +
                $"/ 유저 닉네임 : {FirebaseManager.Auth.CurrentUser.DisplayName}");
            _isClicked = false;
            return;
        }

        FirebaseManager.Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(async task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("게스트 로그인 취소");
                _isClicked = false;
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"게스트 로그인 실패 / 원인: {task.Exception}");
                _isClicked = false;
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            FirebaseUser user = FirebaseManager.Auth.CurrentUser;
            
            if (user != null)
            {
                Debug.Log("게스트 생성 완료");
                
                await user.ReloadAsync();
                // 게스트 닉네임 변경 
                await Utility.SetGuestNickname(user);
                // Firebase DB에 닉네임 저장
                await DatabaseManager.Instance.SaveNicknameAsync();

                Debug.Log("------유저 정보(GuestLogin)------");
                Debug.Log($"유저 닉네임 : {user.DisplayName}");
                Debug.Log($"유저 ID : {user.UserId}");
                Debug.Log($"이메일 : {user.Email}");
                
                // LoginPanel -> GameStartPanel 로 변경
                LoginCompleted?.Invoke();
            }
            _isClicked = false;
        });
    }
}
