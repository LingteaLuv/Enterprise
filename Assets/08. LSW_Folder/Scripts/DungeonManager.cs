using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private Button _successBtn;
    [SerializeField] private Button _failBtn;
    
    private readonly string _gameScene = "Game";
        
    private void Start()
    {
        _successBtn.onClick.AddListener(() =>
        {
            Debug.Log("전투 승리!");
            SceneManager.LoadScene(_gameScene);
        });
        
        _failBtn.onClick.AddListener(() =>
        {
            Debug.Log("전투 패배!");
            SceneManager.LoadScene(_gameScene);
        });
    }
}
