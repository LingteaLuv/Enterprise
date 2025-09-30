using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _05._CSJ_Folder.Scripts.Quest;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace _05._CSJ_Folder.Scripts.Codex.UI
{
    public class CodexUIController : UIBase
    { 
        [SerializeField] private TextMeshProUGUI _codexName;
        
        [SerializeField] private CodexListController _codexListController;
        [SerializeField] private Button _closeButton;

        [SerializeField] private Slider _progress;
        [SerializeField] private TextMeshProUGUI _progressText;
        
        [SerializeField] private TextMeshProUGUI _curLevelText;
        [SerializeField] private TextMeshProUGUI _nextLevelText;
        
        [SerializeField] private Button[] stdButtons;
        // [SerializeField] private GameObject[] stdPanels;
        
        [SerializeField] private Button[] factionButtons;
        //[SerializeField] private GameObject[] factionPanels;

        [SerializeField] private Sprite _toggleButton_On;
        [SerializeField] private Sprite _toggleButton_Off;
        
        
        private Faction _faction;
        private CodexStd_Enum _std;

        private Dictionary<Button, CodexStd_Enum> _stdButtonMap = new Dictionary<Button, CodexStd_Enum>();
        private Dictionary<Button, Faction> _factionButtonMap= new Dictionary<Button, Faction>();

        public Action OnTouchedExitBtn;
        
        private bool isReady = false;
        private bool _bootstrapped;
        private Coroutine _waitReadyCo;
        
        private readonly string pirateText = "해적 도감";
        private readonly string marineText = "해군 도감";
        private readonly string monsterText = "괴물 도감";
        
        private int _codexIndex;
        
        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {

        }

        private void OnEnable()
        {
            Debug.Log("Enable");
            _closeButton.onClick.AddListener(CloseQuestTab);
            InitCodexTab();
            _stdButtonMap.Clear();
            _factionButtonMap.Clear();
            
            for (int i = 0; i < factionButtons.Length; i++)
            {
                var faction = (Faction)(i);
                factionButtons[i].onClick.AddListener(() => ChangeType(faction));
                _factionButtonMap.TryAdd(factionButtons[i], faction);
                //_factionPanelMap.TryAdd(factionButtons[i], factionPanels[i]);
            }
            
            for (int i = 0; i < stdButtons.Length; i++)
            {
                var std = (CodexStd_Enum)(i);
                stdButtons[i].onClick.AddListener(() => ChangeType(std));
                _stdButtonMap.TryAdd(stdButtons[i], std);
                //_stdPanelMap.TryAdd(stdButtons[i], stdPanels[i]);
            }
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
            _faction = Faction.Pirate;
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
                
                bool on = type == std;
                btn.interactable = !on;
                btn.image.sprite = on ? _toggleButton_On : _toggleButton_Off;
            }
            foreach (var kv in _factionButtonMap)
            {
                var btn = kv.Key;
                var type = kv.Value;
                
                bool on = faction == type;
                btn.interactable = !on;
                btn.image.sprite = on ? _toggleButton_On : _toggleButton_Off;
            }

            _codexName.text = faction switch
            {
                Faction.Pirate => pirateText,
                Faction.Marine => marineText,
                Faction.Monster => monsterText,
                _ => _codexName.text
            };

            var curValue = CodexManager.Instance.GetCurrentValue(faction, std);
            _progress.value = curValue;
            var nextValue = CodexManager.Instance.GetNextValue(faction, std);
            _progress.maxValue = nextValue[0];
            _progressText.text = $"{curValue} / {nextValue[0]}";
            
            _curLevelText.text = nextValue[1].ToString();
            _nextLevelText.text = nextValue[2].ToString();
            
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