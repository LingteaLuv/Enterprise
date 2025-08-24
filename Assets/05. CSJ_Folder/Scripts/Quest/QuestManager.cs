using System;
using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.Sequence;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class QuestManager : MonoBehaviour, IRewardGiver
    {
        #region SerializedFields
        //기간제 퀘스트
        [SerializeField] private TemporaryQuestListSO _dailyQuestLists;
        [SerializeField] private TemporaryQuestListSO _weeklyQuestLists;
        // 퀘스트 시그널
        [SerializeField] private QuestSignalSO _signal;

        [SerializeField] private GeneralQuestDefinitionSO _stageClearTemplate;
        // 튜토리얼 시퀀스
        [SerializeField] private TutorialSequenceSO _tutorialSequence;
        // 루틴 시퀀스
        [SerializeField] private RoutineSequenceSO _routineSequence;
        //private QuestSaveData _save;

        [SerializeField] private GameObject QuestUI; 
        #endregion
        
        // quest Queue
        private Queue<TutorialQuestDefinitionSO>  _tutorialQuests; 
        private Queue<GeneralQuestDefinitionSO>  _generalQuests;
        
        // event Dictionary
        // 퀘스트 정의 딕셔너리 < questId, definitionSO>
        private Dictionary<string, QuestDefinitionSO> _defs;
        // 인스턴스 딕셔너리 < questId, Instance>
        private Dictionary<string, QuestInstance>  _instances;

        private int GeneralQuestCount = 1;
        private int ClearedQuestCount;
        private int CurrentQuestStage;
        private int CurrentClearedStage;

        private QuestUIController _questUI;

        // 이벤트
        // UI 퀘스트의 텍스트 업데이트
        public Action<QuestDefinitionSO, QuestInstance> OnQuestUpdated;
        // UI에 퀘스트 완료 표시와 연결
        public Action<QuestDefinitionSO, QuestInstance> OnQuestCompleted;
        // 추후 보상관련 사용
        public Action<QuestRewardSO.RewardEntry> OnRewardGranted;

        #region UnityLifeCycle

        private void Awake()
        {
            _defs = new Dictionary<string, QuestDefinitionSO>();
            _instances = new Dictionary<string, QuestInstance>();
            _tutorialQuests = new Queue<TutorialQuestDefinitionSO>();
            _generalQuests = new Queue<GeneralQuestDefinitionSO>();
            
            BuildCatalog();
            
            Load();
            if (_signal != null) _signal.OnSignal += OnSignal;
            
            EnsureGeneralActive();

            ActiveQuestUI();
        }

        private void OnDestroy()
        {
            if(_signal != null) _signal.OnSignal -= OnSignal;
            _questUI.OnRewardRequest -= ReceiveReward;
#if UNITY_EDITOR
            _questUI.OnForceClear -= ForceQuestComplete;
#endif
            Save();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        #endregion
        
        #region public

        private void OnStageCleared(int stage)
        {
            if(stage > CurrentClearedStage) CurrentClearedStage = stage;

            var activeGeneral = GetActiveGeneralInstance();
            if (activeGeneral == null) return;
            
            var def = _defs[activeGeneral.QuestId];
            if (def.GeneralType == GeneralType_Enum.StageClear && 
                CurrentClearedStage >= activeGeneral.needToClearStage)
            {
                MarkCompleted(def, activeGeneral);
            } 
        }
        
        public bool ActivateQuest(string questId)
        {
            return _defs.TryGetValue(questId, out var def) && EnsureActive(def);
        }
        
        [CanBeNull]
        public QuestInstance GetInstance(string questId)
        {
            _instances.TryGetValue(questId, out var inst);
            return inst;
        }
        
        #endregion

        //TODO
        private void OnSignal(QuestType_Enum type, string signalKey, int amount)
        {
            // TODO : 추후 기간제 퀘스트 쪽도 추가해야 함
            if (type != QuestType_Enum.General) return;
            if (signalKey == QuestKeys.StageClear())
            {
                OnStageCleared(amount);
                return;
            }
            
            var activeGeneral = GetActiveGeneralInstance();
            if  (activeGeneral == null) return;
            
            var def = _defs[activeGeneral.QuestId];
            var goal = def.Goal;
            if (goal == null) return;
                
            var delta = goal.ProgressDeltaFrom(signalKey, amount);
            if (delta <= 0) return;
            
            var before = activeGeneral.CurrentGoalCount;
            var require = def.Goal.RequireCount;
            activeGeneral.GoalCountAdjust(Mathf.Clamp(before + delta, 0, require));
            
            OnQuestUpdated?.Invoke(def, activeGeneral);
            
            if (IsInstanceComplete(def, activeGeneral))
                MarkCompleted(def, activeGeneral);
        }

        #region GeneralQuest

        private void GeneralQuestProgress()
        {
            //TODO : 추후 save 데이터에서 클리어 기록을 보고 Queue에서 이를 떼어냄
            if (_generalQuests.Count != 0) return;
            
            CurrentQuestStage++;
            
            InsertStageClearMission();
            InsertRoutineQuest();

            EnsureGeneralActive();
        }

        //TODO : 추후 어떻게 넣을지 생각하고 수정할 예정입니다.
        private void InsertStageClearMission()
        {
            if (CurrentClearedStage == 0) return;
            if (_stageClearTemplate == null) return;
             
            _generalQuests.Enqueue(_stageClearTemplate);
            
            if (!_instances.TryGetValue(_stageClearTemplate.questId, out var inst))
            {
                inst = new QuestInstance
                {
                    QuestId = _stageClearTemplate.questId,
                    QuestState = QuestState_Enum.BeforeActive,
                    GeneralQuestCount = GeneralQuestCount,
                    needToClearStage = CurrentQuestStage,
                    stageClearMission = $"스테이지 {CurrentQuestStage}를 클리어 하세요"
                };
                _instances.Add(_stageClearTemplate.questId, inst);
            }
            else
            {
                inst.QuestState = QuestState_Enum.BeforeActive;
                inst.GeneralQuestCount = GeneralQuestCount;
                inst.needToClearStage = CurrentQuestStage;
                inst.GoalCountInit();
                inst.stageClearMission = $"스테이지 {CurrentQuestStage}를 클리어 하세요";
            }

            GeneralQuestCount++;

            if (GetActiveGeneralInstance() == null)
                EnsureActive(_stageClearTemplate);

        }
        
        private void InsertRoutineQuest()
        {
            int target = _tutorialQuests.Count >0 ?_tutorialQuests.Peek().targetStage : int.MaxValue;
            foreach (var quest in GetNextParity())
            {
                if (target <= GeneralQuestCount)
                {
                    InsertTutorials();
                    target = _tutorialQuests.Count >0 ?_tutorialQuests.Peek().targetStage : int.MaxValue;
                }
    
                // TODO : 만약 CLearMin의 내림차순 정렬이 보장되어 있다면 return으로 변경 
                if (quest.RequiredQuestClearMin > GeneralQuestCount)
                {
                    continue;
                }
                _generalQuests.Enqueue(quest);
                GeneralQuestCount++;
            }
        }

        private RoutineQuestDefinitionSO[] GetNextParity()
        {
            bool isOdd = (CurrentQuestStage & 1) == 1;
            RoutineQuestDefinitionSO[] sequence =
                    isOdd ? 
                        _routineSequence.OddRoutine:  
                        _routineSequence.EvenRoutine;

            return sequence;
        }
        
        private void InsertTutorials()
        {
            if (_tutorialQuests.Count == 0) return;
            while (_tutorialQuests.Count >0 &&_tutorialQuests.Peek().targetStage <= GeneralQuestCount)
            {
                if (_instances.TryGetValue(_tutorialQuests.Peek().questId, out var inst))
                {
                    if (inst.QuestState != QuestState_Enum.BeforeActive)
                    {
                        _tutorialQuests.Dequeue();
                        continue;
                    }
                }
                _generalQuests.Enqueue(_tutorialQuests.Dequeue());
                GeneralQuestCount++;
            }
        }

        private void EnsureGeneralActive()
        { 
            if (GetActiveGeneralInstance() != null) return;
            Debug.Log("Ensuring general active instance");
            if (_generalQuests.Count == 0)
            {
                GeneralQuestProgress();
                return;
            }
            
            var next = _generalQuests.Dequeue();
            EnsureActive(next);
        }
        
        #endregion
        
        #region QuestUtillity

        private bool IsInstanceComplete(QuestDefinitionSO def, QuestInstance inst)
        {
            return inst.IsCompleted(def.Goal.RequireCount);
        }

        private void MarkCompleted(QuestDefinitionSO def, QuestInstance inst)
        {
            if(!IsInstanceComplete(def, inst)) return;

            inst.QuestState = QuestState_Enum.Completed;
                        
            OnQuestCompleted?.Invoke(def, inst);

            Save();
        }
        

        private void OnGeneralCompleted()
        {
            Debug.Log("General completed");
            ClearedQuestCount++;
            
            EnsureGeneralActive();
            RefreshActiveQuestUI();
        }

        private bool EnsureActive(QuestDefinitionSO def)
        {
            if (def.isGeneral && GetActiveGeneralInstance() != null) return false;

            if (!_instances.TryGetValue(def.questId, out var inst))
            {
                inst = new QuestInstance
                {
                    QuestId = def.questId,
                    QuestState = QuestState_Enum.Active,
                    GeneralQuestCount = ClearedQuestCount + 1,
                    
                };
                _instances.Add(def.questId, inst);
            }
            else
            {
                inst.QuestState = QuestState_Enum.Active;
                inst.GoalCountInit();
                inst.GeneralQuestCount = ClearedQuestCount + 1;
                GeneralQuestCount = ClearedQuestCount + 1;
                
            }

            RefreshActiveQuestUI();
            return true;
        }

        private QuestInstance GetActiveGeneralInstance()
        {
            foreach (var kv in _instances)
            {
                var def  = _defs[kv.Key];
                if (!def.isGeneral) continue;
                if (kv.Value.QuestState == QuestState_Enum.Active) return kv.Value;
            }

            return null;
        }
        private QuestDefinitionSO GetActiveQuestDefinition()
        {
            return _defs.GetValueOrDefault(GetActiveGeneralInstance().QuestId);
        }
        
        private void ReceiveReward(QuestDefinitionSO def, QuestInstance inst)
        {
            def.Reward.AddReward(this);
            
            OnGeneralCompleted();

            Save();
        }

        private void ActiveQuestUI()
        {
            _questUI = QuestUI.GetComponentInChildren<QuestUIController>();
            _questUI.QuestNumber = ClearedQuestCount + 1;
            QuestInstance inst = GetActiveGeneralInstance();
            _defs.TryGetValue(inst.QuestId, out var so);
            if (so == null)
            {
                Debug.LogError($"instance {inst.QuestId} 미 발견");
                return;
            }

            _questUI.QuestDef = so;
            _questUI.QuestInst = inst;
            
            QuestUI.SetActive(true);
            
            _questUI.OnRewardRequest += ReceiveReward;
            OnQuestCompleted += _questUI.UpdateQuest;
            OnQuestUpdated += _questUI.UpdateQuest;
#if UNITY_EDITOR
            _questUI.OnForceClear += ForceQuestComplete;
#endif
            OnQuestUpdated?.Invoke(GetActiveQuestDefinition(), GetActiveGeneralInstance());
        }

        private void RefreshActiveQuestUI()
        {
            if (_questUI == null) return;
            var inst = GetActiveGeneralInstance();
            if (inst == null) return;
            if (!_defs.TryGetValue(inst.QuestId, out var def)) return;

            _questUI.QuestNumber = inst.GeneralQuestCount;
            _questUI.UpdateQuest(def, inst);
        }
        #endregion
        

        // 데이터 베이스 연동은 추후 추가
        #region dataSetting
        private void BuildCatalog()
        {
            _defs.Clear();
            if (_tutorialSequence != null && _tutorialSequence.Quests != null)
            {
                foreach (var t in _tutorialSequence.Quests)
                {
                    if (_defs.ContainsKey(t.tutorial.questId)) continue;
                    _defs.Add(t.tutorial.questId, t.tutorial);
                    _tutorialQuests.Enqueue(t.tutorial);
                }
            }

            if (_routineSequence != null)
            {
                foreach (var t in _routineSequence.OddRoutine)
                {
                    _defs.TryAdd(t.questId, t);
                }
                foreach (var t in _routineSequence.EvenRoutine)
                {
                    _defs.TryAdd(t.questId, t);
                }
            }
            
            // TODO : 일일 퀘스트와 주간 퀘스트 추가
            
            if(_stageClearTemplate && !_defs.ContainsKey(_stageClearTemplate.questId))
            {
                _defs.TryAdd(_stageClearTemplate.questId, _stageClearTemplate);
            }
        }

        private void Save()
        {
            
        }

        private void Load()
        {
            
        }
        #endregion

        #region IRewardGiver
        //TODO : 인벤토리등과 연계하여 지급
        public void Give(QuestRewardSO.RewardEntry entry)
        {
            OnRewardGranted?.Invoke(entry);
            // TODO : Inventory.Add(entry); || Inventory.Add(entry.RewardType, entry.Amount); 등 진행
        }
        #endregion

        // TODO : 추후 기간제 퀘스트 생성
        private void TemporaryQuestInit()
        {
            
        }

#if  UNITY_EDITOR
        private void ForceQuestComplete()
        {
            QuestInstance inst = GetActiveGeneralInstance();
            Debug.Log(inst);
            inst.ForceComplete();   
            MarkCompleted(GetActiveQuestDefinition(),inst);
        }
#endif

    }
}
