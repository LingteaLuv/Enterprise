using System;
using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Codex.UI;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Codex
{
    public class CodexManager : Singleton<CodexManager>, ICodexRewardGiver
    {
        [SerializeField] private CodexSignalSO _codexSignal;
        [SerializeField] private List<CodexList> _codexLists;
        
        
        private Dictionary<(Faction, CodexStd_Enum),List<CodexInstance>> _instMap;
        private Dictionary<(Faction, CodexStd_Enum), Dictionary<int, bool>> _receiveState;
        
        public Dictionary<Faction, CodexData> FactionData;
        private CodexUIController _codexUIController;


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

        private void SaveData()
        {
            
        }
        private void LoadData()
        {
             
        }
        
        private void Awake()
        {
            _instMap = new Dictionary<(Faction,CodexStd_Enum),List<CodexInstance>>();
            _receiveState = new Dictionary<(Faction, CodexStd_Enum), Dictionary<int, bool>>();
            
            FactionData = new Dictionary<Faction, CodexData>();
            
            CodexInit();

            // TODO : DB 연동 방식에 따라 조절, codexData는 앞에서 받아야할 수도?
            LoadData();

            _codexSignal.OnSignal -= OnSignal;
            _codexSignal.OnSignalComplete -= ReciveReward;
            
            _codexSignal.OnSignal += OnSignal;
            _codexSignal.OnSignalComplete += ReciveReward;
            
            

        }

        private void CodexInit()
        {
            var factions = (Faction[])Enum.GetValues(typeof(Faction));
            var stds = (CodexStd_Enum[])Enum.GetValues(typeof(CodexStd_Enum));
            Debug.Log(factions.Length);
            foreach (var faction in factions)
            {
                if (!FactionData.TryGetValue(faction, out var codexData))
                {
                    var _codexData = new CodexData()
                    {
                        Faction = faction,
                        LevelSum = 0,
                        RankSum = 0,
                    };
                    FactionData.TryAdd(faction, _codexData);
                }

                foreach (var std in stds)
                {
                    var list = _codexLists.Find(x => x.faction == faction && x._codexStd == std);
                    if (list == null) continue;

                    var _codexValue = GetCurrentValue(codexData, std);
                    
                    var StateMap = new Dictionary<int, bool>();
                    var codexList = new List<CodexInstance>();
                    var specialIndex = 0;

                    for (int i = 0; i < list._lastCodexIndex; i++)
                    {
                        CodexInstance inst;
                        if (i == 0 || list._specialQuestDistance == 0 || list._specialQuestDistance % (i + 1) != 0)
                        {
                            inst = new CodexInstance(
                                list._generalCodexReward, 
                                _codexValue, 
                                i, 
                                list._codexDistance,
                                list._codexIncrease,
                                list._codexStd,
                                list._codexStat,
                                list.faction);
                        }
                        else
                        {
                            inst = new CodexInstance(
                                list._specialCodexRewards[specialIndex++],
                                _codexValue,
                                i,
                                list._codexDistance,
                                list._codexIncrease,
                                list._codexStd,
                                list._codexStat,
                                list.faction);
                            if (specialIndex == list._specialCodexRewards.Length) specialIndex = 0;
                        }
                        codexList.Add(inst);
                        StateMap.TryAdd(i, false);
                        inst.OnStateChanged += UpdateState;
                    }
                    Debug.Log($"{faction}, {std}, {codexList.Count}");
                    _instMap.TryAdd((faction, std), codexList);
                    _receiveState.TryAdd((faction, std), StateMap);
                }
            }
        }

        private int GetCurrentValue(CodexData codexData, CodexStd_Enum std)
        {
            return std == CodexStd_Enum.Level ? codexData.LevelSum : codexData.RankSum;
        }

        private void OnSignal(Faction faction, CodexStd_Enum std, int value)
        {
            if (!FactionData.TryGetValue(faction, out var _codexData)) return;
            if (!_instMap.TryGetValue((faction,std), out var codexList)) return;
            
            var _codexValue = GetCurrentValue(_codexData, std);


            bool allSuccessed = true;
            foreach (var codexInstance in codexList)
            {
                if (!codexInstance.AddProgress(value, _codexValue)) allSuccessed = false;
                UpdateState(codexInstance);
            }

            if (!allSuccessed) return;
            _codexData.AdjustValueSum(std, value, true); 
        }

        private void ReciveReward(CodexInstance inst)
        {
            inst.RewardSO?.AddReward(this);

            if (FactionData.TryGetValue(inst.CodexFaction, out var codexData))
            {
                codexData.ClearCodex(inst.StatAmount, inst.CodexStd);
            }
            
            inst.CodexReceived();
            
            _codexUIController.UpdateCodex(inst);
        }

        public void Give(CodexRewardSO.CodexRewardEntry entry)
        {
            // TODO : 아직 재화 구조가 확정되지 않았고 작동 확인은 진행했으므로 우선 주석 전환
            // 추후 팝업 메시지 창과 연결 가능성
            // DatabaseManager.Instance.AddCurrency(entry.RewardType, entry.amount);
        }

        private void UpdateState(CodexInstance inst)
        {
            var key = (inst.CodexFaction, inst.CodexStd);

            if(!_receiveState.TryGetValue(key, out var dic)) return;
            
            var index = inst.Index;
            
            if (dic.Count > index)
                dic[index] = inst.IsReceived;
            
            _codexUIController.UpdateCodex(inst);
        }
    }
}