using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InfoPanel : UIBase
{
    [SerializeField] private Button _startBtn;
    [SerializeField] private Button _linkBtn;
    [SerializeField] private Button _deleteBtn;
    [SerializeField] private Button _logoutBtn;

    [SerializeField] private TextMeshProUGUI _welcomeText;

    public Action OnGameExit;
    public Action OnGameStart;
    
    private void Start()
    {
        _startBtn.onClick.AddListener(OnTouchStartBtn);
        _linkBtn.onClick.AddListener(OnTouchLinkBtn);
        _deleteBtn.onClick.AddListener(async () => await OnTouchDeleteBtn());
        _logoutBtn.onClick.AddListener(OnTouchLogoutBtn);

        if (!FirebaseManager.Auth.CurrentUser.IsAnonymous)
        {
            _linkBtn.gameObject.SetActive(false);
        }
    }
    
    private async void OnEnable()
    {
        await SetText();
    }

    private void OnDisable()
    {
        _welcomeText.text = "";
    }

    private async Task SetText()
    {
        await DatabaseManager.Instance.LoadNicknameAsync((nickname) =>
        {
            _welcomeText.text = $"어서오세요\n{nickname} 님";
        });
    }

    private void OnTouchStartBtn()
    {
        OnGameStart?.Invoke();
    }
    
    private void OnTouchLinkBtn()
    {
        AuthManager.Instance.LinkWithGoogleAsync(async () =>
        {
            await SetText();
        });
    }
    
    private async Task OnTouchDeleteBtn()
    {
        await AuthManager.Instance.DeleteAccount();
        OnGameExit?.Invoke();
    }
    
    private void OnTouchLogoutBtn()
    {
        AuthManager.Instance.Logout();
        OnGameExit?.Invoke();
    }
}
