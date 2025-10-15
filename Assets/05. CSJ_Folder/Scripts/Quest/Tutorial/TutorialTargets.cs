using System.Collections.Generic;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public static class TutorialTargets
    {
        private static readonly Dictionary<string, RectTransform> map = new();

        public static void Register(string key, RectTransform rt)
        {
            if (string.IsNullOrEmpty(key) || rt == null) return;
            Unregister(key, rt);
            map[key] = rt;
        }

        public static void Unregister(string key, RectTransform rt)
        {
            if (map.TryGetValue(key, out var value) && value == rt) map.Remove(key);
        }
        
        public static RectTransform TryGet(string key) => map.TryGetValue(key, out var value) ? value : null;
         
    }

    public class TutorialTarget : MonoBehaviour
    {
        public string Key;
        private void OnEnable() => TutorialTargets.Register(Key, transform as RectTransform);
        private void OnDisable() => TutorialTargets.Unregister(Key, transform as RectTransform);
    }
}