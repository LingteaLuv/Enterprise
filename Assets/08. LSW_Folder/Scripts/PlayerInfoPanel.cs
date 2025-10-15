using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;

public class PlayerInfoPanel : UIBase
{
    [SerializeField] private TextMeshProUGUI _nicknameText;
    [SerializeField] private TextMeshProUGUI _uidText;
    [SerializeField] private Button _nicknameChangeBtn;
    [SerializeField] private Button _googleLinkBtn;
    [SerializeField] private Button _playGamesLinkBtn;
    //[SerializeField] private Button _logoutBtn;
    [SerializeField] private Button _exitBtn;
    [SerializeField] private Button _imageChangeBtn;
    
    [SerializeField] private Transform _parent;
    [SerializeField] private GameObject _imagePrefab;
    [SerializeField] private Image _crewImage;

    private Sprite _curImage;
    private int _curImageId;
    private Vector2 _curPos;
    private Dictionary<int, GameObject> _images = new Dictionary<int, GameObject>();
    
    public Action OnClickedExitBtn;
    public Action OnClickedNicknameChangeBtn;
    public Action<Sprite> OnImageChanged;
    
    private async void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();
        _curPos = rt.anchoredPosition;
        rt.anchoredPosition = new Vector2(3000, -3000);
        _googleLinkBtn.onClick.AddListener(OnTouchGoogleLinkBtn);
        _playGamesLinkBtn.onClick.AddListener(async () => await OnTouchPlayGamesLinkBtn() );
        
        //_logoutBtn.onClick.AddListener(OnTouchLogoutBtn);

        if (!FirebaseManager.Auth.CurrentUser.IsAnonymous)
        {
            _googleLinkBtn.gameObject.SetActive(false);
            _playGamesLinkBtn.gameObject.SetActive(false);
        }
        
        PlayerDataManager.Instance.OnOwnedCharacterAdded += (key) => AddImage(key);
        
        _nicknameChangeBtn.onClick.AddListener(async () =>
        {
            OnClickedNicknameChangeBtn.Invoke();
        });
        
        _exitBtn.onClick.AddListener(() =>
        {
            OnClickedExitBtn?.Invoke();
        });
        
        _imageChangeBtn.onClick.AddListener(() =>
        {
            _curImage = _crewImage.sprite;
            string path = $"UserData/image";
            DatabaseManager.Instance.SaveField(path, _curImageId);
            OnImageChanged?.Invoke(_curImage);
        });
        await Init();
        OnImageChanged?.Invoke(_curImage);
        rt.anchoredPosition = _curPos;
        gameObject.SetActive(false);
    }
    
    private async UniTask Init()
    {
        string path = $"{FirebaseManager.Auth.CurrentUser.UserId}/UserData/image";
        await DatabaseManager.Instance.LoadFieldAsync<int>(path, (value) =>
        {
            _curImageId = value;
            InitImage();
        }, true, value : 20001);
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
        _crewImage.sprite = _curImage;
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

    private void InitImage()
    {
        foreach (var kvp in PlayerDataManager.Instance.AllCharacters)
        {
            if (kvp.Value.characterdata.characterID == _curImageId)
            {
                _curImage = kvp.Value.characterdata.characterSprite;
            }
            
            GameObject go = Instantiate(_imagePrefab, _parent);
            go.GetComponent<CrewIcon>().IconImage = kvp.Value.characterdata.characterSprite;
            go.GetComponent<CrewIcon>().GetComponent<Button>().onClick.AddListener(() =>
            {
                _crewImage.sprite = go.GetComponent<CrewIcon>().IconImage;
                _curImageId = kvp.Value.characterdata.characterID;
            });
            go.transform.GetChild(0).GetComponent<Image>().sprite = kvp.Value.characterdata.characterSprite;
            
            if (!PlayerDataManager.Instance.OwnedCharacters.ContainsKey(kvp.Key))
            {
                go.GetComponent<CrewIcon>().GetComponent<Button>().enabled = false;
                go.transform.GetChild(0).GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 255);
            }

            _images[kvp.Value.characterdata.characterID] = go;
        }
    }
    
    private void AddImage(int key)
    {
        _images[key].gameObject.GetComponent<CrewIcon>().GetComponent<Button>().enabled = true;
        _images[key].gameObject.transform.GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255, 255);
    }

    public Sprite GetCurImage()
    {
        return _curImage;
    }
}
