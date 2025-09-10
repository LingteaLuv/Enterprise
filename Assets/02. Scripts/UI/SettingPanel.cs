using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : UIBase
{
    [SerializeField] private Button _exitBtn;
    [SerializeField] private Button _bugReportButton;
    
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _effectSlider;
    
    public Action OnTouchedExitBtn;
    
    private void Start()
    {
        _exitBtn.onClick.AddListener(() =>
        {
            OnTouchedExitBtn?.Invoke();
            gameObject.SetActive(false);
        });
        
        _bugReportButton.onClick.AddListener(() =>
        {
            Application.OpenURL("https://forms.gle/nyEfTEzds2rUKATs7");
        });
        _musicSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetBGM(volume));
        _effectSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetSFX(volume));
        
        SettingManager.Instance.BGM.OnChanged += (value) => _musicSlider.value = value;
        SettingManager.Instance.SFX.OnChanged += (value) => _effectSlider.value = value;

        _musicSlider.value = SettingManager.Instance.BGM.Value;
        _effectSlider.value = SettingManager.Instance.SFX.Value;
        
        gameObject.SetActive(false);
    }
}
