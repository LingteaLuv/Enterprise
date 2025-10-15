using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class TutorialDialogView : MonoBehaviour
    {
        public CanvasGroup group;
        public TextMeshProUGUI Speaker;
        public TextMeshProUGUI text;
        public Button nextButton;

        void Awake()
        {
            group.alpha = 0;
            group.blocksRaycasts = false;
            nextButton.gameObject.SetActive(false);
        }

        public IEnumerator Show(string speaker, string message)
        {
            if (speaker is not null)
            {
                Speaker.gameObject.SetActive(true);
                Speaker.text = speaker;
            }
            else
            {
                Speaker.gameObject.SetActive(false);
            }

            text.text = message;
            group.blocksRaycasts = true;
            nextButton.gameObject.SetActive(true);
            yield return Fade(0, 1, 0.15f);

            bool clicked = false;
            nextButton.onClick.AddListener(() => clicked = true);
            yield return new WaitUntil(() => clicked);
            nextButton.onClick.RemoveAllListeners();
            nextButton.gameObject.SetActive(false);
            
            group.blocksRaycasts = false;
            yield return Fade(1, 0, 0.12f);
        }
        
        private IEnumerator Fade(float from, float to, float time)
        {
            float t = 0;
            while (t < time)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, t / time);
                yield return null;
            }
            group.alpha = to;
        }

    }
}