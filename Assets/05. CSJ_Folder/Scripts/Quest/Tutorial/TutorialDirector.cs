using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.SO.Tutorial;
using _05._CSJ_Folder.Scripts.Quest.UI;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class TutorialDirector : MonoBehaviour
    {
        [Header( "UI")]
        [SerializeField] private TutorialUIController tutorialUI;


        [Header("첫 시작 튜토리얼 아크")]
        [SerializeField] private TutorialArcSO defaultArc;
        [SerializeField] private TutorialArcSO[] allArcs;
        
        [Header("퀘스트 트리거 매핑")]
        [SerializeField] private TutorialQuestLink[] questLinks;

        [Header("동작 옵션")] [SerializeField] private bool playDefaultOnFirst = true;

        public string _currentArcId { get; private set; } = "";
        public int _currentStepIndex { get; private set; } = 0;
        public bool IsFinished { get; private set; } = false;

        public event Action OnStepChanged;
        
        private readonly Queue<(TutorialArcSO arc, int index)> _arcQueue = new();
        private TutorialArcSO _currentArc;
        private bool _isPlaying;
        private bool _primed;
        private bool _questHooksBound;

        public void PrimeFromSave(string arcId, int stepIndex, bool finished)
        {
            _currentArcId = arcId ?? "";
            _currentStepIndex = Mathf.Max(0, stepIndex);
            IsFinished = finished;
            _primed = true;
        }

        public void Init()
        {

            if (IsFinished) return;

            if (!string.IsNullOrEmpty(_currentArcId))
            {
                var arc = FindArcById(_currentArcId);
                if (arc is not null)
                {
                    StartArc(arc, _currentStepIndex);
                    return;
                }

                _currentArcId = "";
                _currentStepIndex = 0;
            }

            if (playDefaultOnFirst && defaultArc is not null)
            {
                StartArc(defaultArc, 0);
                return;
            }

            TryPlayPending();
        }

        public void StartArcById(string arcId)
        {
            var arc = FindArcById(arcId);
            if (arc is null) return;
            StartArc(arc, 0);
        }
        
        // public void BindQuestHooks()
        // {
        //     if (_questHooksBound) return;
        //     _questHooksBound = true;
        //
        //     if (questLinks is null || questLinks.Length == 0) return;
        //
        //     QuestManager.Instance.OnQuestUpdated += HandleQuestUpdated;
        // }
        //
        // public void UnBindQuestHooks()
        // {
        //     if (!_questHooksBound) return;
        //     _questHooksBound = false;
        //
        //     QuestManager.Instance.OnQuestUpdated -= HandleQuestUpdated;
        // }

        public void HandleQuestActive(TutorialQuestDefinitionSO def, GeneralQuestInstance inst)
        {
            if (def is null || inst is null) return;
            
            TryTriggerByQuest(def, inst);
        }
        
        private void TryTriggerByQuest(TutorialQuestDefinitionSO def, GeneralQuestInstance inst)
        {
            if (questLinks is null || questLinks.Length == 0) return;

            foreach (var link in questLinks)
            {
                if (link?.arc is null) continue;
                if (!string.Equals(link.quest.questId, def.questId, StringComparison.Ordinal)) continue;

                EnqueueOrStart(link.arc, 0);
                return;
            }
        }

        private void StartArc(TutorialArcSO arc, int stepIndex)
        {
            if (arc is null) return;

            if (_isPlaying)
            {
                _arcQueue.Enqueue((arc, stepIndex));
                return;   
            }

            _currentArc = arc;
            _isPlaying = true;
            
            _currentArcId = arc.arcId;
            _currentStepIndex = Mathf.Max(0, stepIndex);
            IsFinished = false;

            if (tutorialUI is not null)
            {
                tutorialUI.StartSequence(
                    arc.Sequence,
                    _currentStepIndex,
                    OnStepChanged: idx =>
                    {
                        _currentStepIndex = Mathf.Max(0, idx);
                        OnStepChanged?.Invoke();
                    },
                    onCompleted: OnArcCompleted);
            }
            else
            {
                OnArcCompleted();
            }

            OnStepChanged?.Invoke();
        }

        private void OnArcCompleted()
        {
            if (_currentArc is not null && _currentArc.NextArc is not null)
            {
                var next = _currentArc.NextArc;
                _currentArc = null;
                _isPlaying = false;
                StartArc(next, 0);
                return;
            }

            _currentArc = null;
            _isPlaying = false;

            _currentArcId = "";
            _currentStepIndex = 0;
            IsFinished = true;
            OnStepChanged?.Invoke();

            tutorialUI.Hide();
            OnStepChanged?.Invoke();

            TryPlayPending();
        }
        
        private void EnqueueOrStart(TutorialArcSO arc, int stepIndex)
        {
            if (!_isPlaying) StartArc(arc, stepIndex);
            else _arcQueue.Enqueue((arc, stepIndex));
        }
        
        private void TryPlayPending()
        {
            if (_isPlaying) return;
            if (_arcQueue.Count == 0) return;
            
            var (arc, stepIndex) = _arcQueue.Dequeue();
            StartArc(arc, stepIndex);
        }
        
        private TutorialArcSO FindArcById(string id)
        => allArcs?.FirstOrDefault(a => a is not null && a.arcId.Equals(id, StringComparison.Ordinal));
    }
}