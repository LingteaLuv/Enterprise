using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInfoPanel : UIBase
{
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _loginType;
    [SerializeField] private Button _linkBtn;
    [SerializeField] private Button _logoutBtn;
    [SerializeField] private Button _exitBtn;
    
    public Action OnTouchedExitBtn;
    
    private void Start()
    {
        _linkBtn.onClick.AddListener(OnTouchLinkBtn);
        
        _logoutBtn.onClick.AddListener(OnTouchLogoutBtn);

        if (!FirebaseManager.Auth.CurrentUser.IsAnonymous)
        {
            _linkBtn.gameObject.SetActive(false);
        }
        
        _exitBtn.onClick.AddListener(() =>
        {
            OnTouchedExitBtn?.Invoke();
            gameObject.SetActive(false);
        });
        gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        _nicknameText.text = LoginManager.Instance.Nickname;
        _loginType.text = LoginManager.Instance.LoginType.ToString();
    }

    private void OnDisable()
    {
        _nicknameText.text = "";
        _loginType.text = "";
    }
    
    private async UniTask SetText()
    {
        if (!FirebaseManager.Auth.CurrentUser.IsAnonymous)
        {
            _linkBtn.gameObject.SetActive(false);
        }
        
        await DatabaseManager.Instance.LoadNicknameAsync((nickname) =>
        {
            _nicknameText.text = nickname;
        });
    }
    
    private void OnTouchLinkBtn()
    {
        AuthManager.Instance.LinkWithGoogleAsync(async () =>
        {
            await SetText();
        });
    }
    
    private void OnTouchLogoutBtn()
    {
        //AuthManager.Instance.Logout();
        Utility.OnDestroyAll.Invoke();
        SceneTransitionManager.Instance.LoadSceneWithLoading("LoginScene", 2f);
    }
}
