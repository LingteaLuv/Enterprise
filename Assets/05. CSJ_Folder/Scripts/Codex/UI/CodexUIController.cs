using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _05._CSJ_Folder.Scripts.Quest;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Codex.UI
{
    public class CodexUIController : UIBase
    { 
        [SerializeField] private CodexListController _codexListController;
        [SerializeField] private Button _closeButton;

        [SerializeField] private Button[] stdButtons;
        [SerializeField] private GameObject[] stdPanels;
        
        [SerializeField] private Button[] factionButtons;
        [SerializeField] private GameObject[] factionPanels;
        
        private Faction _faction;
        private CodexStd_Enum _std;

        private Dictionary<Button, CodexStd_Enum> _stdButtonMap = new Dictionary<Button, CodexStd_Enum>();
        private Dictionary<Button, GameObject> _stdPanelMap = new Dictionary<Button, GameObject>();
        private Dictionary<Button, Faction> _factionButtonMap= new Dictionary<Button, Faction>();
        private Dictionary<Button, GameObject> _factionPanelMap = new Dictionary<Button, GameObject>();

        public Action OnTouchedExitBtn;
        
        public bool isReady = false;
        private bool _bootstrapped;
        private Coroutine _waitReadyCo;

        private void Awake()
        {
            _stdButtonMap.Clear();
            _stdPanelMap.Clear();
            _factionButtonMap.Clear();
            _factionPanelMap.Clear();
            
            for (int i = 0; i < factionButtons.Length; i++)
            {
                var faction = (Faction)(i);
                factionButtons[i].onClick.AddListener(() => ChangeType(faction));
                _factionButtonMap.TryAdd(factionButtons[i], faction);
                _factionPanelMap.TryAdd(factionButtons[i], factionPanels[i]);
            }
            
            for (int i = 0; i < stdButtons.Length; i++)
            {
                var std = (CodexStd_Enum)(i);
                stdButtons[i].onClick.AddListener(() => ChangeType(std));
                _stdButtonMap.TryAdd(stdButtons[i], std);
                _stdPanelMap.TryAdd(stdButtons[i], stdPanels[i]);
            }
        }

        private void OnEnable()
        {
            Debug.Log("Enable");
            _closeButton.onClick.AddListener(CloseQuestTab);
            InitCodexTab();
            if (_waitReadyCo != null) StopCoroutine(_waitReadyCo);
            _waitReadyCo = StartCoroutine(WaitReady());
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveAllListeners();
            foreach (var button in stdButtons)
            {
                button.onClick.RemoveAllListeners();
            }
            foreach (var button in factionButtons)
            {
                button.onClick.RemoveAllListeners();
            }
            if (_waitReadyCo != null) { StopCoroutine(_waitReadyCo); _waitReadyCo = null; }
        }

        private IEnumerator WaitReady()
        {
            yield return new WaitUntil(() => isReady);
            
            ChangeCodexType(_faction, _std);
        }
        
        private void InitCodexTab()
        {
            _faction = Faction.Marine;
            _std = CodexStd_Enum.Level;
        }

        private void CloseQuestTab()
        {  
            OnTouchedExitBtn?.Invoke();
            gameObject.SetActive(false);
        }

        private void ChangeType(Faction faction)
        {
            _faction = faction;
            ChangeCodexType(faction, _std);
        }

        private void ChangeType(CodexStd_Enum std)
        {
            _std = std;
            ChangeCodexType(_faction, std);
        }

        private void ChangeCodexType(Faction faction, CodexStd_Enum std)
        {
            foreach (var kv in _stdButtonMap)
            {
                var btn = kv.Key;
                var type = kv.Value;
                if (!_stdPanelMap.TryGetValue(btn, out var panel)) continue;
                
                bool on = type == std;
                panel.SetActive(on);
                btn.interactable = !on;
            }
            foreach (var kv in _factionButtonMap)
            {
                var btn = kv.Key;
                var type = kv.Value;
                if (!_factionPanelMap.TryGetValue(btn, out var panel)) continue;
                
                bool on = faction == type;
                panel.SetActive(on);
                btn.interactable = !on;
            }
            
            _codexListController.RebuildList(faction, std);
        }

        public void UpdateCodex(CodexInstance inst)
        {
            _codexListController.Refresh(inst);
        }

        public void CodexListInit(Dictionary<(Faction, CodexStd_Enum), List<CodexInstance>> instDic)
        {
            _codexListController._instMap = instDic;
            isReady = true;
        }
    }
}