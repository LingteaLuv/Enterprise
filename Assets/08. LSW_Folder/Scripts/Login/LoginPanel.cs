using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : UIBase
{
    [SerializeField] private Button _googleLoginBtn;
    [SerializeField] private Button _guestLoginBtn;
    [SerializeField] private Button _playGamesLoginBtn;

    public Action OnLoginCompleted;

    private void Start()
    {
        _googleLoginBtn.onClick.AddListener(async () => await OnTouchGoogleLoginBtn());
        _guestLoginBtn.onClick.AddListener(async () => await OnTouchGuestLoginBtn());
        _playGamesLoginBtn.onClick.AddListener(async () => await OnTouchPlayGamesLoginBtn());
    }

    private async Task OnTouchGoogleLoginBtn()
    {
        if (await AuthManager.Instance.GoogleLogin())
        {
            Debug.Log("구글 로그인 성공, Panel 변환");
            OnLoginCompleted?.Invoke();
        }
    }
    
    private async Task OnTouchGuestLoginBtn()
    {
        Debug.Log("진입");
        if (await AuthManager.Instance.GuestLogin())
        {
            Debug.Log("게스트 로그인 성공, Panel 변환");
            OnLoginCompleted?.Invoke();
        }
    }
    
    private async Task OnTouchPlayGamesLoginBtn()
    {
        Debug.Log("진입");
        if (await AuthManager.Instance.PlayGamesLogin())
        {
            Debug.Log("게스트 로그인 성공, Panel 변환");
            OnLoginCompleted?.Invoke();
        }
    }
}
