using UnityEngine;
using UnityEngine.UI;

public class TestLevelManager : MonoBehaviour
{
    [SerializeField] private Button _upBtn;

    private int _level = 1;

    private void Awake()
    {
        Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
    }
    
    private void Start()
    {
        _upBtn.onClick.AddListener(OnClickUpBtn);
        
        AnalyticsManager.Instance.LogLevelComplete(_level);
    }
    
    private void OnClickUpBtn()
    {
        AnalyticsManager.Instance.LogLevelComplete(_level);
        _level++;
        AnalyticsManager.Instance.LogLevelStart(_level);
    }
}
