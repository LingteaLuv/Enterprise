using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class SettingManager : Singleton<SettingManager>
{
    public Property<float> BGM;
    public Property<float> SFX;
    public Property<bool> CamMode;
    
 
    protected override void Awake()
    {
        base.Awake();
        Init();
 
        BGM.OnChanged += (value) => SaveSettings();
        SFX.OnChanged += (value) => SaveSettings();
        CamMode.OnChanged += (value) => SaveSettings();
    }
     
    private void Init()
    {
        BGM = new Property<float>(0.5f);
        SFX = new Property<float>(0.5f);
        CamMode = new Property<bool>(true);
        LoadSettings();
    }
 
    public void SetBGM(float input)
    {
        BGM.Value = input;
    }
     
    public void SetSFX(float input)
    {
        SFX.Value = input;
    }
 
    public void SetCamMode(bool input)
    {
        CamMode.Value = input;
    }
 
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", BGM.Value);
        PlayerPrefs.SetFloat("SFXVolume", SFX.Value);
        PlayerPrefs.SetInt("IsFixedCam", CamMode.Value ? 1 : 0);
    }
 
    private void LoadSettings()
    {
        BGM.Value = PlayerPrefs.HasKey("BGMVolume") ? PlayerPrefs.GetFloat("BGMVolume") : 0.5f;
        SFX.Value = PlayerPrefs.HasKey("SFXVolume") ? PlayerPrefs.GetFloat("SFXVolume") : 0.5f;
        CamMode.Value = PlayerPrefs.HasKey("IsFixedCam") && PlayerPrefs.GetInt("IsFixedCam") == 1;
    }
}
