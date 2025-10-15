using System;
using System.Collections;
using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest.SO.Reward;
using UnityEngine;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Quest.UI
{
    public class RewardPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Transform content;        // 카드가 들어갈 부모
        [SerializeField] private RewardView itemPrefab;
        [SerializeField] private Button closeButton;

        [Header("Options")]
        [SerializeField] private float fadeIn = 0.18f;
        [SerializeField] private float fadeOut = 0.12f;
        [SerializeField] private bool pauseWhileOpen = true;

        private readonly List<GameObject> _spawned = new();
        private Coroutine running;

        void Awake()
        {
            if (group)
            {
                group.alpha = 0;
                group.blocksRaycasts = false;
                group.interactable = false;
            }
            if (closeButton) closeButton.onClick.AddListener(Close);
            gameObject.SetActive(false);
        }

        public void Show(RewardBundleSO bundle, Action onClosed = null)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(CoShow(bundle, onClosed));
        }

        public void Show(IEnumerable<RewardBundleSO.Item> items, Action onClosed = null)
        {
            var so = ScriptableObject.CreateInstance<RewardBundleSO>();
            so.items.AddRange(items);
            Show(so, onClosed);
        }

        private IEnumerator CoShow(RewardBundleSO bundle, Action onClosed)
        {
            if (bundle is null || bundle.items is null || bundle.items.Count is 0)
            {
                Debug.LogError("RewardPanel : Show() : bundle is null or empty");
                yield break;
            }

            gameObject.SetActive(true);
            Clear();
            
            Debug.Log($"RewardPanel : Show() : {bundle.items.Count} items");
            
            foreach (var it in bundle.items)
            {
                var view = Instantiate(itemPrefab, content);
                if (it.rewardType == QuestRewardType_Enum.Crew) view.SetChar(it);
                else view.Set(it);
                _spawned.Add(view.gameObject);
            }
            
            // Fade In
            if (group)
            {
                group.blocksRaycasts = true;
                group.interactable = true;
                closeButton.gameObject.SetActive(true);
                yield return Fade(0, 1, fadeIn);
            }
            
            bool closed = false;
            void OnCloseClick() => closed = true;
            closeButton.onClick.AddListener(OnCloseClick);
            yield return new WaitUntil(() => closed);
            closeButton.onClick.RemoveListener(OnCloseClick);

            // Fade Out
            if (group)
            {
                group.interactable = false;
                group.blocksRaycasts = false;
                yield return Fade(1, 0, fadeOut);
            }

            gameObject.SetActive(false);
            onClosed?.Invoke();
            running = null;
        }

        private IEnumerator Fade(float from, float to, float time)
        {
            if (!group || time <= 0f) { if (group) group.alpha = to; yield break; }

            float t = 0;
            while (t < time)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, t / time);
                yield return null;
            }
            group.alpha = to;
        }

        private void Clear()
        {
            foreach (var go in _spawned) if (go) Destroy(go);
            _spawned.Clear();
        }

        public void Close()
        {
            group.blocksRaycasts = false;
            closeButton.gameObject.SetActive(false);
            gameObject.SetActive(false); 
            
            
        }
        
    }
}