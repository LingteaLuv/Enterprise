using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _05._CSJ_Folder.Scripts.Codex.UI;
using _05._CSJ_Folder.Scripts.Quest;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Codex
{
    public class CodexManager : Singleton<CodexManager>, ICodexRewardGiver
    {
        [SerializeField] private CodexSignalSO _codexSignal;
        [SerializeField] private List<CodexList> _codexLists;
        
        
        private Dictionary<(Faction, CodexStd_Enum),List<CodexInstance>> _instMap;
        private Dictionary<(Faction, CodexStd_Enum), Dictionary<int, bool>> _receiveState;
        
        private Dictionary<Faction, CodexData> FactionData;
        private CodexUIController _codexUIController;

        private readonly string CodexDataPath = "CodexData";
        private static Coroutine _saveCoroutine;


        public void BindUI(CodexUIController uiController)
        {
            _codexUIController = uiController;
            
            if (_codexUIController is null) return;
            
            _codexUIController.CodexListInit(_instMap);
        }

        public void UnBindUI()
        {
            _codexUIController = null;
        }
        
        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(ReadyForCodex());
        }

        private IEnumerator ReadyForCodex()
        {
            _instMap = new Dictionary<(Faction, CodexStd_Enum), List<CodexInstance>>();
            _receiveState = new Dictionary<(Faction, CodexStd_Enum), Dictionary<int, bool>>();
            FactionData = new Dictionary<Faction, CodexData>();
            
            var loadTask = LoadData();                
            yield return loadTask.AsIEnumerator();    
            
            CodexInit();
        }

        protected void OnEnable()
        {
            if (_codexSignal == null) return;
            _codexSignal.OnSignal         -= OnSignal;
            _codexSignal.OnSignalComplete -= ReciveReward;
            _codexSignal.OnSignal         += OnSignal;
            _codexSignal.OnSignalComplete += ReciveReward;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _codexSignal.OnSignal -= OnSignal;
            _codexSignal.OnSignalComplete -= ReciveReward;
            SaveImmediate();
        }

        protected new void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            SaveImmediate();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if(pauseStatus)
            {
                RequestSave(0.1f);
            }
        }
        
        private void RequestSave(float delaySec = 0.75f)
        {
            CodexDataManager._saveQueued = true;
            if (_saveCoroutine != null) StopCoroutine(_saveCoroutine);
            _saveCoroutine = StartCoroutine(
                CodexDataManager.SaveDebounce(CodexDataPath,FactionData.Values.ToList(), delaySec));
        }

        private void SaveImmediate()
        {
            CodexDataManager.TrySave();
        }
        
        private async Task LoadData()
        { 
            FactionData = await DatabaseManager.Instance.LoadCodexDataAsync(CodexDataPath);
        }

        private void CodexInit()
        {
            var factions = (Faction[])Enum.GetValues(typeof(Faction));
            var stds = (CodexStd_Enum[])Enum.GetValues(typeof(CodexStd_Enum));
            foreach (var faction in factions)
            {
                if (!FactionData.TryGetValue(faction, out var codexData))
                {
                    var _codexData = new CodexData()
                    {
                        Faction = faction,
                        LevelSum = 0,
                        RankSum = 0,
                        
                        ClearedLevelQuestCount = 0,
                        ClearedRankQuestCount = 0,
                    };
                    FactionData.TryAdd(faction, _codexData);
                    codexData = _codexData;
                }

                foreach (var std in stds)
                {
                    var list = _codexLists.Find(x => x.faction == faction && x._codexStd == std);
                    if (list == null) continue;

                    var _codexValue = GetCurrentValue(faction, std);
                    
                    var StateMap = new Dictionary<int, bool>();
                    var codexList = new List<CodexInstance>();
                    var specialIndex = 0;
                    
                    for (int i = 0; i < list._lastCodexIndex; i++)
                    {
                        CodexInstance inst;
                        if (list._specialQuestDistance == 0 ||  (i + 1) % list._specialQuestDistance != 0)
                        {
                            inst = new CodexInstance(
                                list, 
                                _codexValue, 
                                i,
                                list._generalCodexReward,
                                std == CodexStd_Enum.Level ? 
                                    codexData.ClearedLevelQuestCount : codexData.ClearedRankQuestCount);
                        }
                        else
                        {

                            inst = new CodexInstance(
                                list,
                                _codexValue,
                                i, 
                                list._specialCodexRewards[specialIndex++],
                                std == CodexStd_Enum.Level ? 
                                    codexData.ClearedLevelQuestCount : codexData.ClearedRankQuestCount);
                            if (specialIndex == list._specialCodexRewards.Length) specialIndex = 0;
                        }
                        codexList.Add(inst);
                        StateMap.TryAdd(i, false);
                        inst.OnStateChanged += UpdateState;
                        bool isClearedIndex =
                            (std == CodexStd_Enum.Level && i < codexData.ClearedLevelQuestCount) ||
                            (std == CodexStd_Enum.Rank  && i < codexData.ClearedRankQuestCount);

                        if (isClearedIndex)
                        {
                            codexData.AchieveCodex(inst, true);
                        }
                    }
                    _instMap.TryAdd((faction, std), codexList);
                    _receiveState.TryAdd((faction, std), StateMap);
                }
            }
        }

        public int GetCurrentValue(Faction faction, CodexStd_Enum std)
        {
            var codexData = FactionData[faction];
            return std == CodexStd_Enum.Level ? codexData.LevelSum : codexData.RankSum;
        }
        
        public int[] GetNextValue(Faction faction, CodexStd_Enum std)
        {
            foreach (var inst in _instMap[(faction, std)])
            {
                inst.CheckClear();
                if (!inst.IsCleared) return new[] {inst.MaxProgress, inst.Index, inst.Index+1};
            }
            return new[] {_instMap[(faction, std)].Last().MaxProgress, _instMap[(faction, std)].Last().Index, _instMap[(faction, std)].Last().Index};
        }

        private void OnSignal(Faction faction, CodexStd_Enum std, int value)
        {
            if (!FactionData.TryGetValue(faction, out var _codexData)) return;
            if (!_instMap.TryGetValue((faction,std), out var codexList)) return;
            
            var _codexValue = GetCurrentValue(faction, std);
            
            bool allSuccessed = true;
            foreach (var codexInstance in codexList)
            {
                if (!codexInstance.AddProgress(value, _codexValue)) allSuccessed = false;
                UpdateState(codexInstance);
            }

            if (!allSuccessed) return;
            _codexData.AdjustValueSum(std, value, true); 
            FactionData[faction] = _codexData;
        }

        private void ReciveReward(CodexInstance inst)
        {
            inst.RewardSO?.AddReward(this);

            if (FactionData.TryGetValue(inst.CodexFaction, out var codexData))
            {
                codexData.AchieveCodex(inst, false);
            }
            
            inst.CodexReceived();
            
            _codexUIController.UpdateCodex(inst);
            
            QuestSignalManager.Instance.ETCAchieve("CodexRegister", 1);
            
            RequestSave();
        }

        public void Give(CodexRewardSO.CodexRewardEntry entry)
        {
            // TODO : 아직 재화 구조가 확정되지 않았고 작동 확인은 진행했으므로 우선 주석 전환
            // 추후 팝업 메시지 창과 연결 가능성
            // DatabaseManager.Instance.AddCurrency(entry.RewardType, entry.amount);
        }

        public int GetCodexStat(Stat stat, Faction faction)
        {
            int statAmount;
            switch (stat)
            {
                case Stat.CritChance:
                    statAmount = FactionData[faction].CritChance;
                    break;
                case Stat.CritDamage:
                    statAmount = FactionData[faction].CritDamage;
                    break;
                default:
                    statAmount = 0;
                    break;
            }
            return statAmount;
        }

        private void UpdateState(CodexInstance inst)
        {
            var key = (inst.CodexFaction, inst.CodexStd);

            if(!_receiveState.TryGetValue(key, out var dic)) return;
            
            var index = inst.Index;
            
            if (dic.Count > index)
                dic[index] = inst.IsReceived;
            
            _codexUIController.UpdateCodex(inst);
            RequestSave();
        }
    }
}