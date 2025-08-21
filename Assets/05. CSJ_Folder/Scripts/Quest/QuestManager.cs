using System;
using System.Collections.Generic;
using System.Linq;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.Sequence;
using JetBrains.Annotations;
using UnityEngine;

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
        private int ClearedQuestCount = 0;
        private int CurrentQuestStage = 0;
        private int CurrentClearedStage = 0;

        // 이벤트
        public Action<QuestDefinitionSO, QuestInstance> OnQuestUpdated;
        public Action<QuestDefinitionSO, QuestInstance> OnQuestCompleted;
        public Action<QuestRewardSO.RewardEntry> OnRewardGranted;
        private IRewardGiver _rewardGiverImplementation;

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
        }

        private void OnDestroy()
        {
            if(_signal != null) _signal.OnSignal -= OnSignal;
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
            if (activeGeneral == null)return;
            
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
        
        public void ReceiveReward(QuestDefinitionSO def, QuestInstance inst)
        {
            def.Reward.AddReward(this);
            
            OnGeneralCompleted(def, inst);

            Save();
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
                };
                _instances.Add(_stageClearTemplate.questId, inst);
            }
            else
            {
                inst.QuestState = QuestState_Enum.BeforeActive;
                inst.GeneralQuestCount = GeneralQuestCount;
                inst.needToClearStage = CurrentQuestStage;
                inst.GoalCountAdjust(inst.CurrentGoalCount+1);
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
                        (RoutineQuestDefinitionSO[])_routineSequence.OddRoutine:  
                        (RoutineQuestDefinitionSO[])_routineSequence.EvenRoutine;

            return sequence;
        }
        
        private void InsertTutorials()
        {
            if (_tutorialQuests.Count == 0) return;

            
            while (_tutorialQuests.Count >0 &&_tutorialQuests.Peek().targetStage <= GeneralQuestCount)
            {
                if (_instances.TryGetValue(_tutorialQuests.Peek().questId, out var inst))
                {
                    if (inst.QuestState == QuestState_Enum.Received || 
                        inst.QuestState == QuestState_Enum.Active || 
                        inst.QuestState == QuestState_Enum.Completed)
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
            if (_generalQuests.Count == 0)
            {
                GeneralQuestProgress();
                return;
            }
            
            var next = _generalQuests.Dequeue();
            EnsureActive(next);
        }
        
        #endregion

        private bool IsInstanceComplete(QuestDefinitionSO def, QuestInstance inst)
        {
            return inst.IsCompleted(def.Goal.RequireCount);
        }

        private void MarkCompleted(QuestDefinitionSO def, QuestInstance inst)
        {
            if(!IsInstanceComplete(def, inst)) return;

            inst.QuestState = QuestState_Enum.Completed;

            Save();
        }
        

        private void OnGeneralCompleted(QuestDefinitionSO def, QuestInstance inst)
        {
            ClearedQuestCount++;
            
            OnQuestCompleted?.Invoke(def, inst);
            
            EnsureGeneralActive();
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
                    GeneralQuestCount = ClearedQuestCount,
                };
                _instances.Add(def.questId, inst);
            }
            else
            {
                inst.QuestState = QuestState_Enum.Active;
            }

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

        //TODO : 인벤토리등과 연계하여 지급
        public void Give(QuestRewardSO.RewardEntry entry)
        {
            OnRewardGranted?.Invoke(entry);
        }

        // TODO : 추후 기간제 퀘스트 생성
        private void TemporaryQuestInit()
        {
            
        }

    }
}
