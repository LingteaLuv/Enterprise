using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public static class TutorialUtils
    {
        // key로 찾고 없으면 기다렸다가 CanvasGroup 붙이고 알파/인터랙션 설정
        public static IEnumerator WaitAndSetAlpha(string key, float alpha, bool blocksRaycasts = false, bool interactable = false)
        {
            RectTransform rt = null;
            yield return new WaitUntil(() => (rt = TutorialTargets.TryGet(key)) != null);

            var cg = rt.GetComponent<CanvasGroup>();
            if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();

            cg.alpha = alpha;
            cg.blocksRaycasts = blocksRaycasts;
            cg.interactable = interactable;
        }

        // 바로 시도(등록이 이미 되어 있다고 확신할 때)
        public static void SetAlphaNow(string key, float alpha, bool blocksRaycasts = false, bool interactable = false)
        {
            var rt = TutorialTargets.TryGet(key);
            if (rt == null) return;

            var cg = rt.GetComponent<CanvasGroup>() ?? rt.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
            cg.blocksRaycasts = blocksRaycasts;
            cg.interactable = interactable;
        }

        public static void BtnInvoke(string key)
        {
            var rt = TutorialTargets.TryGet(key);
            if (rt == null) return;

            var btn = rt.GetComponent<Button>();
            btn.onClick?.Invoke();
        }
    }
}