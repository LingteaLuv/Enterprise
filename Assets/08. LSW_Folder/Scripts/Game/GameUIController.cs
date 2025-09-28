using System;
using System.Collections;
using System.Collections.Generic;
using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUIController : UIController<GameUIController.GameUIType>
{
    [SerializeField] private Toggle _attendanceToggle;
    [SerializeField] private Toggle _toolToggle;
    [SerializeField] private Toggle _shipToggle;
    [SerializeField] private Toggle _playerInfoToggle;
    [SerializeField] private Button _bossBtn;
    [SerializeField] private Button _dungeonBtn;
    
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _gemText;

    public WeaponStatPanel StatPanel;
    
    public enum GameUIType
    {
        AttendancePanel,
        ToolPanel,
        Btn1Panel,
        Btn2Panel,
        Btn3Panel,
        Btn4Panel,
        ShipPanel,
        PlayerInfoPanel
    }

    private void Awake()
    {
        CurrencyManager.Instance.OnUpdateCurrency += UpdateText;
        
        DragAndDropController.Instance.GetCanvas(GetComponent<Canvas>());
    }
    
    private void Start()
    {
        DatabaseManager.Instance.DisplayCreditData();
        foreach (var ui in _uiList)
        {
            if (ui is AttendancePanel attendancePanel)
            {
                attendancePanel.OnTouchedExitBtn += () =>
                {
                    HideUI(GameUIType.AttendancePanel);
                };
            }

            if (ui is ToolPanel toolPanel)
            {
                toolPanel.OnButtonTouched += (index) =>
                {
                    for (int j = 0; j < _uiList.Count; j++)
                    {
                        HideUI((GameUIType)(j));
                    }
                    ShowUI((GameUIType)(index+2));
                    _toolToggle.isOn = false;
                };
            }
        }
        _attendanceToggle.onValueChanged.AddListener((isOn) =>
        {
            if (!isOn) HideUI(GameUIType.AttendancePanel);
            else ShowUI(GameUIType.AttendancePanel);
        });
        _toolToggle.onValueChanged.AddListener((isOn) =>
        {
            if (!isOn) HideUI(GameUIType.ToolPanel);
            else ShowUI(GameUIType.ToolPanel);
        });
        _shipToggle.onValueChanged.AddListener((isOn) =>
        {
            if (!isOn)
            {
                _shipToggle.image.color = Color.white;
                HideUI(GameUIType.ShipPanel);
            }
            else
            {
                _shipToggle.image.color = Color.gray;
                ShowUI(GameUIType.ShipPanel);
            }
        });
        _playerInfoToggle.onValueChanged.AddListener((isOn) =>
        {
            if (!isOn) HideUI(GameUIType.PlayerInfoPanel);
            else ShowUI(GameUIType.PlayerInfoPanel);
        });
        
        _bossBtn.onClick.AddListener(() =>
        {
            GlobalStageManager.Instance.bossBattleTriggered = true;
            StartCoroutine(Delay());
        });
        _bossBtn.onClick.AddListener(QuestSignalManager.Instance.OnBossBattleEnter);
        _dungeonBtn.onClick.AddListener(()=>
        {
            JHT_MonsterSpawnManager.Instance.MonsterAllClear();
            BattleManager.Instance.ClearEnemies();
            QuestSignalManager.Instance.OnDungeonEntered();
            SceneManager.LoadScene("DungeonScene");
        });
        
        HideUI(GameUIType.AttendancePanel);
        HideUI(GameUIType.ToolPanel);

        //CurrencyManager.Instance.OnUpdateCurrency += UpdateText;
        //DragAndDropController.Instance.GetCanvas(GetComponent<Canvas>());
        CurrencyManager.Instance.UpdateCurrencyUI();
    }

    private void UpdateText(List<string> currency)
    {
        Debug.Log("Text 업데이트 호출");
        _goldText.text = currency[0];
        //_stoneText.text = currency[1];
        _gemText.text = currency[2];
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(5f);
        JHT_MonsterSpawnManager.Instance.MonsterAllClear();
        BattleManager.Instance.ClearEnemies();
        BattleManager.Instance.ClearPlayers();
        SceneManager.LoadScene("BossBattleScene");
    }
}
