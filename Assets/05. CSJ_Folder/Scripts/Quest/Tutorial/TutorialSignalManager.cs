using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _05._CSJ_Folder.Scripts.Quest.SO.Tutorial;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _05._CSJ_Folder.Scripts.Quest
{
    public class TutorialSignalManager : Singleton<TutorialSignalManager>
    {
        [Serializable] 
        struct eventEnum
        {
            public TutorialEventSO events;
            public string name;
        }
        
        [SerializeField] private eventEnum[] events;
       
        public Dictionary<string,TutorialEventSO> eventDic;

        private void Awake()
        {
            base.Awake();


            eventDic = new Dictionary<string, TutorialEventSO>();

            foreach (var e in events)
            {
                eventDic.TryAdd(e.name, e.events);
            }
        }
        
        public void ConnectEvent(string eName)
        {
            if (eventDic.TryGetValue(eName, out var value))
            {
                value.Raise();
            }
        }
        
        
    }
}