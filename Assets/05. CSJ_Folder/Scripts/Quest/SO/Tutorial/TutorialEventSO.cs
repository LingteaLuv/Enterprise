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
            Debug.LogError("tutoEvent");
        }
    }
}