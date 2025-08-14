using Firebase.Auth;
using Firebase.Extensions;
using Google;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GoogleLogin : MonoBehaviour
{
    // 구글 로그인 버튼
    [SerializeField] private Button _googleLoginButton;
    [SerializeField] private Button _googleLogoutButton;
    [SerializeField] private Button _googleLinkButton;

    public event Action LoginCompleted;

    private void Start()
    {
        _googleLoginButton.onClick.AddListener(OnGoogleSignInClicked);
        _googleLogoutButton.onClick.AddListener(OnGoogleSignOutClicked);
        _googleLinkButton.onClick.AddListener(OnLinkWithGoogleClicked);
    }

    /// <summary>
    /// 구글 로그인 버튼 클릭 시 호출되는 메서드
    /// GoogleSignIn 설정 초기화 및 인증 
    /// 인증 결과는 OnGoogleAuthenticatedFinished() 메서드에서 처리
    /// </summary>
    private void OnGoogleSignInClicked()
    {
        Debug.Log("구글 로그인 버튼 입력");
        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnGoogleAuthenticatedFinished);
    }

    /// <summary>
    /// 구글 로그아웃 버튼 클릭 시 호출되는 메서드
    /// Google 로그인 결과 SignOut 설정
    /// 인증 결과는 OnGoogleAuthenticatedFinished() 메서드에서 처리
    /// </summary>
    private void OnGoogleSignOutClicked()
    {
        Debug.Log("구글 로그아웃 버튼 입력");
        GoogleSignIn.DefaultInstance.SignOut();
        FirebaseManager.Auth.SignOut();
    }

    /// <summary>
    /// 구글 인증 완료 후 호출되는 콜백 메서드
    /// 인증 실패: 에러 코드
    /// 인증 성공: GoogleSignUp 메서드 호출
    /// </summary>
    /// <param name="task">구글 인증 결과 Task</param>
    private void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsCanceled)
        {
            Debug.LogError("구글 인증 취소");
        }

        if (task.IsFaulted)
        {
            Debug.LogError($"구글 인증 실패 : {task.Exception}");
        }

        else
        {
            GoogleSignUp(task);
        }
    }

    /// <summary>
    /// 구글 인증 완료 후 Credential 생성
    /// Firebase에 로그인/회원가입 시도하는 메서드
    /// </summary>
    /// <param name="userTask">구글 로그인 결과 Task</param>
    private void GoogleSignUp(Task<GoogleSignInUser> userTask)
    {
        Firebase.Auth.Credential credential =
        Firebase.Auth.GoogleAuthProvider.GetCredential(userTask.Result.IdToken, null);
        FirebaseManager.Auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(async task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("구글 로그인 취소");

                //PopupManager.Instance.ShowOKPopup("구글 로그인 취소", "OK", () => PopupManager.Instance.HidePopup());
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"구글 로그인 실패 : {task.Exception}");

                //PopupManager.Instance.ShowOKPopup("구글 로그인 실패", "OK", () => PopupManager.Instance.HidePopup());
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            // 구글 로그인 한 계정을 CurrentUser로 설정
            FirebaseUser user = FirebaseManager.Auth.CurrentUser;

            await DatabaseManager.Instance.SaveNicknameAsync();
            await user.ReloadAsync();


            // LoginPanel -> GameStartPanel 로 변경
            if (user != null)
            {
                Debug.Log("구글 로그인 성공. GameStart패널 활성화");
                LoginCompleted?.Invoke();
            }
        });
    }
    
    /// <summary>
    /// 익명 계정을 구글 계정으로 전환 및 연동
    /// </summary>
    public void OnLinkWithGoogleClicked()
    {
        // 계정 전환 가능 여부 체크
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
    
        if (user == null || !user.IsAnonymous)
        {
            //PopupManager.Instance.ShowOKPopup("게스트x. 계정 전환 불가", "OK", () => PopupManager.Instance.HidePopup());
            return;
        }
    
        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError($"구글 로그인 실패 / 원인: {task.Exception}");
    
                //PopupManager.Instance.ShowOKPopup("구글 로그인 실패", "OK", () => PopupManager.Instance.HidePopup());
                return;
            }
    
            GoogleSignInUser googleUser = task.Result;
            string idToken = googleUser.IdToken;
    
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
    
            FirebaseManager.Auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(async linkTask =>
            {
                if (linkTask.IsCanceled)
                {
                    //PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 취소", "OK", () => PopupManager.Instance.HidePopup());
                    return;
                }
    
                if (linkTask.IsFaulted)
                {
                    //PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 실패", "OK", () => PopupManager.Instance.HidePopup());
                    GoogleSignIn.DefaultInstance.SignOut();
                    GoogleSignIn.DefaultInstance.Disconnect();
                    return;
                }
    
                Firebase.Auth.AuthResult linkedUser = linkTask.Result;
    
                string googleDisplayName = googleUser.DisplayName;
                Debug.Log($"구글 계정 닉네임 : {googleDisplayName}");
    
                if (FirebaseManager.Auth.CurrentUser == null)
                {
                    Debug.Log("현재 유저 상태: CurrentUser is null");
                }
                else
                {
                    Debug.Log("현재 유저 상태: CurrentUser");
                }
    
                // 구글 닉네임 변경 
                await Utility.SetGoogleNickname(user, googleDisplayName);
                await user.ReloadAsync();
    
                //GameStartPanel 닉네임 text 변경 이벤트 호출
                //_gameStartPanel.OnSetNicknameField?.Invoke(user.DisplayName);
    
                Debug.Log("------유저 정보------");
                Debug.Log($"유저 이름 : {user.DisplayName}");
                Debug.Log($"유저 ID: {user.UserId}");
                Debug.Log($"이메일 : {user.Email}");
    
                /*PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 성공\r\n 다시 로그인 해주세요.", "OK", () =>
                {
                    //PopupManager.Instance.HidePopup();
    
                    // SignOut
                    //Utility.SetOffline();
    
                    // 구글 계정 로그아웃 처리 및 계정과 앱 연결 해제
                    bool isGoogleUser = user.ProviderData.Any(provider => provider.ProviderId == "google.com");
                    if (isGoogleUser)
                    {
                        GoogleSignIn.DefaultInstance.SignOut();
                        GoogleSignIn.DefaultInstance.Disconnect();
                    }
    
                    FirebaseManager.Auth.SignOut();
    
                    // 구글 계정 로그아웃 처리 및 계정과 앱 연결 해제
                    GoogleSignIn.DefaultInstance.SignOut();
                    GoogleSignIn.DefaultInstance.Disconnect();
    
                    // LoginPanel ShowUI, GameStartPanel HideUI
                    OnClickSignOut?.Invoke();
                });*/
            });
        });
    }
}
