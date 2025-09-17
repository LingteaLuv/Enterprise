using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _05._CSJ_Folder.Scripts.Quest.Data;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.Sequence;
using JetBrains.Annotations;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class QuestManager : Singleton<QuestManager>, IRewardGiver
    {
        #region Def

        #region SerializedFields
        // 퀘스트 시그널
        [SerializeField] private QuestSignalSO _signal;

        [SerializeField] private GeneralQuestDefinitionSO _stageClearTemplate;
        // 튜토리얼 시퀀스
        [SerializeField] private TutorialSequenceSO _tutorialSequence;
        // 루틴 시퀀스
        [SerializeField] private RoutineSequenceSO _routineSequence;

        [SerializeField] private TemporaryQuestListSO _temporaryQuest;
        //private QuestSaveData _save;
        #endregion
        
        // quest Queue
        private Queue<TutorialQuestDefinitionSO>  _tutorialQuests; 
        private Queue<GeneralQuestDefinitionSO>  _generalQuests;
        private Queue<RoutineQuestDefinitionSO> _oddSequence;
        private Queue<RoutineQuestDefinitionSO> _evenSequence;
        
        private Queue<TemporaryInstance> _dailyQuests;
        private Queue<TemporaryInstance> _weeklyQuests;
        private Queue<TemporaryInstance> _temporaryQuests;

        // event Dictionary
        // 퀘스트 정의 딕셔너리 < questId, definitionSO>
        private Dictionary<string, GeneralQuestDefinitionSO> _generalDefs;
        // 인스턴스 딕셔너리 < questId, Instance>
        
        private Dictionary<string, QuestInstance> _instances;
        private Dictionary<string, GeneralQuestInstance>  _generalInstances;

        private Dictionary<string, TemporaryInstance> _temporaryInstances;
        
        private List<string> _clearedTutorialQuestIds;
        private GeneralQuestInstance _completedInstance;
        
        // 클리어한 퀘스트 개수, 완료 버튼을 누를때 +1
        private int ClearedQuestCount;
        // 현재 퀘스트가 요구하는 스테이지, 완료 버튼을 눌러서 새 사이클이 시작될 때 +1
        private int CurrentQuestStage = 1;
        // 현재 플레이어가 클리어한 스테이지, 클리어 스테이지를 signal로 받아 +1
        public int CurrentClearedStage => GlobalStageManager.Instance.CurrentStageIndex.Value;
        
        // 현재 active 일반퀘스트의 Id를 캐싱
        private string _activeGeneralId;

        // QuestUI와의 연동
        private QuestUIController _questUI;
        private GameObject QuestUI; 
        private TemporaryQuestController _temporaryQuestController;

        // 이벤트
        // UI 퀘스트의 텍스트 업데이트
        public event Action<GeneralQuestDefinitionSO, GeneralQuestInstance> OnQuestUpdated;
        // UI에 퀘스트 완료 표시와 연결
        public event Action<GeneralQuestDefinitionSO, GeneralQuestInstance> OnQuestCompleted;
        
        public event Action<TemporaryInstance> OnTempoQuestUpdated;

        // 추후 보상관련 사용
        public event Action<QuestRewardSO.RewardEntry> OnRewardGranted;
        public event Action<TutorialQuestDefinitionSO> OnTutorialQuestCompleted;

        // 퀘스트 초기화 확인용 코루틴
        private Coroutine _nextResetCoroutine;
        // 퀘스트 리셋중인지 확인
        private bool _isResetting;
        private bool isLoaded;
        private bool _initialized;
        
        private const string QuestDataPath = "QuestData";
        
        // 저장에 사용할 디바운싱
        private bool _saveQueued;
        private bool _isSaving;
        private Coroutine _saveCoroutine;

        private const int CurrentDataVersion = 3;
        private static string QuestDataPathV => $"{QuestDataPath}_v{CurrentDataVersion}";
        
        #endregion

        #region UnityLifeCycle

        protected override void Awake()
        {
            base.Awake();
            // 자료구조 초기화
            _generalDefs = new Dictionary<string, GeneralQuestDefinitionSO>();
            _instances = new Dictionary<string, QuestInstance>();
            _generalInstances = new Dictionary<string, GeneralQuestInstance>();
            _temporaryInstances = new Dictionary<string, TemporaryInstance>();
            _tutorialQuests = new Queue<TutorialQuestDefinitionSO>();
            _generalQuests = new Queue<GeneralQuestDefinitionSO>();
            _dailyQuests = new();
            _weeklyQuests = new();
            _clearedTutorialQuestIds = new List<string>();

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // 시그널 이벤트 구독 해제
            if(_signal != null) 
            {
                _signal.OnSignal -= OnSignal;                
                _signal.OnSignalComplete -= OnSignalComplete;
            }

            if (_questUI != null)
            {
                // 퀘스트 UI 구독 해제
                _questUI.OnRewardRequest -= ReceiveReward;

                // 테스트용 코드 구독해제
                // TODO: 정식 출시시 삭제
                _questUI.OnForceClear -= ForceQuestComplete;
                _questUI.OnForceInit -= ForceQuestReset;

                OnQuestCompleted -= _questUI.UpdateQuest;
                OnQuestUpdated -= _questUI.UpdateQuest;
            }

            // 변경 사항 저장
            SaveImmediate();
        }

        protected new void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            SaveImmediate();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                RequestSave(0.1f);
            }
        }


        private IEnumerator Start()
        {
            yield return WaitForLogin();
            if (_initialized) yield break;
            yield return Initialize();

        }

        private IEnumerator Initialize()
        {
            _initialized = true;
            yield return DatabaseManager.Instance.EnsureServerOffset().AsIEnumerator();
            
            var dataTask = LoadAsync();
            yield return dataTask.AsIEnumerator();
            var loaded = dataTask.IsCompleted ? dataTask.Result : null;
            
            // 현재 존재하는 퀘스트 목록들을 _defs 딕셔너리에 추가
            BuildCatalog();
            
            // 시그널 SO가 존재할 경우
            if (_signal != null)
            {
                // 중복 방지
                _signal.OnSignal -= OnSignal;
                _signal.OnSignalComplete -= OnSignalComplete;
                _signal.OnSignal += OnSignal;                
                _signal.OnSignalComplete += OnSignalComplete;
            }
            // 기간 퀘스트 
            RegisterTemporaryQuests();


            if (loaded != null)
            {
                RestoreFromQuestData(loaded);
                RebuildGeneralQuest(loaded);
                
                var active = GetActiveGeneralInstance();
                _activeGeneralId = active?.QuestId;
                isLoaded = true;
                EnsureGeneralActive();
            }
            else
            {
                GeneralQuestEnqueue();
                EnsureGeneralActive();
            }
            
            ActiveQuestUI();
            
            bool dailyCheck = DatabaseManager.Instance.QuickDailyCheck();
            bool weeklyCheck = DatabaseManager.Instance.QuickWeeklyCheck();
            yield return CheckTemporaryQuestsReset(dailyCheck, weeklyCheck).AsIEnumerator();

            ScheduleNextResetTick();
        }

        private IEnumerator WaitForLogin()
        {
            while (AuthManager.Instance == null) yield return null;
            
            if (AuthManager.Instance.isLogined) yield break;

            bool done = false;
            void Handler() => done = true;
            AuthManager.Instance.LoginCompleted += Handler;
            yield return new WaitUntil(() => done || AuthManager.Instance.isLogined);
            AuthManager.Instance.LoginCompleted -= Handler;
        }

        #endregion
        
        #region public
        
        public void BindUI(GameObject uiCon, TemporaryQuestController tempoCon)
        {
            QuestUI = uiCon;
            _temporaryQuestController = tempoCon;
            
            ActiveQuestUI();
            
            // 기간 퀘스트 컨트롤러가 존재한다면
            if (_temporaryQuestController != null)
            {
                // 퀘스트 초기화를 진행합니다.
                _temporaryQuestController.QuestInit(_temporaryQuests.ToList(), _temporaryInstances);
            
                foreach (var kv in _temporaryInstances)
                {
                    OnTempoQuestUpdated?.Invoke(kv.Value);
                }
            }
        }

        public void UnBindUI()
        {
            _questUI.OnRewardRequest -= ReceiveReward;
            OnQuestCompleted -= _questUI.UpdateQuest;
            OnQuestUpdated -= _questUI.UpdateQuest;
            
            OnTempoQuestUpdated -= _temporaryQuestController.UpdateQuest;
            
            // 삭제
            _questUI.OnForceClear -= ForceQuestComplete;
            _questUI.OnForceInit -= ForceQuestReset;
            
            _questUI = null;
            QuestUI = null;
            _temporaryQuestController = null;
            
        }
        #endregion

        #region GeneralQuest

        /// <summary>
        /// 일반 퀘스트를 큐에 채우는 메서드입니다.
        /// 스테이지 클리어 -> 튜토리얼 -> 루틴 순으로 탐색하고 삽입합니다.
        /// </summary>
        private void GeneralQuestEnqueue()
        {
            // 일반 퀘스트 큐에 원소가 들어있다면 탈출
            if (_generalQuests.Count != 0) return;
            
            // 튜토리얼 삽입(수정)
            InsertTutorials();
            
            // 반복 퀘스트 삽입
            InsertRoutineQuest();
        }
        
        /// <summary>
        /// 스테이지 클리어 미션을 삽입하는 메서드입니다.
        /// </summary>
        private void InsertStageClearMission()
        {
            // 만약 게임의 시작이라면 return (클리어한 스테이지가 없다면)
            if (CurrentClearedStage == 0) return;
            // 스테이지 클리어 템플릿이 없다면 return
            if (_stageClearTemplate == null) return;
            
            
            // 일반 퀘스트 큐에 스테이지 클리어 미션을 삽입
            _generalQuests.Enqueue(_stageClearTemplate);
            
            // 해당 스테이지 클리어 미션 템플릿의 인스턴스를 찾을 수 없다면
            if (!_generalInstances.TryGetValue(_stageClearTemplate.questId, out var inst))
            {
                // 인스턴스를 새로 제작
                inst = new GeneralQuestInstance(_stageClearTemplate.questId, _stageClearTemplate)
                {
                    // 퀘스트의 상태는 활성화 전 상태로 초기 설정
                    QuestState = QuestState_Enum.BeforeActive,
                    // 퀘스트 번호는 지금까지 삽입한 퀘스트의 개수 + 1
                    GeneralQuestCount = computeQuestNumber(),
                    // 퀘스트 목표인 클리어해야하는 스테이지는 현재 퀘스트 스테이지 (GeneralQueueEnqueue()를 할 때마다 +1)
                    needToClearStage = CurrentQuestStage,
                    // 스테이지 미션 string 삽입
                    stageClearMission = $"스테이지 {CurrentQuestStage}를 클리어 하세요"
                };
                // 해당 미션과 인스턴스 연결
                _instances.TryAdd(_stageClearTemplate.questId, inst);
                _generalInstances.TryAdd(_stageClearTemplate.questId, inst);
            }
            // 인스턴스를 찾았다면
            else
            {
                // 해당 인스턴스를 업데이트
                // 인스턴스의 상태를 활성화 전으로 재설정
                inst.QuestState = QuestState_Enum.BeforeActive;
                // 퀘스트 번호는 지금까지 삽입한 퀘스트의 개수 + 1
                inst.GeneralQuestCount = computeQuestNumber();
                // 퀘스트 목표인 클리어해야하는 스테이지는 현재 퀘스트 스테이지 (GeneralQueueEnqueue()를 할 때마다 +1)
                inst.needToClearStage = CurrentQuestStage;
                // 인스턴스의 목표 수치를 0으로 초기화
                inst.GoalCountInit();
                // 스테이지 미션 string 삽입
                inst.stageClearMission = $"스테이지 {CurrentQuestStage}를 클리어 하세요";
            }

        }
        
        /// <summary>
        /// 일반 퀘스트를 삽입한다.
        /// </summary>
        private void InsertRoutineQuest()
        { 
            // 다음 사이클 큐에 들어있는 퀘스트들을 검사한다.
            foreach (var quest in GetNextParity())
            {
                // ※ quest의 경우 quest 번호로 정렬된 큐이므로 quest 번호 순으로 최초 삽입 스테이지가 정렬된다고 가정  
                // 추후 앞부분에 quest가 추가될 예정이라면 이부분에 변경 필요
                if (quest.RequiredQuestClearMin > CurrentQuestStage)
                {
                    return;
                }

                if (quest.GeneralType == GeneralType_Enum.StageClear)
                {
                    InsertStageClearMission();
                    continue;
                }
                
                //일반 퀘스트에 삽입
                _generalQuests.Enqueue(quest);
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
            
            // 튜토리얼이 있고 타깃 차례인 경우
            while (_tutorialQuests.Count >0 && _tutorialQuests.Peek().targetStage <= CurrentQuestStage)
            {
                // 튜토리얼 큐의 제일 앞의 questid를 검사하고
                if (_generalInstances.TryGetValue(_tutorialQuests.Peek().questId, out var inst))
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
            }
        }

        

        /// <summary>
        /// 일반 퀘스트를 작동시키는 함수
        /// </summary>
        private void EnsureGeneralActive()
        { 
            // 만일 활성화된 인스턴스가 존재한다면 탈출
            if (GetActiveGeneralInstance() is not null) return;
            
            // 일반 퀘스트 큐가 비어있다면 일반 퀘스트 큐를 채우는 것을 진행
            if (_generalQuests.Count == 0)
            {
                InsertQuest();
                // 이후 현재 퀘스트 진행 스테이지를 +1 
                CurrentQuestStage++;
            }

            while (_generalQuests.Count > 0 
                   && _clearedTutorialQuestIds.Contains(_generalQuests.Peek().questId))
                _generalQuests.Dequeue();
                
            // 일반퀘스트 큐에서 퀘스트를 꺼냄
            var next = _generalQuests.Dequeue();
            // 인자로 해당 퀘스트를 넘김
            EnsureActive(next);
        }

        private void InsertQuest()
        {
            // 일반 퀘스트 삽입
            GeneralQuestEnqueue();
            // 만약 그래도 큐가 비어있다면 큐 채우기에 문제가 발생한 것이므로 에러 발생
            if (_generalQuests.Count != 0) return;
            Debug.LogError($"현재 일반 퀘스트 큐에 퀘스트를 삽입할 수 없습니다. 퀘스트 목록을 확인해주세요");
        }
        
        /// <summary>
        /// 인자로 받은 퀘스트를 실행시킵니다. 
        /// </summary>
        private void EnsureActive(GeneralQuestDefinitionSO def)
        {
            // 현재 인스턴스가 활성화 중인 경우 탈출 
            if (GetActiveGeneralInstance() is not null) return;
            GeneralQuestInstance g;
            // 인스턴스에서 questId로 탐색 실패시 (=_instances에 def가 없는 경우)
            if (!_instances.TryGetValue(def.questId, out var inst))
            {
                QuestState_Enum state;
                if (_completedInstance != null 
                    && _completedInstance.QuestId == def.questId
                    && !isLoaded)
                    state = QuestState_Enum.Completed;
                else state = QuestState_Enum.Active;
                // 새로운 인스턴스를 생성합니다.
                g = new GeneralQuestInstance(def.questId, def)
                {
                    QuestState = state,
                    GeneralQuestCount = computeQuestNumber(),
                };
                // 인스턴스를 추가합니다.
                _instances.Add(def.questId, g);
                _generalInstances.Add(def.questId, g);

                _activeGeneralId = g.QuestId;
                if (IsStageCleared(g.Def))
                {
                    g.GoalCountAdjust(CurrentClearedStage);
                    g.QuestState = QuestState_Enum.Completed;
                    _completedInstance = g;
                }
                isLoaded = false;
            }
            // 탐색에 성공했다면
            else
            {
                if (inst is not GeneralQuestInstance value) return;
                g = value;
                
                Debug.Log($"1. {g.QuestState}");
                
                if (g.QuestState != QuestState_Enum.Active && !isLoaded)
                {
                    // 인스턴스를 활성화 상태로 변경
                    g.QuestState = QuestState_Enum.Active;
                    // 인스턴스의 카운트를 초기화
                    g.GoalCountInit();
                    _completedInstance = null;
                }

                Debug.Log($"2. {g.QuestState}");
                g.GeneralQuestCount = computeQuestNumber();
                _generalInstances.TryAdd(def.questId, g);

                _activeGeneralId = g.QuestId;
                if (IsStageCleared(g.Def))
                {
                    g.GoalCountAdjust(CurrentClearedStage);
                    g.QuestState = QuestState_Enum.Completed;
                    _completedInstance = g;
                }

                isLoaded = false;
                Debug.Log($"3. {g.QuestState}");
            }

            RefreshActiveQuestUI(def, g);
        }
        
        private bool IsStageCleared(GeneralQuestDefinitionSO def)
        {
            if (def is null) return false;

            if (def.GeneralType != GeneralType_Enum.StageClear) return false;
                
            int targetStage = CurrentQuestStage; 
            return CurrentClearedStage >= targetStage;
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
            Debug.Log($"OnSignal : {type} {signalKey} {amount}");
            if (type != QuestType_Enum.General)
            {
                OnTemporaryProgress(type, signalKey, amount);
                return;
            }
            
            // 키 값이 stageClear인 경우
            if (signalKey == QuestKeys.StageClear())
            {
                // 스테이지 클리어 처리 후 탈출
                OnStageCleared();
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
            if (IsInstanceComplete(activeGeneral))
                // ui에 클리어로 전환합니다.
                MarkCompleted(def, activeGeneral);
        }
        
        // 스테이지 클리어 시 
        private void OnStageCleared()
        {
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
        private bool IsInstanceComplete(QuestInstance inst)
        {
            return inst.IsCompleted();
        }

        /// <summary>
        /// 퀘스트를 완료 상태로 전환합니다.
        /// </summary>
        private void MarkCompleted(GeneralQuestDefinitionSO def, GeneralQuestInstance inst)
        {
            if(!IsInstanceComplete(inst)) return;

            _completedInstance = inst;

            // 퀘스트 상태를 완료상태로 전환
            inst.QuestState = QuestState_Enum.Completed;
                        
            // 퀘스트가 완료되었다는 사실을 전달합니다.
            OnQuestCompleted?.Invoke(def, inst);

            RequestSave();
        }
        

        /// <summary>
        /// 일반 퀘스트의 보상 처리가 완료되었을 때의 처리를 진행합니다.
        /// </summary>
        private void OnGeneralCompleted()
        {
            // 퀘스트 클리어 카운트를 +1합니다
            ClearedQuestCount++;
            QuestSignalManager.Instance.ETCAchieve("GeneralClear");
            
            // 다음 일반 퀘스트를 작동시킵니다.
            EnsureGeneralActive();
            // QuestUI를 새로고침합니다
            RefreshActiveQuestUI();
            RequestSave();
        }


        
        /// <summary>
        /// 활성화된 인스턴스를 받아옴
        /// </summary>
        private GeneralQuestInstance GetActiveGeneralInstance()
        {
            if (!string.IsNullOrEmpty(_activeGeneralId)
                && _instances.TryGetValue(_activeGeneralId, out var inst)
                && inst is GeneralQuestInstance { QuestState: QuestState_Enum.Active} g)
                return g;
            
            // 인스턴스에 존재하는 키밸류들을 받아온다
            foreach (var kv in _instances)
            {
                // 키에 해당하는 정의
                if (!_generalDefs.TryGetValue(kv.Key, out var def)) continue;
                
                // 만약 해당 정의가 일반 퀘스트가 아니라면 다시 반복한다 
                if (!def.isGeneral) continue;
                // 만약 해당 밸류 값의 인스턴스가 활성화 상태라면 이를 반환합니다. 
                if (kv.Value.QuestState != QuestState_Enum.Active) continue;
                var general = kv.Value as GeneralQuestInstance;
                _activeGeneralId = general?.QuestId;
                return general;
            }

            _activeGeneralId = null;
            // 탐색에 실패하면 null값을 반환합니다.
            return null;
        }
        
        /// <summary>
        /// 활성화된 인스턴스의 일반 퀘스트 정의를 반환합니다.
        /// </summary>
        private GeneralQuestDefinitionSO GetActiveQuestDefinition()
        {
            var inst = GetActiveGeneralInstance();
            if (inst == null) return null;
            return _generalDefs.TryGetValue(inst.QuestId, out var def) ? def : null;
        }
        
        /// <summary>
        /// 보상 획득을 진행합니다
        /// </summary>
        private void ReceiveReward(QuestDefinitionSO def, QuestInstance inst)
        {
            if (def is null || inst is null) return;
            if (inst.QuestState == QuestState_Enum.Received) return;
            if (!inst.IsCompleted()) return;
            // def의 보상에서 획득을 호출합니다.
            def.Reward?.AddReward(this, def, inst);

            if (inst.IsOnce())
            {
                _clearedTutorialQuestIds.Add(inst.QuestId);
            }

            if (def.isGeneral)
            {
                if (def is TutorialQuestDefinitionSO tutorialQuestDef)
                    OnTutorialQuestCompleted?.Invoke(tutorialQuestDef);
                // 일반 퀘스트의 보상처리를 마무리합니다.
                OnGeneralCompleted();
            }

            if (def.isTemporary)
                OnTemporaryCompleted(inst as TemporaryInstance);
        }

        private int computeQuestNumber()
        {
            return ClearedQuestCount + 1;
        }
        
        // 퀘스트 ID를 받으면 해당 퀘스트의 인스턴스 반환
        [CanBeNull]
        private QuestInstance GetInstance(string questId)
        {
            return _instances.TryGetValue(questId, out var inst) ? inst : null;
        }
        
        private QuestDefinitionSO GetDefinition(string questId)
        {
            return _generalDefs.TryGetValue(questId, out var def) ? def : null;
        }
    
        
        #endregion

        #region UI
        /// <summary>
        /// questUI를 작동시킵니다.
        /// 구독 타이밍 보장을 위해 따로 처리
        /// </summary>
        private void ActiveQuestUI()
        {
            var inst = GetActiveGeneralInstance();
            if (inst == null && _completedInstance != null) inst = _completedInstance;
            if (inst == null)
            {
                Debug.LogError("inst = null");
                return;
            }
            
            if (_questUI != null) return;
            if (QuestUI == null) return;
            // questUI에서 UIController를 받아옵니다
            _questUI = QuestUI.GetComponentInChildren<QuestUIController>();
            if (_questUI == null) return;
            // 퀘스트 ui의 번호를 등록합니다
            _questUI.QuestNumber = computeQuestNumber();
            
            Debug.Log($"questId : {inst.QuestId} ");
            
            // so가 비어있다면 에러 메시지 표시
            if (! _generalDefs.TryGetValue(inst.QuestId, out var so) || so == null)
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
            
            _questUI.OnRewardRequest -= ReceiveReward;
            OnQuestCompleted -= _questUI.UpdateQuest;
            OnQuestUpdated -= _questUI.UpdateQuest;
            
            _questUI.OnRewardRequest += ReceiveReward;
            OnQuestCompleted += _questUI.UpdateQuest;
            OnQuestUpdated += _questUI.UpdateQuest;
            
            // TODO : 정식 출시 시 삭제
            _questUI.OnForceClear += ForceQuestComplete;
            _questUI.OnForceInit += ForceQuestReset;

            if (_temporaryQuestController != null)
            {
                OnTempoQuestUpdated -= _temporaryQuestController.UpdateQuest;
                OnTempoQuestUpdated += _temporaryQuestController.UpdateQuest;
            }
            
            // 퀘스트 ui에 퀘스트 정보를 전달합니다
            OnQuestUpdated?.Invoke(so, inst);
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
            if (!_generalDefs.TryGetValue(inst.QuestId, out var def)) return;

            Debug.Log($"questId : {inst.QuestId} ");
            // 퀘스트UI의 퀘스트 번호를 인스턴스의 퀘스트 번호로 초기화
            _questUI.QuestNumber = inst.GeneralQuestCount;
            // 퀘스트UI에 퀘스트 정보를 업데이트
            _questUI.UpdateQuest(def, inst);
        }
        
        /// <summary>
        /// 퀘스트 ui를 새로고침 합니다
        /// </summary>
        private void RefreshActiveQuestUI(GeneralQuestDefinitionSO def, GeneralQuestInstance inst)
        {
            // questUI가 비할당된 경우 탈출
            if (_questUI == null) return;

            Debug.Log($"questId : {inst.QuestId} ");
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
            _generalDefs.Clear();
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
                _oddSequence = EnqueueOrderById(_routineSequence.GetSequence());
                _evenSequence = EnqueueOrderById(_routineSequence.GetEvenRoutine());
            }

            if (_temporaryQuest != null)
            {
                _temporaryQuests = EnqueueOrderByIdTemporary(_temporaryQuest.GetSequence());
                Debug.Log($"temporaryQuestsCount {_temporaryQuests.Count}, _dailyQuest {_dailyQuests.Count}, _weekly {_weeklyQuests.Count}");
            }
            
            
            // 스테이지 클리어 템플릿이 존재하고 이게 defs에 없다면
            if(_stageClearTemplate && !_generalDefs.ContainsKey(_stageClearTemplate.questId))
            {
                // def에 추가합니다
                _generalDefs.TryAdd(_stageClearTemplate.questId, _stageClearTemplate);
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
        private Queue<T> EnqueueOrderById<T>(T[] sequence) where T : GeneralQuestDefinitionSO
        {
            var _sequence = new List<T>();
            var _queue = new Queue<T>();
            // 입력받은 리스트를 순회
            foreach (var t in sequence)
            {
                // 딕셔너리에 삽입 시도 후 _sequence에 추가
                _generalDefs.TryAdd(t.questId, t);
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
        
        /// <summary>
        /// 배열을 받아서 questid순으로 정렬
        /// </summary>
        private Queue<TemporaryInstance> EnqueueOrderByIdTemporary(TemporaryQuestDefinitionSO[] sequence)
        {
            var _sequence = new List<TemporaryInstance>();
            var _queue = new Queue<TemporaryInstance>();
            // 입력받은 리스트를 순회
            foreach (var t in sequence)
            {
                for (int i = 0; i < t.QuestCount; i++)
                {
                    QuestType_Enum type = t.isDaily(i) ? QuestType_Enum.Daily : QuestType_Enum.Weekly;
                    TemporaryInstance inst = new TemporaryInstance(t, type, i);
                    string id;
                    if (type == QuestType_Enum.Daily)
                        id = (t.GetQuestKeyByType(type) + i).ToString();
                    else
                    {
                        id = (t.GetQuestKeyByType(type) + i - t.dailyCountArray.Length).ToString();
                    }
                    
                    _temporaryInstances.TryAdd(id, inst);
                    _instances.TryAdd(id, inst);
                    
                    _sequence.Add(inst);
                }

            }
            // 순회를 마친 후 _sequence를 questId로 정렬하고 List로 저장 
            var sorted = _sequence
                .OrderBy(t => ParseQuestOrder(t.TemporaryQuestId))
                .ToList();
            // 저장된 리스트를 순회하며 큐에 삽입
            foreach (var t in sorted)
            {
                _queue.Enqueue(t);
                                    
                if (t.QuestType == QuestType_Enum.Daily) _dailyQuests.Enqueue(t);
                else _weeklyQuests.Enqueue(t);
            }

            return _queue;
        }

        private void RequestSave(float delaySec = 0.75f)
        {
            _saveQueued = true;
            if (_saveCoroutine != null) StopCoroutine(_saveCoroutine);
            _saveCoroutine = StartCoroutine(SaveDebounce(delaySec));
        }

        private IEnumerator SaveDebounce(float delaySec)
        {
            yield return new WaitForSeconds(delaySec);
            if (_saveQueued)
            {
                _saveQueued = false;
                yield return SaveAsync().AsIEnumerator();
            }
        }

        /// <summary>
        /// 데이터를 저장하는 메서드
        /// </summary>
        private async Task SaveAsync()
        {
            if (_isSaving) return;
            _isSaving = true;
            try
            {
                // 현재의 일반 퀘스트와 기간 퀘스트 데이터를 저장
                var data = BuildQuestData();
                // 데이터베이스 매니저에 해당 데이터와 경로로 저장 대기
                await DatabaseManager.Instance.SaveQuestDataAsync(QuestDataPathV, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"퀘스트 저장 실패 {e}");
            }
            finally
            {
                _isSaving = false;
            }

        }

        private void SaveImmediate()
        {
            try
            {
                if (_isSaving || _saveQueued)
                {
                    _saveQueued = false;
                    SaveAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"즉시 저장 실패 {e}");
            }
        }

        private async Task<QuestData> LoadAsync()
        {
            try
            {
                const int maxWaitMs = 5000;
                const int pollMs = 100;
                int waited = 0;
                while (DatabaseManager.Instance == null && waited < maxWaitMs)
                {
                    await Task.Delay(pollMs);
                    waited += pollMs;
                }

                if (DatabaseManager.Instance == null)
                {
                    Debug.LogError("퀘스트 로드 실패 : DatabaseManager.Instance 미할당");
                    return null;
                }
                
                try
                {
                    await DatabaseManager.Instance.EnsureServerOffset();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"서버 오프셋 보장 실패(무시하고 진행): {e}");
                }

                const int maxRetry = 3;
                int attempt = 0;

                while (attempt < maxRetry)
                {
                    attempt++;
                    try
                    {
                        // 현재 데이터 경로에서 questData를 불러옵니다.
                        var data = await DatabaseManager.Instance.LoadQuestDatatAsync(QuestDataPathV) ?? await DatabaseManager.Instance.LoadQuestDatatAsync(QuestDataPath);

                        return data;
                    }
                    catch (NullReferenceException)
                    {
                        await Task.Delay(500);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"퀘스트 로드 실패 {e}");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"퀘스트 로드 실패 : {e}");
                return null;
            }

            return null;
        }

        /// <summary>
        /// 현재의 Quest 진행도를 바탕으로 QuestData를 제작합니다
        /// </summary>
        private QuestData BuildQuestData()
        {
            var generalInst = GetActiveGeneralInstance() ?? _completedInstance;

            // 현재 진행 중인 퀘스트 내용을 바탕으로 GeneralQuestData을 제작합니다.
            var general = new GeneralQuestData
            {
                ActiveQuestId = generalInst?.QuestId ?? string.Empty,
                ActiveState = generalInst != null ? (int)generalInst.QuestState : 0,
                ActiveProgress = generalInst?.CurrentGoalCount ?? 0,
                ClearedQuestCount = ClearedQuestCount,
                CurrentQuestStage = CurrentQuestStage,
            };
            Debug.Log($"SaveGeneralId : {general.ActiveQuestId} ");
            
            var temporary = new Dictionary<string, TemporaryQuestData>(); 
            // 기간 퀘스트를 순회하며 각 퀘스트 들의 내용을 저장합니다.
            foreach (var kv in _temporaryInstances)
            {
                var inst = kv.Value;
                temporary.Add(inst.QuestId, new TemporaryQuestData
                {
                    State = (int)inst.QuestState,
                    QuestType = (int)inst.QuestType,
                    Progress = inst.CurrentGoalCount,
                });
            }

            var tutorial = new TutorialQuestData()
            {
                ClearedTutorialQuestIds = _clearedTutorialQuestIds
            };

            // QuestData를 반환합니다.
            return new QuestData
            {
                General = general,
                Temporary = temporary,
                Tutorial = tutorial,
            };
        }

        /// <summary>
        /// QuestData를 바탕으로 퀘스트 진척도를 복원합니다.
        /// </summary>
        private void RestoreFromQuestData(QuestData data)
        {
            // General QuestData가 존재한다면
            if (data.General != null)
            { 
                ClearedQuestCount = data.General.ClearedQuestCount;
                CurrentQuestStage = data.General.CurrentQuestStage;
                if (!string.IsNullOrEmpty(data.General.ActiveQuestId) &&
                    _generalDefs.TryGetValue(data.General.ActiveQuestId, out var def))
                {
                    if (!_generalInstances.TryGetValue(def.questId, out var inst))
                    {
                        inst = new GeneralQuestInstance(def.questId, def);
                        _instances.Add(def.questId, inst);
                        _generalInstances.Add(def.questId, inst);
                    }


                    inst.QuestState = (QuestState_Enum)data.General.ActiveState;
                    inst.GoalCountAdjust(Mathf.Max(0, data.General.ActiveProgress));
                    inst.GeneralQuestCount = computeQuestNumber();
                    
                    if (inst.QuestState == QuestState_Enum.Active)
                        _activeGeneralId = inst.QuestId;
                    if (inst.QuestState == QuestState_Enum.Completed)
                        _completedInstance = inst;
                    
                    Debug.Log($"activeId : {_activeGeneralId}");
                }
            }

            // 기간 퀘스트 정보가 존재한다면
            if (data.Temporary != null)
            {
                foreach (var t in data.Temporary)
                {
                    if (string.IsNullOrEmpty(t.Key)) continue;
                    if (!_temporaryInstances.TryGetValue(t.Key, out var def)) continue;
                    
                    def.QuestState = (QuestState_Enum)t.Value.State;
                    def.QuestType = (QuestType_Enum)t.Value.QuestType;
                    def.GoalCountAdjust(Mathf.Max(0, t.Value.Progress));
                }
            }

            if (data.Tutorial != null)
            {
                _clearedTutorialQuestIds = data.Tutorial.ClearedTutorialQuestIds;
            }
        }

        private void RebuildGeneralQuest(QuestData data)
        {
            _generalQuests.Clear();

            var activeId = data?.General?.ActiveQuestId ?? string.Empty;
            var isEnqueued = TryEnqueueActiveTutorial(activeId);
            InsertTutorials();
            if (!isEnqueued)
            {
                var parity = GetNextParity().ToList();
                if (parity.Count > 0)
                {
                    bool found = false;
                    foreach (var quest in parity)
                    {
                        if (!found && quest.questId == activeId) found = true;
                        if (found && quest.GeneralType == GeneralType_Enum.StageClear)
                        {
                            InsertStageClearMission();
                            continue;
                        }
                        if (found)
                        {
                            _generalQuests.Enqueue(quest);
                        }
                    }
                }
            }
            else
            {
                InsertRoutineQuest();
            }
        }


        private bool TryEnqueueActiveTutorial(string activeId)
        {
            if (_tutorialQuests == null) return false;
            if (_tutorialQuests.Count == 0) return false;
            
            var snapshot = _tutorialQuests.ToArray();
            var index = Array.FindIndex(snapshot, tutorial => tutorial.questId == activeId);
            if (index < 0) return false;

            for (int i = 0; i < index && _tutorialQuests.Count > 0; i++)
            {
                var tutorial = _tutorialQuests.Dequeue();
                QuestSignalManager.Instance.UnSubscribeTutorial(tutorial);
            }


            if (_tutorialQuests.Count > 0)
            {
                var activeTutorial = _tutorialQuests.Dequeue();
                _generalQuests.Enqueue(activeTutorial);
                return true;
            }
            return false;
        }
        #endregion

        #region IRewardGiver
        public void Give(QuestRewardSO.RewardEntry entry)
        {
            OnRewardGranted?.Invoke(entry);
            // TODO : 아직 재화 구조가 확정되지 않았고 작동 확인은 진행했으므로 우선 주석 전환
            // 추후 팝업 메시지 창과 연결 가능성
            // DatabaseManager.Instance.AddCurrency(entry.RewardType, entry.FixedAmount);
        }
        #endregion

        #region TemporaryQuest

        private void RegisterTemporaryQuests()
        {
            if (_temporaryQuest == null) return;
            if (_temporaryInstances != null && _temporaryInstances.Count > 0) return;
            RegisterTemporary(_temporaryQuest.GetSequence());
        }

        private void RegisterTemporary(IEnumerable<TemporaryQuestDefinitionSO> quests)
        {
            if (quests == null) return;
            foreach (var quest in quests)
            {
                if (quest == null) continue;
                GetTemporaryInstance(quest);
            }
        }

        private async Task CheckTemporaryQuestsReset(bool daily, bool weekly)
        {
            if(_isResetting) return;
            _isResetting = true;

            try
            {
                if (daily)
                {
                    var needDailyReset = await DatabaseManager.Instance.DailyCheckIn();
                    if (needDailyReset)
                    {
                        ResetTemporaryQuests(_dailyQuests);
                        await DatabaseManager.Instance.SetDailyQuestTime();
                    }
                }
                else return;

                if (weekly)
                {
                    var needWeeklyReset = await DatabaseManager.Instance.WeeklyCheckIn();
                    if (needWeeklyReset)
                    {
                        ResetTemporaryQuests(_weeklyQuests);
                        await DatabaseManager.Instance.SetWeeklyQuestTime();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"기간제 퀘스트 초기화 오류: {e}");
            }
            finally
            {
                _isResetting = false;
            }
        }

        private void ScheduleNextResetTick(int resetHour = 6, DayOfWeek resetDay = DayOfWeek.Monday)
        {
            if (_nextResetCoroutine != null)
                StopCoroutine(_nextResetCoroutine);
            _nextResetCoroutine = StartCoroutine(WaitUntilNextBoundary(resetHour, resetDay));
        }

        private IEnumerator WaitUntilNextBoundary(int resetHour, DayOfWeek resetDay)
        {
            var approxMs = DatabaseManager.Instance.GetApproxServerTime();
            if (approxMs == 0)
            {
                yield return new WaitForSeconds(5f);
                approxMs = DatabaseManager.Instance.GetApproxServerTime();
            }
            
            var nowStd = approxMs == 0
                ? DateTime.UtcNow.AddHours(9)
                : DateTimeOffset.FromUnixTimeMilliseconds(approxMs).UtcDateTime.AddHours(9);
            
            DateTime Today = new DateTime(nowStd.Year, nowStd.Month, nowStd.Day, resetHour, 0, 0);
            
            var nextDaily = (nowStd >= Today) ? Today.AddDays(1) : Today;
            
            int dayUntilReset = ((int)resetDay - (int)nowStd.DayOfWeek + 7) % 7;
            if (dayUntilReset == 0 && nowStd >= Today) dayUntilReset = 7;
            
            var nextWeekly = Today.AddDays(dayUntilReset);

            var nextBoundary = nextDaily < nextWeekly ? nextDaily : nextWeekly;
            var WaitSec = (nextBoundary - nowStd).TotalSeconds;
            if (WaitSec < 1) WaitSec = 1;
            
            yield return new WaitForSeconds((float)WaitSec + 1f);

            yield return DatabaseManager.Instance.EnsureServerOffset().AsIEnumerator();
            var daily = DatabaseManager.Instance.QuickDailyCheck();
            var weekly = DatabaseManager.Instance.QuickWeeklyCheck();
            yield return CheckTemporaryQuestsReset(daily, weekly).AsIEnumerator();
            
            ScheduleNextResetTick(resetHour, resetDay);
        }


        private void ResetTemporaryQuests(IEnumerable<TemporaryInstance> quests)
        {
            if(quests == null) return;
            
            foreach (var quest in quests)
            {
                if (quest == null) continue;

                quest.QuestState = QuestState_Enum.Active;
                quest.GoalCountInit();
                OnTempoQuestUpdated?.Invoke(quest);
            }
            RequestSave();
        }


        private TemporaryInstance[] GetTemporaryInstance(TemporaryQuestDefinitionSO def)
        {
            var instCount = def.QuestCount;
            var insts = new TemporaryInstance[instCount];
            
            for (int i = 0; i < instCount; i++)
            {
                var id = (def.GetQuestKeyByType(def.isDaily(i) ? QuestType_Enum.Daily : QuestType_Enum.Weekly) + i).ToString();
                if (!_temporaryInstances.TryGetValue(id, out var inst))
                {  
                    QuestType_Enum type = (i < def.dailyCountArray.Length) ? QuestType_Enum.Daily : QuestType_Enum.Weekly;

                    inst = new TemporaryInstance(def, type, i);
                    
                    _instances.Add(id, inst);
                    _temporaryInstances.Add(id, inst);
                }
                insts[i] = inst;
            }
            return insts;
        }

        private void OnSignalComplete(string signalName)
        {
            var inst = GetInstance(signalName);
            if (inst == null) return;
            
            var def = GetDefinition(signalName);
            if (def is null && inst is TemporaryInstance t)
                def = t.Def;
            
            ReceiveReward(def, inst);
        }

        private void OnTemporaryCompleted(TemporaryInstance inst)
        {
            if (inst == null) return;
            inst.QuestState = QuestState_Enum.Received;
            _temporaryQuestController?.UpdateQuest(inst);
            RequestSave();
        }

        private void OnTemporaryProgress(QuestType_Enum type, string signalKey, int progress)
        {
            var quests = type == QuestType_Enum.Daily ? _dailyQuests : _weeklyQuests;
            foreach (var inst in quests)
            {
                if (inst is null) continue;
                if (inst.QuestState == QuestState_Enum.Received) continue;
                if (type != inst.QuestType) continue;
                if (inst.Def?.TempoGoal is null) continue;
                
                var delta = inst.Def.TempoGoal.ProgressDeltaFrom(signalKey, progress);
                // 만약 진척도가 <=0이라면; 키가 일치하지 않는다면
                if (delta <= 0) continue;
                
                var before = inst.CurrentGoalCount;
                var require = inst.DemandedGoalCount;
                inst.GoalCountAdjust(Mathf.Clamp(before + delta, 0, require));
                
                // 만약 갱신한 인스턴스의 완료 조건이 충족됬을 경우
                if(IsTemporaryInstanceComplete(inst)) inst.QuestState = QuestState_Enum.Completed;
                    
                // 퀘스트 사양이 바뀌었으므로 업데이트 이벤트를 호출한다 
                OnTempoQuestUpdated?.Invoke(inst);
            }
            
            RequestSave();
        }
        
        private bool IsTemporaryInstanceComplete(TemporaryInstance inst)
        {
            return inst.IsCompleted();
        }
        
        #endregion
        
        #region editor-only
        // 클리어 버튼 / 정식 빌드에선 빠질 예정
        private void ForceQuestComplete()
        {
            if (_isResetting) return;
            GeneralQuestInstance inst = GetActiveGeneralInstance();
            inst.ForceComplete();   
            MarkCompleted(GetActiveQuestDefinition(),inst);
        }

        private void ForceQuestReset()
        {
            if (_isResetting) return;
            _isResetting = true;

            try
            {
                CurrentQuestStage = 1;
                ClearedQuestCount = 0;

                _activeGeneralId = null;

                _tutorialQuests ??= new Queue<TutorialQuestDefinitionSO>();
                _generalQuests ??= new Queue<GeneralQuestDefinitionSO>();
                _oddSequence ??= new Queue<RoutineQuestDefinitionSO>();
                _evenSequence ??= new Queue<RoutineQuestDefinitionSO>();

                _instances ??= new Dictionary<string, QuestInstance>();
                _generalInstances ??= new Dictionary<string, GeneralQuestInstance>();
                _generalDefs ??= new Dictionary<string, GeneralQuestDefinitionSO>();
                _clearedTutorialQuestIds ??= new List<string>();

                _temporaryInstances ??= new Dictionary<string, TemporaryInstance>();
                _temporaryQuests ??= new Queue<TemporaryInstance>();
                _dailyQuests ??= new Queue<TemporaryInstance>();
                _weeklyQuests ??= new Queue<TemporaryInstance>();

                _tutorialQuests.Clear();
                _generalQuests.Clear();
                _oddSequence.Clear();
                _evenSequence.Clear();

                _instances.Clear();
                _generalInstances.Clear();
                _generalDefs.Clear();
                _clearedTutorialQuestIds.Clear();

                _temporaryInstances.Clear();
                _temporaryQuests.Clear();
                _dailyQuests.Clear();
                _weeklyQuests.Clear();

                BuildCatalog();

                GeneralQuestEnqueue();
                EnsureGeneralActive();

                if (_temporaryQuestController != null)
                {
                    _temporaryQuestController.QuestInit(_temporaryQuests.ToList(), _temporaryInstances);
                }

                foreach (var kv in _temporaryInstances)
                {
                    var t = kv.Value;
                    if (t == null) continue;
                    t.QuestState = QuestState_Enum.Active;
                    t.GoalCountInit();
                    OnTempoQuestUpdated?.Invoke(t);
                }
                
                ActiveQuestUI();
                RequestSave(0.1f);
            }
            finally
            {
                _isResetting = false;
            }
        }

        #endregion
    }

    internal static class TaskHelper
    {
        public static IEnumerator AsIEnumerator(this Task task)
        {
            while (!task.IsCompleted) yield return null;
        }
    }
}
