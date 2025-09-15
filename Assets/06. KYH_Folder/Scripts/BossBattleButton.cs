using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BossBattleButton : MonoBehaviour
{
    [SerializeField] private Button bossButton;

    private void Start()
    {
        bossButton.onClick.AddListener(EnterBossBattle);
        bossButton.onClick.AddListener(QuestSignalManager.Instance.OnBossBattleEnter);
    }

    private void EnterBossBattle()
    {
        Debug.Log(" 수동으로 보스전투 진입!");

        GlobalStageManager.Instance.bossBattleTriggered = true;
        // 보스 진입 여부는 자동 진입에서만 쓰고, 수동 진입은 무조건 진입 가능
        SceneManager.LoadScene("BossBattleScene");
    }
}
