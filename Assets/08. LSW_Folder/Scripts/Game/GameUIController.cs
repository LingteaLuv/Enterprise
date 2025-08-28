using UnityEngine;

public class GameUIController : UIController<GameUIController.GameUIType>
{
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
    }
}
