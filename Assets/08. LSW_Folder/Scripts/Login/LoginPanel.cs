using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : UIBase
{
    [SerializeField] private Button _googleLoginBtn;
    [SerializeField] private Button _guestLoginBtn;
    [SerializeField] private Button _playGamesLoginBtn;
    //[SerializeField] private Button _testLoginBtn;

    public Action OnLoginCompleted;
    public Action OnLoginTried;
    public Action OnLoginFailed;

    private void Start()
    {
        _googleLoginBtn.onClick.AddListener(async () => await OnTouchGoogleLoginBtn());
        _guestLoginBtn.onClick.AddListener(async () => await OnTouchGuestLoginBtn());
        _playGamesLoginBtn.onClick.AddListener(async () => await OnTouchPlayGamesLoginBtn());
        //_testLoginBtn.onClick.AddListener(async () => await OnTouchTestLoginBtn());
    }

    private async Task OnTouchGoogleLoginBtn()
    {
        OnLoginTried?.Invoke();
        if (await AuthManager.Instance.GoogleLogin())
        {
            Debug.Log("구글 로그인 성공");
            OnLoginCompleted?.Invoke();
            return;
        }
        Debug.Log("구글 로그인 실패or취소");
        OnLoginFailed?.Invoke();
    }
    
    private async Task OnTouchGuestLoginBtn()
    {
        OnLoginTried?.Invoke();
        if (await AuthManager.Instance.GuestLogin())
        {
            Debug.Log("게스트 로그인 성공");
            OnLoginCompleted?.Invoke();
            return;
        }
        OnLoginFailed?.Invoke();
    }
    
    private async Task OnTouchPlayGamesLoginBtn()
    {
        OnLoginTried?.Invoke();
        if (await AuthManager.Instance.PlayGamesLogin())
        {
            Debug.Log("플레이 게임즈 로그인 성공");
            OnLoginCompleted?.Invoke();
            return;
        }
        OnLoginFailed?.Invoke();
    }
    
    private async Task OnTouchTestLoginBtn()
    {
        OnLoginTried?.Invoke();
        if (await AuthManager.Instance.TestLogin())
        {
            Debug.Log("게스트 로그인 성공");
            OnLoginCompleted?.Invoke();
            return;
        }
        OnLoginFailed?.Invoke();
    }
}
