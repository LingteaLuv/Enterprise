using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInfoPanel : UIBase
{
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _uidText;
    [SerializeField] private Button _nicknameChangeBtn;
    [SerializeField] private Button _googleLinkBtn;
    [SerializeField] private Button _playGamesLinkBtn;
    //[SerializeField] private Button _logoutBtn;
    [SerializeField] private Button _exitBtn;
    
    public Action OnClickedExitBtn;
    public Action OnClickedNicknameChangeBtn;
    
    private void Start()
    {
        _googleLinkBtn.onClick.AddListener(OnTouchGoogleLinkBtn);
        _playGamesLinkBtn.onClick.AddListener(async () => await OnTouchPlayGamesLinkBtn() );
        
        //_logoutBtn.onClick.AddListener(OnTouchLogoutBtn);

        if (!FirebaseManager.Auth.CurrentUser.IsAnonymous)
        {
            _googleLinkBtn.gameObject.SetActive(false);
            _playGamesLinkBtn.gameObject.SetActive(false);
        }
        
        _nicknameChangeBtn.onClick.AddListener(async () =>
        {
            OnClickedNicknameChangeBtn.Invoke();
        });
        
        _exitBtn.onClick.AddListener(() =>
        {
            OnClickedExitBtn?.Invoke();
        });
        gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        _nicknameText.text = LoginManager.Instance.Nickname;
        _uidText.text = $"UID : {FirebaseManager.Auth.CurrentUser.UserId.Substring(0,6)}";
    }

    private void OnDisable()
    {
        _nicknameText.text = "";
        _uidText.text = "";
    }
    
    private async UniTask SetText()
    {
        if (!FirebaseManager.Auth.CurrentUser.IsAnonymous)
        {
            _googleLinkBtn.gameObject.SetActive(false);
            _playGamesLinkBtn.gameObject.SetActive(false);
        }
        
        await DatabaseManager.Instance.LoadNicknameAsync((nickname) =>
        {
            _nicknameText.text = nickname;
        });
    }
    
    private void OnTouchGoogleLinkBtn()
    {
        AuthManager.Instance.LinkWithGoogleAsync(async () =>
        {
            await SetText();
        });
    }
    
    private async UniTask OnTouchPlayGamesLinkBtn()
    {
        await AuthManager.Instance.LinkWithPlayGamesAsync(async () =>
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
