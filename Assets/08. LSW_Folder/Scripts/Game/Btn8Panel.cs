using System;
using UnityEngine;
using UnityEngine.UI;

public class Btn8Panel : UIBase
{
    [SerializeField] private Button _exitBtn;
    
    public Action OnTouchedExitBtn;
    
    private void Start()
    {
        _exitBtn.onClick.AddListener(() =>
        {
            OnTouchedExitBtn?.Invoke();
            gameObject.SetActive(false);
        });
        gameObject.SetActive(false);
    }
}
