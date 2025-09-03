using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class goto0828Scene : MonoBehaviour
{
    [SerializeField] private Button Goto0828;
    private string returnSceneName = "0828Demo";

    private void Start()
    {
        Goto0828.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(returnSceneName);
        });
    }
}
