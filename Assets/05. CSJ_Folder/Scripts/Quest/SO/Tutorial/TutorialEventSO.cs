using System;
using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Quest.SO.Tutorial
{
    [CreateAssetMenu (menuName = "Tutorial/Event")]
    public class TutorialEventSO :ScriptableObject
    {
        public Action tutoEvent;
        public void Raise()
        {
            tutoEvent?.Invoke();
            Debug.LogError("튜토 이벤");
            Debug.LogError($"이벤트 발생한 WaitEvent instance ID: {this.GetInstanceID()}");
        }
    }
}