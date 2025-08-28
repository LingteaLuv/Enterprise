using UnityEngine;
using UnityEngine.UI;

public class GameUIController : UIController<GameUIController.GameUIType>
{
    [SerializeField] private Toggle _attendanceBtn;
    
    public enum GameUIType
    {
        AttendancePanel,
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
        }
        _attendanceBtn.onValueChanged.AddListener((isOn) =>
        {
            if (isOn) HideUI(GameUIType.AttendancePanel);
            else ShowUI(GameUIType.AttendancePanel);
        });
    }
}
