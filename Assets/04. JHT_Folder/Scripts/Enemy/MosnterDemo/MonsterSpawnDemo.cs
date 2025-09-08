using JHT;
using UnityEngine;
using UnityEngine.UI;

public class MonsterSpawnDemo : MonoBehaviour
{

    [SerializeField] private Button changeStage;
    [SerializeField] private Button changeRound;

    int stage = -1;
    int round = -1;

    void Start()
    {
        changeStage.onClick.AddListener(ChangeStage);
        changeRound.onClick.AddListener(ChangeRound);
    }
    
    
    public void ChangeStage()
    {
        JHT_MonsterSpawnManager.Instance.ChangeStage();
    }
    
    public void ChangeRound()
    {
        JHT_MonsterSpawnManager.Instance.roundIndex += 1;
        JHT_MonsterSpawnManager.Instance.SpawnMonster(JHT_MonsterSpawnManager.Instance.roundIndex);
    }

}
