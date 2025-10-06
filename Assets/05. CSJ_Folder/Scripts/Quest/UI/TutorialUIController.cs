using System;
using System.Collections;
using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest.Definition;
using _05._CSJ_Folder.Scripts.Quest.SO.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace _05._CSJ_Folder.Scripts.Quest.UI
{
    public class TutorialUIController : MonoBehaviour
    {
        [SerializeField] private Canvas overlayCanvas;
        [SerializeField] private Image overlay;
        [SerializeField] private TutorialHighlighter highlighter;
        [SerializeField] private TutorialClickForwarder forwarder;
        [SerializeField] private TutorialDialogView dialog;

        private Coroutine running;


        public void StartSequence(TutorialStepSequenceSO sequence,
            int startIndex = 0,
            Action<int> OnStepChanged = null,
            Action onCompleted = null)
        {
            StopIfRunning();
            running = StartCoroutine(Run(sequence?.steps, startIndex, OnStepChanged, onCompleted));
        }

        public void Hide()
        {
            StopIfRunning();
            overlayCanvas.enabled = false;
            overlay.raycastTarget = false;
            highlighter.gameObject.SetActive(false);
            forwarder.gameObject.SetActive(false);
        }

        void StopIfRunning()
        {
            if (running is null) return;
            StopCoroutine(running);
            running = null;
        }

        private IEnumerator Run(
            IList<TutorialStepSO> steps,
            int startIndex,
            Action<int> OnStepChanged,
            Action onCompleted)
        {
            if ( steps is null || steps.Count == 0 ) yield break;
            
            overlayCanvas.enabled = true;
            overlay.raycastTarget = true;

            for (int i = Mathf.Clamp(startIndex, 0, steps.Count - 1); i < steps.Count; i++)
            {
                var step = steps[i];
                if (step.delay > 0) yield return new WaitForSeconds(step.delay);

                switch (step.type)
                {
                    case TutorialStepType.Dialogue:
                        highlighter.gameObject.SetActive(false);
                        forwarder.gameObject.SetActive(false);
                        yield return dialog.Show(step.text);
                        break;
                    case TutorialStepType.Highlight:
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
                            btn.onClick.AddListener(() => done = true);
                            yield return new WaitUntil(() => done);
                            btn.onClick.RemoveAllListeners();
                        }
                        else
                        {
                            Debug.LogError("Tutorial Highlighter : Target is not Button");
                            yield return null;
                        }
                        forwarder.gameObject.SetActive(false);
                        break;
                    
                    case TutorialStepType.WaitSignal:
                        if (step.signal is null) break;
                        bool arrived = false; 
                        void Handler(TutorialQuestDefinitionSO _) => arrived = true;
                        step.signal.OnSignal += Handler;
                        yield return new WaitUntil(() => arrived);
                        step.signal.OnSignal -= Handler;
                        break;
                    
                    case TutorialStepType.CompleteQuest:
                        //TODO
                        break;
                    
                    case TutorialStepType.ClaimReward:
                        RectTransform targetTransform = null;
                        yield return new WaitUntil(() => 
                            (targetTransform = TutorialTargets.TryGet("QuestClaimButton")) is not null);
                        
                        highlighter.gameObject.SetActive(true);
                        highlighter.SetTarget(targetTransform);
                        
                        forwarder.gameObject.SetActive(true);
                        forwarder.target = targetTransform;
                        forwarder.button = targetTransform.GetComponent<Button>();

                        bool got = false;
                        var btn2 = targetTransform.GetComponent<Button>();
                        if (btn2)
                        {
                            btn2.onClick.AddListener(() => got = true);
                            yield return new WaitUntil(() => got);
                            btn2.onClick.RemoveAllListeners();
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
            highlighter.gameObject.SetActive(false);
            forwarder.gameObject.SetActive(false);
            overlayCanvas.enabled = false;
            overlay.raycastTarget = false;

            onCompleted?.Invoke();
        }
    }
}