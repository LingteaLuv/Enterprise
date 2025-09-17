using System;
using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using JHT;
using UnityEngine;

public class QuestSignalManager : Singleton<QuestSignalManager>
{
    [SerializeField] private QuestSignalSO _signal;

    private Action FomationChanged;
    private Action AutoFomation;
    private Action<RelicsObject> RelicTutorial;
    
    public event Action BossBattleEnter;
    public event Action OnDungeonEnter;
    
    private void OnEnable()
    {
        QuestManager.Instance.OnTutorialQuestCompleted -= UnSubscribeTutorial;
        QuestManager.Instance.OnTutorialQuestCompleted += UnSubscribeTutorial;
        
        FomationChanged = () => Tutorial("ArrangeTutorial");
        FormationManager.Instance.OnTempFormationChanged -= FomationChanged;
        FormationManager.Instance.OnTempFormationChanged += FomationChanged;
        
        AutoFomation = () => Tutorial("AutoArrangeTutorial");
        FormationManager.Instance.OnAutoFormation -= AutoFomation;
        FormationManager.Instance.OnAutoFormation += AutoFomation;
        
        RelicTutorial = (_) => Tutorial("RelicTutorial");
        InventoryManager.Instance.OnChangePanel -= RelicTutorial;
        InventoryManager.Instance.OnChangePanel += RelicTutorial;

        BossBattleEnter -= OnBossTutorial;
        BossBattleEnter += OnBossTutorial;

        OnDungeonEnter -= OnTutorial;
        OnDungeonEnter -= OnActiveDungeon;
        OnDungeonEnter += OnTutorial;
        OnDungeonEnter += OnActiveDungeon;
    }

    public void UnSubscribeTutorial(TutorialQuestDefinitionSO def)
    {
        var tuto = def.Goal.enumKey.ToKeyString();
        switch (tuto)
        {
            case "ArrangeTutorial":
                FormationManager.Instance.OnTempFormationChanged -= FomationChanged;
                break;
            case "AutoArrangeTutorial":
                FormationManager.Instance.OnAutoFormation -= AutoFomation;
                break;
            case "RelicTutorial":
                InventoryManager.Instance.OnChangePanel -= RelicTutorial;
                break;
            case "BossTutorial":
                BossBattleEnter -= OnBossTutorial;
                break;
            case "RogulikeTutorial":
                OnDungeonEnter -= OnTutorial;
                break;
            // TODO : 추후 스킬을 이으며 추가
            case "SkillTutorial":
                break;
        }
    }

    public void OnBossBattleEnter()
    {
        Debug.Log("Boss Battle Enter");
        ETCAchieve("BossBattle");
        BossBattleEnter?.Invoke();
    }

    public void OnBossTutorial()
    {
        Debug.Log("Boss Tutorial");
        Tutorial("BossTutorial");
    }

    public void OnDungeonEntered()
    {
        OnDungeonEnter?.Invoke();
    }

    public void OnTutorial()
    {
        Tutorial("RoguelikeTutorial");
    }

    public void OnActiveDungeon()
    {
        Active(ActiveType.Dungeon);
    }
    
    /// <summary>
    /// 적을 죽였을 때 이를 퀘스트로 갱신할 때 호출하는 시그널
    /// </summary>
    /// <param name="enemyId">적의 종류를 보냄 ex) general, boss, all</param>
    /// <param name="count"> 몇 마리의 적을 잡았는 지 보냄</param>
    /// <param name="general">기본적으로 true, 일반 퀘스트를 갱신할 때 사용</param>
    /// <param name="daily">일일 퀘스트 갱신용</param>
    /// <param name="weekly">주간 퀘스트 갱신용</param>
    public void KillEnemy(MonsterId enemyId, int count = 1, bool general = true, 
        bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.Kill(enemyId);
        SendSignal(key, count, general, daily, weekly);
        if (enemyId is MonsterId.All) return;
        SendSignal(QuestKeys.Kill(MonsterId.All), count, general,daily, weekly);
    }

    /// <summary>
    /// 아이템 획득시 보내는 시그널
    /// </summary>
    /// <param name="itemId">아이템 이름을 값으로 받음 ex)diamond, gold</param>
    /// <param name="count">획득한 수량, 아이템의 경우 1회만 획득하는 경우가 거의 없을거라 생각하여 실수를 막기 위해 초기값 없음 </param>
    /// <param name="general"></param>
    /// <param name="daily"></param>
    /// <param name="weekly"></param>
    public void CollectItem(MoneyId itemId, int count, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.Collect(itemId);
        SendSignal(key, count, general, daily, weekly);
    }

    /// <summary>
    /// 가차 진행시 보내는 시그널
    /// </summary>
    /// <param name="gachaId">어떤 가차를 진행했는지 키 값으로 보냄 ex) equipment, character</param>
    /// <param name="count">가차를 몇 번 진행했는지 키 값으로 보냄</param>
    /// <param name="general"></param>
    /// <param name="daily"></param>
    /// <param name="weekly"></param>
    public void GachaPull(ItemType gachaId, int count = 1, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.GachaPull(gachaId);
        SendSignal(key, count, general, daily, weekly);
        if (gachaId is ItemType.All or ItemType.RareRelic) return;
        SendSignal(QuestKeys.GachaPull(ItemType.All), count, general, daily, weekly);
    }
    
    /// <summary>
    /// 레벨업시 보내는 시그널
    /// </summary>
    /// <param name="LevelUpId"></param>
    /// <param name="count"></param>
    /// <param name="general"></param>
    /// <param name="daily"></param>
    /// <param name="weekly"></param>
    public void LevelUp(ItemType LevelUpId, int count = 1, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.LevelUp(LevelUpId);
        SendSignal(key, count, general, daily, weekly);
    }
    ///<summary>
    /// 레벨업시 보내는 시그널
    /// </summary>
    /// <param name="UpgradeId">어떤 함선 강화를 진행하였는지를 타입으로 보냄 </param>
    /// <param name="count"></param>
    /// <param name="general"></param>
    /// <param name="daily"></param>
    /// <param name="weekly"></param>
    public void Upgrade(UpgradeType UpgradeId, int count = 1, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.Upgrade(UpgradeId);
        SendSignal(key, count, general, daily, weekly);
    }

    public void Active(ActiveType ActiveId, int count = 1, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.Active(ActiveId);
        SendSignal(key, count, general, daily, weekly);
    }

    /// <summary>
    /// 기타 사항 달성시 보내는 메일
    /// </summary>
    /// <param name="achieveType">string으로 메시지를 보냄</param>
    /// <param name="count"></param>
    /// <param name="general"></param>
    /// <param name="daily"></param>
    /// <param name="weekly"></param>
    public void ETCAchieve(string achieveType, int count = 1, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.Achieve(achieveType);
        SendSignal(key, count, general, daily, weekly);
    }

    /// <summary>
    /// 스테이지 클리어시 보내는 시그널
    /// </summary>
    /// <param name="stage">몇 번째 스테이지인지 스테이지 넘버를 보냄</param>
    /// <param name="general"></param>
    /// <param name="daily"></param>
    /// <param name="weekly"></param>
    public void StageClear(int stage, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.StageClear();
        SendSignal(key, stage, general, daily, weekly);
    }

    public void DeckComposition(DeckSynergy synergy, int count, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.DeckComposition(synergy);
        SendSignal(key, count, general, daily, weekly);
    }

    public void RankUp(ItemType rankKey, int rankCount, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.RankUp(rankKey);
        SendSignal(key, rankCount, general, daily, weekly);
    }

    public void Tutorial(string def, bool general = true, bool daily = true, bool weekly = true)
    {
        var key = QuestKeys.Tutorial(def);
        Debug.Log($"tutorialkey {key}");
        SendSignal(key, 1, general, daily, weekly);
    }

    private void SendSignal(string key, int count, bool general, bool daily, bool weekly)
    {
        if(general) _signal.Raise(QuestType_Enum.General, key, count);
        if(daily) _signal.Raise(QuestType_Enum.Daily, key, count);
        if(weekly) _signal.Raise(QuestType_Enum.Weekly, key, count);
    }

    public void OnCompleteQuest(string QuestId)
    {
        _signal.OnComplete(QuestId);
    }
    
    
}
