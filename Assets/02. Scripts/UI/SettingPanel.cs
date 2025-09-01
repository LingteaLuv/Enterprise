using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : UIBase
{
    [SerializeField] Button _bugReportButton;
    
    [SerializeField] Slider _musicSlider;
    [SerializeField] Slider _effectSlider;
    
    private void Start()
    {
        _bugReportButton.onClick.AddListener(() =>
        {
            Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSe6Q1V_gKJZvaKluFWdPRBpss0Rn6B5FnecEgl-s1lOxSwIjw/viewform?usp=sharing&ouid=100453097753956903851");
        });
        _musicSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetBGM(volume));
        _effectSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetSFX(volume));
        
        SettingManager.Instance.BGM.OnChanged += (value) => _musicSlider.value = value;
        SettingManager.Instance.SFX.OnChanged += (value) => _effectSlider.value = value;

        _musicSlider.value = SettingManager.Instance.BGM.Value;
        _effectSlider.value = SettingManager.Instance.SFX.Value;
    }
}
