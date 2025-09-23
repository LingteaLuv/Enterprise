using System;
using System.Collections;
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
    public bool isLogined = false;

    private void Start()
    {
        FirebaseManager.Instance.OnFirebaseReady += () => Init();
    }
    
    private void Init()
    {
        _auth = FirebaseManager.Auth;
        LoginCompleted += () => isLogined = true;
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
            LoginManager.Instance.SetLoginType(user.ProviderId);
            LoginCompleted?.Invoke();
            return true;
        }
        return false;
    }
    
    public async Task<bool> TestLogin()
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
        
        string testId = "hagwhr2@naver.com";
        string testPassword = "123456";
        AuthResult testUser = await _auth.SignInWithEmailAndPasswordAsync(testId, testPassword);
        FirebaseUser user = testUser.User;
        if (user != null)
        {
            DatabaseManager.Instance.Init();
            await DatabaseManager.Instance.SaveFieldAsync($"CreditData/Gold", 1000000000);
            await DatabaseManager.Instance.SaveFieldAsync($"CreditData/Gem", 1000000000);
            await DatabaseManager.Instance.SaveFieldAsync($"CreditData/EnhancementStone", 1000000000);
            await DatabaseManager.Instance.SaveFieldAsync($"CreditData/RelicsCoupon", 1000000000);
            await DatabaseManager.Instance.SaveFieldAsync($"CreditData/RelicsPoint", 1000000000);
            await DatabaseManager.Instance.SaveFieldAsync($"CreditData/CrewDrawTicket", 1000000000);
            await DatabaseManager.Instance.SaveFieldAsync($"CreditData/EquipDrawTicket", 1000000000);
            
            await DatabaseManager.Instance.SetNickname();
            LoginManager.Instance.SetLoginType("test");
            LoginCompleted?.Invoke();
        }
        _isClicked = false;
        return true;
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
            //await user.ReloadAsync();
            await DatabaseManager.Instance.SetNickname();
            LoginManager.Instance.SetLoginType("anonymous");
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
        try
        {
            GoogleSignInUser user = await GoogleSignIn.DefaultInstance.SignIn();
            if (user == null)
            {
                Debug.LogWarning("구글 로그인 취소(try, user가 null인 상황");
                return false;
            }
            return await GoogleSignUp(user);
        }
        catch (Google.GoogleSignIn.SignInException e)
        {
            Debug.LogWarning("구글 로그인 중 취소 또는 실패: " + e.Status);
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError("구글 로그인 중 예외 발생: " + e);
            return false;
        }
        finally
        {
            _isClicked = false; // 플래그 항상 해제
        }
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
            await DatabaseManager.Instance.SetNickname(userTask.DisplayName);
            //await user.ReloadAsync();
            LoginManager.Instance.SetLoginType(user.ProviderId);
            LoginCompleted?.Invoke();
            return true;
        }
        _isClicked = false;
        return false;
    }

    public async Task<bool> PlayGamesLogin()
    {
        if (_isClicked) return false;
        _isClicked = true;
        Debug.Log("플레이 게임즈 로그인 버튼 입력");

        PlayGamesPlatform.Activate();
        var tcs = new TaskCompletionSource<bool>();

        async void RequestServerSide()
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, async code =>
            {
                try
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
                }
                catch (Exception e)
                {
                    Debug.LogError("PlayGamesSignUp 실패: " + e);
                    tcs.SetResult(false);
                }
                finally
                {
                    _isClicked = false;
                }
            });
        }

        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                if (status == SignInStatus.Success)
                {
                    Debug.Log("플레이 게임즈 로그인 성공: " + PlayGamesPlatform.Instance.GetUserId());
                    RequestServerSide();
                }
                else
                {
                    Debug.LogError("구글 로그인 실패: " + status);
                    tcs.SetResult(false);
                    _isClicked = false;
                }
            });
        }
        else
        {
            // 이미 로그인 되어 있으면 서버 코드만 요청
            RequestServerSide();
        }

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
            //await user.ReloadAsync();
            LoginManager.Instance.SetLoginType(user.ProviderId);
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
    
                PopManager.Instance.ShowOKPopup("구글 로그인 실패", "OK", () => PopManager.Instance.HidePopup());
                return false;
            }
    
            GoogleSignInUser googleUser = task.Result;
            string idToken = googleUser.IdToken;
    
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
    
            FirebaseManager.Auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(async linkTask =>
            {
                if (linkTask.IsCanceled)
                {
                    PopManager.Instance.ShowOKPopup("구글 계정으로 전환 취소", "OK", () => PopManager.Instance.HidePopup());
                    return false;
                }
    
                if (linkTask.IsFaulted)
                {
                    PopManager.Instance.ShowOKPopup("구글 계정으로 전환 실패", "OK", () => PopManager.Instance.HidePopup());
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

    public IEnumerator LoginSuccessed()
    {
        yield return isLogined = true;
    }
}
