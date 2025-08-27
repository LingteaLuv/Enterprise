using System;
using System.Collections;
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
        #region Def

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
        private Queue<RoutineQuestDefinitionSO> _oddSequence;
        private Queue<RoutineQuestDefinitionSO> _evenSequence;
        
        // event Dictionary
        // 퀘스트 정의 딕셔너리 < questId, definitionSO>
        private Dictionary<string, QuestDefinitionSO> _defs;
        // 인스턴스 딕셔너리 < questId, Instance>
        private Dictionary<string, QuestInstance>  _instances;

        // 일반 퀘스트 번호, 생성할 때마다 +1
        private int GeneralQuestCount = 1;
        // 클리어한 퀘스트 개수, 완료 버튼을 누를때 +1
        private int ClearedQuestCount;
        // 현재 퀘스트가 요구하는 스테이지, 완료 버튼을 눌러서 새 사이클이 시작될 때 +1
        private int CurrentQuestStage;
        // 현재 플레이어가 클리어한 스테이지, 클리어 스테이지를 signal로 받아 +1
        private int CurrentClearedStage;

        // QuestUI와의 연동
        private QuestUIController _questUI;

        // 이벤트
        // UI 퀘스트의 텍스트 업데이트
        public Action<QuestDefinitionSO, QuestInstance> OnQuestUpdated;
        // UI에 퀘스트 완료 표시와 연결
        public Action<QuestDefinitionSO, QuestInstance> OnQuestCompleted;
        // 추후 보상관련 사용
        public Action<QuestRewardSO.RewardEntry> OnRewardGranted;
        #endregion

        #region UnityLifeCycle

        private void Awake()
        {
            // 자료구조 초기화
            _defs = new Dictionary<string, QuestDefinitionSO>();
            _instances = new Dictionary<string, QuestInstance>();
            _tutorialQuests = new Queue<TutorialQuestDefinitionSO>();
            _generalQuests = new Queue<GeneralQuestDefinitionSO>();
            
            // 현재 존재하는 퀘스트 목록들을 _defs 딕셔너리에 추가
            BuildCatalog();
            
            // 불러오기 (현재 미구현)
            Load();
            // 시그널 SO가 존재할 경우
            if (_signal != null)
            {
                // 중복 방지
                _signal.OnSignal -= OnSignal;
                _signal.OnSignal += OnSignal;
            }
            
            // 일반 퀘스트 동작
            GeneralQuestEnqueue();
            EnsureGeneralActive();

            // TODO : 기간 퀘스트 동작
            
            // 퀘스트 UI 활성화
            ActiveQuestUI();
        }

        private void OnDestroy()
        {
            // 시그널 이벤트 구독 해제
            if(_signal != null) _signal.OnSignal -= OnSignal;
            // 퀘스트 UI 구독 해제
            _questUI.OnRewardRequest -= ReceiveReward;
#if UNITY_EDITOR
            // 테스트용 코드 구독해제
            _questUI.OnForceClear -= ForceQuestComplete;
#endif
            // 변경 사항 저장
            Save();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        #endregion
        
        #region public (현재 사용중 x)
        // 추후 퀘스트 강제 작동?
        public bool ActivateQuest(string questId)
        {
            return _defs.TryGetValue(questId, out var def) && EnsureActive(def);
        }
        
        // 퀘스트 ID를 받으면 해당 퀘스트의 인스턴스 반환
        [CanBeNull]
        public QuestInstance GetInstance(string questId)
        {
            if(_instances.TryGetValue(questId, out var inst))
                return inst;
            Debug.LogError($"Quest not found: {questId}");
            return null;
        }
        #endregion

        #region GeneralQuest

        /// <summary>
        /// 일반 퀘스트를 큐에 채우는 메서드입니다.
        /// 스테이지 클리어 -> 튜토리얼 -> 루틴 순으로 탐색하고 삽입합니다.
        /// </summary>
        private void GeneralQuestEnqueue()
        {
            //TODO : 추후 Save 데이터와 연동
            
            // 넣을 GeneralQuest가 없다면 중단
            if (_generalQuests.Count != 0) return;
            
            // 우선 스테이지 클리어 미션을 삽입
            InsertStageClearMission();
            
            // 이후 현재 퀘스트 진행 스테이지를 +1 
            CurrentQuestStage++;
            
            // 반복 퀘스트 삽입
            InsertRoutineQuest();
        }

        //TODO : 추후 어떻게 넣을지 생각하고 수정할 예정입니다.
        /// <summary>
        /// 스테이지 클리어 미션을 삽입하는 메서드입니다.
        /// </summary>
        private void InsertStageClearMission()
        {
            // 만약 게임의 시작이라면 return (클리어한 스테이지가 없다면)
            if (CurrentClearedStage == 0) return;
            // 스테이지 클리어 템플릿이 없다면 return
            if (_stageClearTemplate == null) return;
            // 일반 퀘스트가 현재 존재한다면 리턴
            if (_generalQuests.Count != 0) return;
            // 현재 작동중인 일반 퀘스트가 존재한다면 리턴 -> Queue에서 마지막 원소를 꺼내 작동중인 경우
            if (HasActiveGeneral()) return;
            
            // 일반 퀘스트 큐에 스테이지 클리어 미션을 삽입
            _generalQuests.Enqueue(_stageClearTemplate);
            
            // 해당 스테이지 클리어 미션 템플릿의 인스턴스를 찾을 수 없다면
            if (!_instances.TryGetValue(_stageClearTemplate.questId, out var inst))
            {
                // 인스턴스를 새로 제작
                inst = new QuestInstance
                {
                    // 퀘스트 id는 스테이지 템플릿의 것 (아마 4000 쓸 것)
                    QuestId = _stageClearTemplate.questId,
                    // 퀘스트의 상태는 활성화 전 상태로 초기 설정
                    QuestState = QuestState_Enum.BeforeActive,
                    // 퀘스트 번호는 지금까지 삽입한 퀘스트의 개수 + 1
                    GeneralQuestCount = GeneralQuestCount,
                    // 퀘스트 목표인 클리어해야하는 스테이지는 현재 퀘스트 스테이지 (GeneralQueueEnqueue()를 할 때마다 +1)
                    needToClearStage = CurrentQuestStage,
                    // 스테이지 미션 string 삽입
                    stageClearMission = $"스테이지 {CurrentQuestStage}를 클리어 하세요"
                };
                // 해당 미션과 인스턴스 연결
                _instances.Add(_stageClearTemplate.questId, inst);
            }
            // 인스턴스를 찾았다면
            else
            {
                // 해당 인스턴스를 업데이트
                // 인스턴스의 상태를 활성화 전으로 재설정
                inst.QuestState = QuestState_Enum.BeforeActive;
                // 퀘스트 번호는 지금까지 삽입한 퀘스트의 개수 + 1
                inst.GeneralQuestCount = GeneralQuestCount;
                // 퀘스트 목표인 클리어해야하는 스테이지는 현재 퀘스트 스테이지 (GeneralQueueEnqueue()를 할 때마다 +1)
                inst.needToClearStage = CurrentQuestStage;
                // 인스턴스의 목표 수치를 0으로 초기화
                inst.GoalCountInit();
                // 스테이지 미션 string 삽입
                inst.stageClearMission = $"스테이지 {CurrentQuestStage}를 클리어 하세요";
            }
            // 일반 퀘스트를 삽입 했으므로 +1; 
            GeneralQuestCount++;

        }
        
        /// <summary>
        /// 일반 퀘스트를 삽입한다.
        /// </summary>
        private void InsertRoutineQuest()
        {
            // 튜토리얼 퀘스트 큐에 원소가 존재할 경우 맨 앞의 값으로 없다면 int.max로 값을 채운다
            int target = _tutorialQuests.Count > 0 ?_tutorialQuests.Peek().targetStage : int.MaxValue;
            // 다음 사이클 큐에 들어있느 퀘스트들을 검사한다.
            foreach (var quest in GetNextParity())
            {
                // 타깃이 현재 퀘스트 번호 +1 보다 작거나 같은 경우 = 타깃 차례인 경우
                if (target <= GeneralQuestCount + 1)
                {
                    // 튜토리얼 삽입을 진행한다
                    InsertTutorials();
                    // 타깃을 다음 대상으로 변경한다.
                    target = _tutorialQuests.Count > 0 ?_tutorialQuests.Peek().targetStage : int.MaxValue;
                }
    
                // ※ quest의 경우 quest 번호로 정렬된 큐이므로 quest 번호 순으로 최초 삽입 스테이지가 정렬된다고 가정  
                // 추후 앞부분에 quest가 추가될 예정이라면 이부분에 변경 필요
                if (quest.RequiredQuestClearMin > GeneralQuestCount)
                {
                    return;
                }
                
                //일반 퀘스트에 삽입
                _generalQuests.Enqueue(quest);
                GeneralQuestCount++;
                if (target == int.MaxValue) return;
            }
        }

        /// <summary>
        /// 다음 사이클에 들어갈 루틴 퀘스트를 반환한다
        /// </summary>
        private Queue<RoutineQuestDefinitionSO> GetNextParity()
        {
            // 비트 연산을 통해 홀수 검사를 진행한다 홀수라면 true
            bool isOdd = (CurrentQuestStage & 1) == 1;
            // isOdd값에 따라서 맞는 시퀀스를 반환한다.
            return isOdd ? _oddSequence : _evenSequence;
        }
        
        /// <summary>
        /// 튜토리얼을 삽입합니다
        /// </summary>
        private void InsertTutorials()
        {
            // 만약 튜토리얼 큐의 COUNT가 0인 경우 탈출
            if (_tutorialQuests.Count == 0) return;
            
            // 튜토리얼이 있고 타깃 차례인 경우 (2차 검사)
            while (_tutorialQuests.Count >0 &&_tutorialQuests.Peek().targetStage <= GeneralQuestCount + 1)
            {
                // 튜토리얼 큐의 제일 앞의 questid를 검사하고
                if (_instances.TryGetValue(_tutorialQuests.Peek().questId, out var inst))
                {
                    // 퀘스트의 상태가 활성화 전 상태가 아니라면
                    if (inst.QuestState != QuestState_Enum.BeforeActive)
                    {
                        // 튜토리얼 퀘스트 큐에서 제거하고 계속
                        _tutorialQuests.Dequeue();
                        continue;
                    }
                }
                // 활성화 상태라면 큐에 추가한다.
                _generalQuests.Enqueue(_tutorialQuests.Dequeue());
                GeneralQuestCount++;
            }
        }

        /// <summary>
        /// 일반 퀘스트를 작동시키는 함수
        /// </summary>
        private void EnsureGeneralActive()
        { 
            // 만일 활성화된 인스턴스가 존재한다면 탈출
            if (GetActiveGeneralInstance() != null) return;
            // 일반 퀘스트 큐가 비어있다면 일반 퀘스트 큐를 채우는 것을 진행
            if (_generalQuests.Count == 0)
            {
                // 일반 퀘스트 삽입
                GeneralQuestEnqueue();
                // 만약 그래도 큐가 비어있다면 큐 채우기에 문제가 발생한 것이므로 에러 발생
                if (_generalQuests.Count == 0)
                {
                    Debug.LogError($"현재 일반 퀘스트 큐에 퀘스트를 삽입할 수 없습니다. 퀘스트 목록을 확인해주세요");
                    return;
                }
            }
            
            // 일반퀘스트 큐에서 퀘스트를 꺼냄
            var next = _generalQuests.Dequeue();
            // 인자로 해당 퀘스트를 넘김
            EnsureActive(next);
        }
        
        #endregion
        
        #region QuestUtillity

        /// <summary>
        /// SignalManager의 함수를 이벤트로 구독하고 해당 이벤트에 따른 함수 처리를 진행한다. 
        /// </summary>
        /// <param name="type">현재 퀘스트 타입</param>
        /// <param name="signalKey">퀘스트의 키 값</param>
        /// <param name="amount">퀘스트의 성취도</param>
        private void OnSignal(QuestType_Enum type, string signalKey, int amount)
        {
            // TODO : 추후 기간제 퀘스트 쪽도 추가해야 함
            if (type != QuestType_Enum.General) return;
            
            // 키 값이 stageclear인 경우
            if (signalKey == QuestKeys.StageClear())
            {
                // 스테이지 클리어 처리 후 탈출
                OnStageCleared(amount);
                return;
            }
            
            // 현재 작동중인 인스턴스의 정보를 받는다.
            var activeGeneral = GetActiveGeneralInstance();
            if  (activeGeneral == null) return;
            
            // 현재 작동중인 인스턴스의 정의를 가져온다
            var def = GetActiveQuestDefinition();
            var goal = def.Goal;
            if (goal == null) return;
                
            // 목표에서 현재 퀘스트에 대한 키인지 확인한다.
            var delta = goal.ProgressDeltaFrom(signalKey, amount);
            // 만약 진척도가 <=0이라면; 키가 일치하지 않는다면
            if (delta <= 0) return;
            
       
            // 현재 작동중인 인스턴스의 목표 수치를 가져온다
            var before = activeGeneral.CurrentGoalCount;
            // 정의의 목표 수치를 가져온다
            var require = def.Goal.RequireCount;
            // 작동중인 인스턴스의 목표 수치를 0과 정의 수치 사이로 조정한다.
            activeGeneral.GoalCountAdjust(Mathf.Clamp(before + delta, 0, require));
            
            
            // 퀘스트 사양이 바뀌었으므로 업데이트 이벤트를 호출한다 
            OnQuestUpdated?.Invoke(def, activeGeneral);
            
            // 만약 갱신한 인스턴스의 완료 조건이 충족됬을 경우
            if (IsInstanceComplete(def, activeGeneral))
                // ui에 클리어로 전환합니다.
                MarkCompleted(def, activeGeneral);
        }
        
        // 스테이지 클리어 시 
        private void OnStageCleared(int stage)
        {
            // 만약 갱신된 스테이지가 현재 클리어한 스테이지 보다 큰 경우 그 값으로 변경합니다.
            if(stage > CurrentClearedStage) CurrentClearedStage = stage;
            
            var activeGeneral = GetActiveGeneralInstance();
            if (activeGeneral == null) return;

            var def = GetActiveQuestDefinition();
            
            // 만약 현재 인스턴스의 def가 스테이지 클리어이고 완료조건이 충족되었다면
            if (def.GeneralType == GeneralType_Enum.StageClear && 
                CurrentClearedStage >= activeGeneral.needToClearStage)
            {
                // ui에 클리어 표시를 진행합니다.
                MarkCompleted(def, activeGeneral);
            } 
        }
        
        
        /// <summary>
        /// 인스턴스의 퀘스트 완료여부를 판별합니다.
        /// </summary>
        private bool IsInstanceComplete(QuestDefinitionSO def, QuestInstance inst)
        {
            return inst.IsCompleted(def.Goal.RequireCount);
        }

        /// <summary>
        /// 퀘스트를 완료 상태로 전환합니다.
        /// </summary>
        private void MarkCompleted(QuestDefinitionSO def, QuestInstance inst)
        {
            if(!IsInstanceComplete(def, inst)) return;

            // 퀘스트 상태를 완료상태로 전환
            inst.QuestState = QuestState_Enum.Completed;
                        
            // 퀘스트가 완료되었다는 사실을 전달합니다.
            OnQuestCompleted?.Invoke(def, inst);

            Save();
        }
        

        /// <summary>
        /// 일반 퀘스트의 보상 처리가 완료되었을 때의 처리를 진행합니다.
        /// </summary>
        private void OnGeneralCompleted()
        {
            // 퀘스트 클리어 카운트를 +1합니다
            ClearedQuestCount++;
            
            // 다음 일반 퀘스트를 작동시킵니다.
            EnsureGeneralActive();
            // QuestUI를 새로고침합니다
            RefreshActiveQuestUI();
        }

        /// <summary>
        /// 인자로 받은 퀘스트를 실행시킵니다. 
        /// </summary>
        private bool EnsureActive(QuestDefinitionSO def)
        {
            // 퀘스트가 일반 퀘스트이며, 현재 인스턴스가 활성화 중인 경우 탈출 
            if (def.isGeneral && GetActiveGeneralInstance() != null) return false;

            // 인스턴스에서 questId로 탐색 실패시 (=_instances에 def가 없는 경우)
            if (!_instances.TryGetValue(def.questId, out var inst))
            {
                // 새로운 인스턴스를 생성합니다.
                inst = new QuestInstance
                {
                    QuestId = def.questId,
                    QuestState = QuestState_Enum.Active,
                    GeneralQuestCount = ClearedQuestCount + 1,
                };
                // 인스턴스를 추가합니다.
                _instances.Add(def.questId, inst);
            }
            // 탐색에 성공했다면
            else
            {
                // 인스턴스를 활성화 상태로 변경
                inst.QuestState = QuestState_Enum.Active;
                // 인스턴스의 카운트를 초기화
                inst.GoalCountInit();
                GeneralQuestCount = ClearedQuestCount + 1;
                inst.GeneralQuestCount = GeneralQuestCount;
            }
            
            RefreshActiveQuestUI();
            return true;
        }
        
        /// <summary>
        /// 활성화된 인스턴스를 받아옴
        /// </summary>
        private QuestInstance GetActiveGeneralInstance()
        {
            // 인스턴스에 존재하는 키밸류들을 받아온다
            foreach (var kv in _instances)
            {
                // 키에 해당하는 정의
                var def  = _defs[kv.Key];
                // 만약 해당 정의가 일반 퀘스트가 아니라면 다시 반복한다 
                if (!def.isGeneral) continue;
                // 만약 해당 밸류 값의 인스턴스가 활성화 상태라면 이를 반환합니다. 
                if (kv.Value.QuestState == QuestState_Enum.Active) return kv.Value;
            }
            
            Debug.LogError("활성화 인터페이스 탐색 실패");
            // 탐색에 실패하면 null값을 반환합니다.
            return null;
        }
        
        /// <summary>
        /// 활성화된 인스턴스의 일반 퀘스트 정의를 반환합니다.
        /// </summary>
        private QuestDefinitionSO GetActiveQuestDefinition()
        {
            var inst = GetActiveGeneralInstance();
            return inst == null ? null : _defs.GetValueOrDefault(inst.QuestId);
        }
        
        /// <summary>
        /// 현재 활성화중인 일반 퀘스트가 있는지 반환합니다.
        /// </summary>
        private bool HasActiveGeneral() => GetActiveGeneralInstance() != null;
        
        
        /// <summary>
        /// 보상 획득을 진행합니다
        /// </summary>
        private void ReceiveReward(QuestDefinitionSO def, QuestInstance inst)
        {
            // def의 보상에서 획득을 호출합니다.
            def.Reward.AddReward(this);
            
            if (def.isGeneral)
                // 일반 퀘스트의 보상처리를 마무리합니다.
                OnGeneralCompleted();

            // 저장을 진행합니다
            Save();
        }
        #endregion

        #region UI
        /// <summary>
        /// questUI를 작동시킵니다.
        /// 구독 타이밍 보장을 위해 따로 처리
        /// </summary>
        private void ActiveQuestUI()
        {
            // questUI에서 UIController를 받아옵니다
            _questUI = QuestUI.GetComponentInChildren<QuestUIController>();
            if (_questUI == null) return;
            // 퀘스트 ui의 번호를 등록합니다
            _questUI.QuestNumber = ClearedQuestCount + 1;
            QuestInstance inst = GetActiveGeneralInstance();
            
            // _defs에서 questId로 탐색을 진행합니다
            _defs.TryGetValue(inst.QuestId, out var so);
            // so가 비어있다면 에러 메시지 표시
            if (so == null)
            {
                Debug.LogError($"instance {inst.QuestId} 미 발견");
                return;
            }

            // 퀘스트 UI에 값 지정
            _questUI.QuestDef = so;
            _questUI.QuestInst = inst;
            
            // 퀘스트 ui 활성화
            QuestUI.SetActive(true);
            
            // 퀘스트 ui 관련 구독 진행
            _questUI.OnRewardRequest += ReceiveReward;
            OnQuestCompleted += _questUI.UpdateQuest;
            OnQuestUpdated += _questUI.UpdateQuest;
#if UNITY_EDITOR
            _questUI.OnForceClear += ForceQuestComplete;
#endif
            
            // 퀘스트 ui에 퀘스트 정보를 전달합니다
            OnQuestUpdated?.Invoke(GetActiveQuestDefinition(), GetActiveGeneralInstance());
        }

        /// <summary>
        /// 퀘스트 ui를 새로고침 합니다
        /// </summary>
        private void RefreshActiveQuestUI()
        {
            // questUI가 비할당된 경우 탈출
            if (_questUI == null) return;
            // 활성화된 인스턴스를 받아옵니다
            var inst = GetActiveGeneralInstance();
            if (inst == null) return;
            if (!_defs.TryGetValue(inst.QuestId, out var def)) return;

            // 퀘스트UI의 퀘스트 번호를 인스턴스의 퀘스트 번호로 초기화
            _questUI.QuestNumber = inst.GeneralQuestCount;
            // 퀘스트UI에 퀘스트 정보를 업데이트
            _questUI.UpdateQuest(def, inst);
        }
        

        #endregion
        // 데이터 베이스 연동은 추후 추가
        #region dataSetting
        /// <summary>
        /// 딕셔너리와 queue의 초기값들을 설정합니다
        /// </summary>
        private void BuildCatalog()
        {
            _defs.Clear();
            _instances.Clear();
            // tutorialSequence가 존재한다면
            if (_tutorialSequence != null && _tutorialSequence.tutorial != null)
            {
                // tutorialQuests에 sequence 내부 def들을 questId로 정렬해서 queue에 삽입
                _tutorialQuests = EnqueueOrderById(_tutorialSequence.tutorial);
            }

            if (_routineSequence != null)
            {
                // 각 사이클 별 시퀀스에 sequence 내부 def들을 questId로 정렬해서 queue에 삽입
                _oddSequence = EnqueueOrderById(_routineSequence.OddRoutine);
                _evenSequence = EnqueueOrderById(_routineSequence.EvenRoutine);
            }
            
            // TODO : 일일 퀘스트와 주간 퀘스트 추가
            
            // 스테이지 클리어 템플릿이 존재하고 이게 defs에 없다면
            if(_stageClearTemplate && !_defs.ContainsKey(_stageClearTemplate.questId))
            {
                // def에 추가합니다
                _defs.TryAdd(_stageClearTemplate.questId, _stageClearTemplate);
            }
        }

        /// <summary>
        /// questId를 Parse합니다
        /// </summary>
        private static int ParseQuestOrder(string questId)
        {
            // 만약 questId가 null이거나 비었다면 0을 반환
            if (string.IsNullOrEmpty(questId)) return 0;
            // 만약 QuestId를 숫자로 변환할 수 있으면 해당 값, 없다면 0을 반환
            return int.TryParse(questId, out var result) ? result : 0;
        }

        /// <summary>
        /// 배열을 받아서 questid순으로 정렬
        /// </summary>
        /// <param name="sequence">T 배열을 인자로 받는다</param>
        /// <typeparam name="T">정렬할 대상 , QuestDefinitionSO를 상속받아야한다</typeparam>
        private Queue<T> EnqueueOrderById<T>(T[] sequence) where T : QuestDefinitionSO
        {
            var _sequence = new List<T>();
            var _queue = new Queue<T>();
            // 입력받은 리스트를 순회
            foreach (var t in sequence)
            {
                // 딕셔너리에 삽입 시도 후 _sequence에 추가
                _defs.TryAdd(t.questId, t);
                _sequence.Add(t);
            }
            // 순회를 마친 후 _sequence를 questId로 정렬하고 List로 저장 
            var sorted = _sequence
                .OrderBy(t => ParseQuestOrder(t.questId))
                .ToList();
            // 저장된 리스트를 순회하며 큐에 삽입
            foreach (var t in sorted)
            {
                _queue.Enqueue(t);
            }

            return _queue;
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
            CurrentClearedStage++;
            QuestInstance inst = GetActiveGeneralInstance();
            inst.ForceComplete();   
            MarkCompleted(GetActiveQuestDefinition(),inst);
        }
#endif

    }
}
