using JHT;
using UnityEngine;
using UnityEngine.UI;

public class MonsterSpawnDemo : MonoBehaviour
{

    [SerializeField] private Button changeStage;
    [SerializeField] private Button changeRound;
    // 테스트
    [SerializeField] private Button takeDamageButton;
    [SerializeField] private Button connectButton;
    public JHT_NormalMonster monster;

    int stage = -1;
    int round = -1;

    void Start()
    {
        changeStage.onClick.AddListener(ChangeStage);
        changeRound.onClick.AddListener(ChangeRound);
        connectButton.onClick.AddListener(Connect);
        takeDamageButton.onClick.AddListener(ChangeHp);
    }
    
    private void Connect()
    {
        monster = GameObject.FindWithTag("Enemy").GetComponent<JHT_BaseMonsterFSM>() as JHT_NormalMonster;
    }

    public void ChangeHp()
    {
        // monster.TakeDamage(1.5f);
    }


    public void ChangeStage()
    {
        //JHT_MonsterSpawnManager.Instance.ChangeStage();
    }
    
    public void ChangeRound()
    {
        JHT_MonsterSpawnManager.Instance.roundIndex += 1;
        JHT_MonsterSpawnManager.Instance.ChangeRound(JHT_MonsterSpawnManager.Instance.roundIndex);
    }

}
