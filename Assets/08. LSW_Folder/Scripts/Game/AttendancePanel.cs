using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AttendancePanel : UIBase
{
    [SerializeField] private Button _exitBtn;
    [SerializeField] private List<GameObject> _attendanceGameobjects;

    private Button _currentBtn;
    private int _attendance;
    private bool _isTouched;
    
    public Action OnTouchedExitBtn;

    private async void Awake()
    {
        _exitBtn.onClick.AddListener(() =>OnTouchedExitBtn?.Invoke());
        for (int i = 0; i < _attendanceGameobjects.Count; i++)
        {
            int index = i;
            _attendanceGameobjects[i].GetComponent<Button>().onClick.AddListener( () => OnTouchedBtn(index));
            _attendanceGameobjects[i].GetComponent<Button>().interactable = false;
        }
        await Init();
        await CheckAttendance();
        //gameObject.SetActive(false);
    }

    private async Task Init()
    {
        string path = $"{FirebaseManager.Auth.CurrentUser.UserId}/UserData/Date";
        await DatabaseManager.Instance.LoadFieldAsync<int>(path, (value) =>
        {
            _attendance = value;
            _currentBtn = _attendanceGameobjects[_attendance].GetComponent<Button>();
            for (int i = 0; i < value; i++)
            {
                int index = i;
                _attendanceGameobjects[index].transform.GetChild(2).gameObject.SetActive(true);
                _attendanceGameobjects[index].transform.GetChild(3).gameObject.SetActive(true);
            }
        }, true, value : 0);
    }

    private async Task CheckAttendance()
    {
        Debug.Log("CheckAttendance 진입");
        await DatabaseManager.Instance.CheckTodayReward(() =>
        {
            _currentBtn.interactable = true;
        });
    }
    
    private void OnTouchedBtn(int index)
    {
        _attendanceGameobjects[index].GetComponent<Button>().interactable = false;
        /*if (_attendance % 2 == 0)
        {
            Debug.Log("100Gold를 획득하였습니다");
            DatabaseManager.Instance.AddCurrency("Gold", 100);
        }
        else
        {
            Debug.Log("10Gem을 획득하였습니다");
            DatabaseManager.Instance.AddCurrency("Gem", 5);
        }*/
        DatabaseManager.Instance.AddCurrency("Gem", 5);
        DatabaseManager.Instance.Attendance((value) =>
        {
            if (!_isTouched)
            {
                _isTouched = true;
                _attendanceGameobjects[index].transform.GetChild(2).gameObject.SetActive(true);
                _attendanceGameobjects[index].transform.GetChild(3).gameObject.SetActive(true);
                _attendance++;
            }
        });
    }
}
