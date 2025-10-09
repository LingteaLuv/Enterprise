using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.Data
{
    [System.Serializable]
    public class TutorialProgress
    {
        public string currentArcId;
        public int stepIndex;
        public bool finished;
    }

    public static class TutorialProgressStore
    {
        const string key = "tutorialProgress";

        public static TutorialProgress Load()
        {
            if (!PlayerPrefs.HasKey(key))
                return new TutorialProgress {currentArcId = "", stepIndex = 0, finished = false};
            
            var json = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(json))
                return new TutorialProgress {currentArcId = "", stepIndex = 0, finished = false};
            
            return JsonUtility.FromJson<TutorialProgress>(json) ?? new TutorialProgress();
        }

        public static void Save(TutorialProgress progress)
        {
            var json = JsonUtility.ToJson(progress);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }
    }
}