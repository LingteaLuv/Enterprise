using System;
using System.Collections;
using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.SO.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;

namespace _05._CSJ_Folder.Scripts.Quest.UI
{
    public class TutorialUIController : Singleton<TutorialUIController>
    {
        [SerializeField] private Image overlay;
        [SerializeField] private TutorialHighlighterV2 highlighter;
        [SerializeField] private TutorialClickForwarder forwarder;
        [SerializeField] private TutorialDialogView dialog;
        [SerializeField] private RewardPanel RewardPanel;
        [SerializeField] private GameObject _rewardPanelRoot;

        public TutorialStepSO step;

        public Coroutine running;


        public void StartSequence(TutorialArcSO arc,
            int startIndex = 0,
            Action<int> OnStepChanged = null,
            Action onCompleted = null)
        {
            StopIfRunning();
            running = StartCoroutine(Run(arc, startIndex, OnStepChanged, onCompleted));
        }

        public void Hide()
        {
            StopIfRunning();
            overlay.raycastTarget = false;
            highlighter.gameObject.SetActive(false);
            forwarder.gameObject.SetActive(false);
            overlay.gameObject.SetActive(false);
        }

        public void Show()
        {
            overlay.gameObject.SetActive(true);
        }

        void StopIfRunning()
        {
            if (running is null) return;
            StopCoroutine(running);
            running = null;
        }

        void Pause(bool on)
        {
            Time.timeScale = on ? 0f : 1f;
        }

        private IEnumerator Run(
            TutorialArcSO arc,
            int startIndex,
            Action<int> OnStepChanged,
            Action onCompleted)
        {
            if (arc?.steps is null || arc.steps.Count == 0)
            {
                onCompleted?.Invoke();
                yield break;
            }
            
            overlay.raycastTarget = true;
            Show();

            for (int i = startIndex; i < arc.steps.Count; i++)
            {
                step = arc.steps[i];
                if (step.delay > 0) yield return new WaitForSecondsRealtime(step.delay);

                switch (step.type)
                {
                    case TutorialStepType.Dialogue:
                        Pause(true);
                        highlighter.gameObject.SetActive(false);
                        forwarder.gameObject.SetActive(false);
                        yield return dialog.Show(step.text, step.speaker);
                        break;
                    
                    case TutorialStepType.Highlight:
                        Pause(true);
                        RectTransform target = null; 
                        yield return new WaitUntil(() => (target = TutorialTargets.TryGet(step.targetKey)) is not null);
                        
                        highlighter.gameObject.SetActive(true);
                        highlighter.SetTarget(target);
                        
                        forwarder.gameObject.SetActive(true);
                        forwarder.target = target;
                        forwarder.button = target.GetComponent<Button>();

                        bool done = false;
                        var btn = target.GetComponent<Button>();
                        if (btn)
                        {
                            UnityAction onClick = () => done = true;
                            btn.onClick.AddListener(onClick);
                            yield return new WaitUntil(() => done);
                            btn.onClick.RemoveListener(onClick);
                        }
                        else
                        {
                            void OnForwarderClicked() => done = true;
                            forwarder.Clicked += OnForwarderClicked;
                            yield return new WaitUntil(() => done);
                            forwarder.Clicked -= OnForwarderClicked;
                        }
                        forwarder.gameObject.SetActive(false);
                        highlighter.SetTarget(null);
                        highlighter.gameObject.SetActive(false);
                        break;
                    
                    case TutorialStepType.WaitQuestActivated:
                        Pause(false);
                        yield return WaitQuestActivated(step.quest.questId);
                        break;

                    case TutorialStepType.WaitQuestCompleted:
                        yield return WaitQuestCompleted(step.quest.questId);
                        break;
                    
                    case TutorialStepType.WaitSignal:
                        Pause(true);
                        target = null; 
                        yield return new WaitUntil(() => (target = TutorialTargets.TryGet(step.targetKey)) is not null);
                        
                        overlay.gameObject.SetActive(false);
                        yield return new WaitUntil(() => target is null || !target.gameObject.activeInHierarchy);
                        break;

                    case TutorialStepType.WaitTime:
                        Pause(false);
                        
                        if (step.waitEvent is not null)
                        {
                            bool arrived = false;
                            void OnInvoke() => arrived = true;
                            step.waitEvent.tutoEvent += OnInvoke;
                            
                            yield return new WaitUntil(() => arrived);

                            step.waitEvent.tutoEvent -= OnInvoke;
                        }
                        
                        Pause(true);
                        break;

                    case TutorialStepType.InvokeEvent:
                        step.onInvoke?.Invoke(); // 보상연출, 컷씬 등
                        break;

                    case TutorialStepType.GrantRewards:
                        //Grant(step.reward);
                        break;
                    
                    case TutorialStepType.QuestCleared:
                        QuestSignalManager.Instance.Tutorial(step.quest.Goal.enumKey.ToKeyString());
                        break;
                    
                    case TutorialStepType.GrantCrews:
                        Pause(true);
                        RewardPanel.gameObject.SetActive(true);
                        RewardPanel.Show(step.reward);
                        yield return new WaitUntil(() => RewardPanel.gameObject.activeSelf == false);
                        break;
                    
                    case TutorialStepType.ClaimReward:
                        RectTransform targetTransform = null;
                        yield return new WaitUntil(() => 
                            (targetTransform = TutorialTargets.TryGet("QuestButton")) is not null);
                        
                        highlighter.gameObject.SetActive(true);
                        highlighter.SetTarget(targetTransform);
                        
                        forwarder.gameObject.SetActive(true);
                        forwarder.target = targetTransform;
                        forwarder.button = targetTransform.GetComponent<Button>();

                        bool got = false;
                        var btn2 = targetTransform.GetComponent<Button>();
                        if (btn2)
                        {
                            UnityAction onClick = () => got = true;
                            btn2.onClick.AddListener(onClick);
                            yield return new WaitUntil(() => got);
                            btn2.onClick.RemoveListener(onClick);
                        }
                        else
                        {
                            Debug.LogError("Tutorial Highlighter : Target is not Button");
                            yield return null;
                        }

                        forwarder.gameObject.SetActive(false);
                        break;
                }
                OnStepChanged?.Invoke(i + 1);
            }
            Pause(false);
            Hide();
            onCompleted?.Invoke();
        }
        private IEnumerator WaitQuestActivated(string questId)
        {
            bool arrived = false;
            void OnUpdated(GeneralQuestDefinitionSO def, GeneralQuestInstance inst)
            {
                if (def != null && def.questId == questId) arrived = true;
            }

            var qm = QuestManager.Instance;
            if (qm != null)
            {
                qm.OnQuestUpdated += OnUpdated;
                yield return new WaitUntil(() => arrived);
                qm.OnQuestUpdated -= OnUpdated;
            }
        }

        private IEnumerator WaitQuestCompleted(string questId)
        {
            bool arrived = false;
            void OnCompleted(GeneralQuestDefinitionSO def, GeneralQuestInstance inst)
            {
                if (def != null && def.questId == questId) arrived = true;
            }

            var qm = QuestManager.Instance;
            if (qm != null)
            {
                qm.OnTutorialQuestCompleted += OnCompleted;
                yield return new WaitUntil(() => arrived);
                qm.OnTutorialQuestCompleted -= OnCompleted;
            }
        }

        // private void Grant(RewardBundle r)
        // {
        //     if (r == null) return;
        //     if (r.gold > 0) DatabaseManager.Instance.AddCurrency(nameof(QuestRewardType_Enum.Gold), r.gold);
        //     if (r.diamond > 0) DatabaseManager.Instance.AddCurrency(nameof(QuestRewardType_Enum.Gem), r.diamond);
        //     if (r.crewIds != null && r.crewIds.Length > 0)
        //     {
        //         foreach (var id in r.crewIds)
        //             CrewManager.Instance.AddCrewById(id, stars: 0);
        //     }
        // }
    }
    

}