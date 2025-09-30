using System;
using System.Text;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Codex
{
    public class CodexInstance
    {
        public int CurrentProgress
        {
            get => _currentProgress;
            private set
            {
                _currentProgress = value;
                OnProgressChanged?.Invoke();
            }   
        }
        public int Index { get; }

        public int MaxProgress { get; }

        public bool IsReceived
        {
            get => isReceived;
            
            private set
            {
                isReceived = value;
                OnStateChanged?.Invoke(this);
            }
        }
        public bool IsCleared => isCleared;

        private int _currentProgress;
        private bool isCleared;
        private bool isReceived;

        public readonly Faction CodexFaction;
        public readonly CodexStd_Enum CodexStd;
        public readonly CodexRewardSO RewardSO;
        public readonly int StatAmount;
        public readonly CodexStat StatType;
        
        // private readonly StringBuilder _sb = new();
        // private readonly string _FirstContents = "보유중인 ";
        // private readonly string _middleContents = " 크루의 총 ";
        // private readonly string _middle2 = "의 합이 ";
        // private readonly string _wordEnd = "을 달성하자";
        // public readonly string ProgressText;
        
        public event Action OnProgressChanged;
        public event Action<CodexInstance> OnStateChanged;

        public CodexInstance(CodexList list, int value, int idx, CodexRewardSO reward, int cleardCount)
        {
            OnProgressChanged += CheckClear;
            CurrentProgress = value;
            Index = idx;
            CodexFaction = list.faction;
            MaxProgress = (idx + 1) * list._codexDistance;
            StatAmount = list._codexIncrease;
            CodexStd = list._codexStd;
            StatType = list._codexStat;
            RewardSO = reward;
            CheckClear();
            isReceived = idx < cleardCount;
            
            // string a = CodexStd == CodexStd_Enum.Level? "레벨" :  "등급";
            // ProgressText = _sb.Append(_FirstContents)
            //     .Append(CodexFaction)
            //     .Append(_middleContents)
            //     .Append(a)
            //     .Append(_middle2)
            //     .Append(MaxProgress)
            //     .Append(_wordEnd)
            //     .ToString();
            // // 보유중인 "팩션" 크루의 총 "타입"의 합을 "목표"을 달성하자
        }

        public bool AddProgress(int value, int levelSum)
        {
            if (levelSum != _currentProgress)
            {
                Debug.LogWarning($"레벨 합 이상 : 기존 인스턴스 내부 {_currentProgress}, 데이터 상 레벨합 {levelSum}");
                return false;
            }
            _currentProgress += value;
            return true;
        }

        public void CheckClear()
        {
            isCleared = _currentProgress >= MaxProgress;
        }

        internal void CodexReceived()
        {
            IsReceived = true;
        }
    }
}