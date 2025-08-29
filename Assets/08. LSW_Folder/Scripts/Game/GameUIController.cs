using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUIController : UIController<GameUIController.GameUIType>
{
    [SerializeField] private Toggle _attendanceToggle;
    [SerializeField] private Toggle _toolToggle;

    
    public enum GameUIType
    {
        AttendancePanel,
        ToolPanel,
        Btn1Panel,
        Btn2Panel,
        Btn3Panel,
        Btn4Panel,
        Btn5Panel,
        Btn6Panel,
        Btn7Panel,
        Btn8Panel
    }
    
    private void Start()
    {
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
        
        HideUI(GameUIType.AttendancePanel);
        HideUI(GameUIType.ToolPanel);
    }
}
