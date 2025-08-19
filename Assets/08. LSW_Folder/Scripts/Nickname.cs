using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Nickname : MonoBehaviour
{
    [SerializeField] private Button _nicknameButton;

    private void Start()
    {
        _nicknameButton.onClick.AddListener(async () =>
        {
            await OnTouchBtn();
        });
    }

    private async Task OnTouchBtn()
    {
        await DatabaseManager.Instance.LoadNicknameAsync((nickname) =>
        {
            Debug.Log(nickname);
        });
    }
}
