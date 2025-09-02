using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AttendancePanel : UIBase
{
    [SerializeField] private Button _exitBtn;
    [SerializeField] private Button _completeBtn;
    [SerializeField] private List<Image> _attendanceImages;

    private int _attendance;
    private bool _isTouched;
    
    public Action OnTouchedExitBtn;

    private async void Awake()
    {
        Debug.Log("Start 진입");
        _exitBtn.onClick.AddListener(() =>OnTouchedExitBtn?.Invoke());
        _completeBtn.onClick.AddListener(()=>
        {
            if (!_isTouched)
            {
                OnTouchedCompleteBtn();
            }
        });
        _completeBtn.interactable = false;
        
        await Init();
        await CheckAttendance();
        gameObject.SetActive(false);
    }

    private async Task Init()
    {
        Debug.Log("Init 진입");
        string path = $"{FirebaseManager.Auth.CurrentUser.UserId}/UserData/Date";
        await DatabaseManager.Instance.LoadFieldAsync(path, (int value) =>
        {
            _attendance = value;
            for (int i = 0; i < value; i++)
            {
                int index = i;
                Color color = _attendanceImages[index].color;
                color.a = 0.1f;
                _attendanceImages[index].color = color;
            }
        });
    }

    private async Task CheckAttendance()
    {
        Debug.Log("CheckAttendance 진입");
        await DatabaseManager.Instance.CheckTodayReward(() =>
        {
            _completeBtn.interactable = true;
        });
    }
    
    private void OnTouchedCompleteBtn()
    {
        _completeBtn.interactable = false;
        if (_attendance % 2 == 0)
        {
            Debug.Log("100Gold를 획득하였습니다");
            DatabaseManager.Instance.AddCurrency("Gold", 100);
        }
        else
        {
            Debug.Log("10Gem을 획득하였습니다");
            DatabaseManager.Instance.AddCurrency("Gem", 10);
        }
        DatabaseManager.Instance.Attendance((value) =>
        {
            if (!_isTouched)
            {
                Debug.Log($"{_attendance}번째 이미지 알파값 변경 진행");
                Color color = _attendanceImages[_attendance].color;
                color.a = 0.1f;
                _attendanceImages[_attendance].color = color;
                _attendance++;
                _isTouched = true;
            }
        });
    }
}
