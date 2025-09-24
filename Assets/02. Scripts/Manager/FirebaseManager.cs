using System;
using System.Collections;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Google;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseManager : Singleton<FirebaseManager>
{
    private static FirebaseApp _app;
    public static FirebaseApp App { get { return _app; } }
    
    private static FirebaseAuth _auth;
    public static FirebaseAuth Auth { get { return _auth; } }
    
    private static FirebaseUser _user;
    public static FirebaseUser User { get { return _user; } }
    
    private static FirebaseDatabase _database;
    public static FirebaseDatabase Database { get { return _database; } }
    
    private static DatabaseReference _dataReference;
    public static DatabaseReference DataReference { get { return _dataReference; } }

    public Action OnFirebaseReady;
    
    // 구글
    [SerializeField] private string _googleWebAPI = "442492611135-dq4pgnvgurl1urko0ufe2hqbeql3psnu.apps.googleusercontent.com";

    private GoogleSignInConfiguration _configuration;
    public GoogleSignInConfiguration Configuration { get { return _configuration; } }
    
    protected override void Awake()
    {
        OnFirebaseReady = null;
        
        // GoogleSignIn에 사용할 인증 설정 초기화
        _configuration = new GoogleSignInConfiguration
        {
            WebClientId = _googleWebAPI,
            RequestIdToken = true,
            RequestEmail = true
        };

        // 초기화한 설정을 GoogleSignIn.Configuration에 적용
        GoogleSignIn.Configuration = _configuration;
        
        /*StartCoroutine(InitFirebaseCoroutine());
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1f / 60f;*/
        
    }

    private void Start()
    {
        StartCoroutine(InitFirebaseCoroutine());
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1f / 60f;
    }

    
    
    private IEnumerator InitFirebaseCoroutine()
    {
        Task<DependencyStatus> task = Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        DependencyStatus dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            _app = FirebaseApp.DefaultInstance;
            _auth = FirebaseAuth.DefaultInstance;
            _database = FirebaseDatabase.DefaultInstance;
            _dataReference = FirebaseDatabase.DefaultInstance.RootReference;
        }
        else
        {
            _app = null;
            _auth = null;
            _database = null;
            _dataReference = null;
        }
        OnFirebaseReady?.Invoke();
    }

    public async Task<bool> IsLoggedIn()
    {
        if (_auth != null && _auth.CurrentUser != null)
        {
            if (_auth.CurrentUser.IsAnonymous)
            {
                // Todo : DB에 저장된 내역 삭제
                await _auth.CurrentUser.DeleteAsync();
                _auth.SignOut();
                return false;
            }
            return true;
        }
        return false;
    }
}
