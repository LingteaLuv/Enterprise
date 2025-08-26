using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class AuthManager : Singleton<AuthManager>
{
    private FirebaseAuth _auth;
    private bool _isClicked;
    public event Action LoginCompleted;

    private void Start()
    {
        FirebaseManager.Instance.OnFirebaseReady += () => Init();
    }
    
    private void Init()
    {
        _auth = FirebaseManager.Auth;
    }


    #region Login
    
    /// <summary>
    /// 앱 실행시 호출되는 자동 로그인 메서드
    /// 이전 로그인 기록이 없거나, 게스트 로그인인 경우 로그아웃 및 계정 삭제
    /// 이전 로그인이 구글 로그인인 경우 자동 로그인 플로우 실행
    /// </summary>
    /// <returns></returns>
    public async Task<bool> AutoLogin()
    {
        FirebaseUser user = _auth.CurrentUser;
        if (_auth != null && user != null)
        {
            try
            {
                await user.ReloadAsync(); // 서버 동기화
            }
            catch (Exception e)
            {
                Debug.LogWarning($"AutoLogin 실패: {e.Message}");
                _auth.SignOut();
                return false;
            }
            
            if (user.IsAnonymous)
            {
                await DatabaseManager.Instance.DeleteDataAsync();
                await user.DeleteAsync();
                _auth.SignOut();
                return false;
            }
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 게스트 로그인 메서드, 익명 계정 생성 및 임시 닉네임 부여, DB저장
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GuestLogin()
    {
        if (_isClicked) return false;
        _isClicked = true;
        // 게스트 로그인 가능 여부 체크
        if(FirebaseManager.Auth.CurrentUser != null)
        {
            //PopupManager.Instance.ShowOKPopup("게스트 로그인 불가", "OK", () => PopupManager.Instance.HidePopup());
            Debug.LogError($"유저 UID : {FirebaseManager.Auth.CurrentUser.UserId}  " +
                           $"/ 유저 닉네임 : {FirebaseManager.Auth.CurrentUser.DisplayName}");
            _isClicked = false;
            return false;
        }

        await FirebaseManager.Auth.SignInAnonymouslyAsync();
            
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
        
        if (user != null)
        {
            await user.ReloadAsync();
            await DatabaseManager.Instance.SetNickname();
            LoginCompleted?.Invoke();
        }
        _isClicked = false;
        return true;
    }
    
    /// <summary>
    /// 구글 로그인 메서드, 구글 계정 생성 및 구글 닉네임 연동
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GoogleLogin()
    {
        if (_isClicked) return false;
        _isClicked = true;
        Debug.Log("구글 로그인 버튼 입력");
        GoogleSignInUser user = await GoogleSignIn.DefaultInstance.SignIn();
        if (user != null)
        {
            return await GoogleSignUp(user);
        }

        _isClicked = false;
        return false;
    }
    
    private async Task<bool> GoogleSignUp(GoogleSignInUser userTask)
    {
        Firebase.Auth.Credential credential =
            Firebase.Auth.GoogleAuthProvider.GetCredential(userTask.IdToken, null);
        await FirebaseManager.Auth.SignInAndRetrieveDataWithCredentialAsync(credential);
        
        // 구글 로그인 한 계정을 CurrentUser로 설정
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;

        if (user != null)
        {
            await DatabaseManager.Instance.SetNickname(user.DisplayName);
            await user.ReloadAsync();
            LoginCompleted?.Invoke();
        }
        _isClicked = false;
        return true;
    }

    public async Task<bool> PlayGamesLogin()
    {
        if (_isClicked) return false;
        _isClicked = true;
        Debug.Log("플레이 게임즈 로그인 버튼 입력");
        
        PlayGamesPlatform.Activate();
        
        var tcs = new TaskCompletionSource<bool>();
        
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("구글 로그인 성공: " + PlayGamesPlatform.Instance.GetUserId());
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, async code =>
                {
                    if (string.IsNullOrEmpty(code))
                    {
                        tcs.SetResult(false);
                    }
                    else
                    {
                        await PlayGamesSignUp(code);
                        tcs.SetResult(true);
                    }
                    _isClicked = false; 
                });
            }
            else
            {
                Debug.LogError("구글 로그인 실패: " + status);
                tcs.SetResult(false);
                _isClicked = false;
            }
        });
        return await tcs.Task;
    }
    
    private async Task PlayGamesSignUp(string code)
    {
        Firebase.Auth.Credential credential =
            Firebase.Auth.PlayGamesAuthProvider.GetCredential(code);
        await FirebaseManager.Auth.SignInAndRetrieveDataWithCredentialAsync(credential);
        
        // 구글 로그인 한 계정을 CurrentUser로 설정
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;

        if (user != null)
        {
            await DatabaseManager.Instance.SetNickname(user.DisplayName);
            await user.ReloadAsync();
            LoginCompleted?.Invoke();
        }
    }
    
    /// <summary>
    /// 게스트 로그인에서 구글 계정으로 연동하는 메서드
    /// 구글 게정으로 전환하고, 임시 닉네임을 구글 닉네임으로 변경
    /// </summary>
    /// <returns></returns>
    public bool LinkWithGoogleAsync(Action callback)
    {
        // 계정 전환 가능 여부 체크
        FirebaseUser user = FirebaseManager.Auth.CurrentUser;
    
        if (user == null || !user.IsAnonymous)
        {
            //PopupManager.Instance.ShowOKPopup("게스트x. 계정 전환 불가", "OK", () => PopupManager.Instance.HidePopup());
            return false;
        }
    
        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError($"구글 로그인 실패 / 원인: {task.Exception}");
    
                //PopupManager.Instance.ShowOKPopup("구글 로그인 실패", "OK", () => PopupManager.Instance.HidePopup());
                return false;
            }
    
            GoogleSignInUser googleUser = task.Result;
            string idToken = googleUser.IdToken;
    
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
    
            FirebaseManager.Auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(async linkTask =>
            {
                if (linkTask.IsCanceled)
                {
                    //PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 취소", "OK", () => PopupManager.Instance.HidePopup());
                    return false;
                }
    
                if (linkTask.IsFaulted)
                {
                    //PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 실패", "OK", () => PopupManager.Instance.HidePopup());
                    //GoogleSignIn.DefaultInstance.SignOut();
                    //GoogleSignIn.DefaultInstance.Disconnect();
                    return false;
                }
                string googleDisplayName = googleUser.DisplayName;
                
                // 구글 닉네임 변경 
                await DatabaseManager.Instance.SetNickname(googleDisplayName);
                await user.ReloadAsync();
                callback.Invoke();
                return true;
            });
            return false;
        });
        return false;
    }
    
    #endregion

    
    #region Logout&DeleteAccount
    
    public void Logout()
    {
        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result != null)
            {
                GoogleSignIn.DefaultInstance.SignOut();
                GoogleSignIn.DefaultInstance.Disconnect();
            }
        });
        
        _auth.SignOut();
    }
    
    public async Task DeleteAccount()
    {
        FirebaseUser user = _auth.CurrentUser;
        if (user == null) return;

        await DatabaseManager.Instance.DeleteDataAsync();
        await user.DeleteAsync();

        Logout();
    }

    #endregion
    
    
    #region Legacy

    public void LinkWithGoogle()
    {
        // 계정 전환 가능 여부 체크
        FirebaseUser user = _auth.CurrentUser;
    
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
    
            _auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(async linkTask =>
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
                await DatabaseManager.Instance.SetNickname(googleDisplayName);
                await user.ReloadAsync();
            });
        });
    }

    #endregion
}
