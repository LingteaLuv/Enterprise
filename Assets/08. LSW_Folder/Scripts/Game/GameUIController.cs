using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _05._CSJ_Folder.Scripts.Quest;
using Cysharp.Threading.Tasks;
using JHT;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUIController : UIController<GameUIController.GameUIType>
{
    [SerializeField] private Toggle _attendanceToggle;
    [SerializeField] private Toggle _toolToggle;
    [SerializeField] private Button _shipBtn;
    [SerializeField] private Button _codexBtn;
    [SerializeField] private Button _playerInfoBtn;
    [SerializeField] private Button _bossBtn;

    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _gemText;

    //public WeaponStatPanel StatPanel;

    public enum GameUIType
    {
        AttendancePanel,
        ToolPanel,
        Btn1Panel,
        Btn2Panel,
        ShipPanel,
        PlayerInfoPanel,
        CodexPanel
    }

    private void Awake()
    {
        CurrencyManager.Instance.OnUpdateCurrency += UpdateText;

        DragAndDropController.Instance.GetCanvas(GetComponent<Canvas>());
        
        TutorialTargets.Register("BossEnterButton", _bossBtn.transform as RectTransform);
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
                    ShowUI((GameUIType)(index + 2));
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
        _shipBtn.onClick.AddListener(() =>
        {
            ShowUI(GameUIType.ShipPanel);
        });
        _playerInfoBtn.onClick.AddListener(() =>
        {
            ShowUI(GameUIType.PlayerInfoPanel);
        });

        _codexBtn.onClick.AddListener(() =>
        {
            ShowUI(GameUIType.CodexPanel);
        });

        _bossBtn.onClick.AddListener(() =>
        {
            GlobalStageManager.Instance.bossBattleTriggered = true;
            StartCoroutine(Delay());
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

        yield return new WaitForSeconds(1f);
        BossBattleManager.Instance.Battle();
        SceneManager.LoadScene("BossBattleScene");
    }


}
