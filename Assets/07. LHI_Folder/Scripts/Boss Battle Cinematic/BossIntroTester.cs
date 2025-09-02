using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public BossBattleIntroManager manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Test Boss Intro")]
    public void TestBossIntro()
    {
        manager.OnBossChallenge(); // 보스 도전 시퀀스 시작
    }

    [ContextMenu("Test Skip Intro")]
    public void TestSkipIntro() 
    {
        manager.SkipIntro(); // 보스 도전 시퀀스 스킵
    }
}
