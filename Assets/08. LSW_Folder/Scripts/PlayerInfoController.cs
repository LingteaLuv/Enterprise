using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoUIController : MonoBehaviour
{
    [SerializeField] private List<UIBase> _uiList;
    private enum PlayerInfoUIType
    {
        PlayerInfoPanel,
        NicknameChangePanel
    }
    
    void Start()
    {
        foreach (var ui in _uiList)
        {
            if (ui is PlayerInfoPanel playerInfoPanel)
            {
                playerInfoPanel.OnClickedExitBtn = () => HideUI(PlayerInfoUIType.PlayerInfoPanel);
                playerInfoPanel.OnClickedNicknameChangeBtn = () => ShowUI(PlayerInfoUIType.NicknameChangePanel);
            }
            
            if (ui is NicknameChangePanel nicknameChangePanel)
            {
                nicknameChangePanel.OnClickClosePopup = () => HideUI(PlayerInfoUIType.NicknameChangePanel);
                nicknameChangePanel.OnClickNicknameChange = () =>
                {
                    HideUI(PlayerInfoUIType.NicknameChangePanel);
                    RefreshUI(PlayerInfoUIType.PlayerInfoPanel);
                };
            }
        }
    }

    /// <summary>
    /// 패널을 여는 메서드
    /// </summary>
    /// <param name="type"></param>
    private void ShowUI(PlayerInfoUIType type)
    {
        _uiList[(int)type].SetShow();
    }

    /// <summary>
    /// 패널을 숨기는 메서드
    /// </summary>
    /// <param name="type"></param>
    private void HideUI(PlayerInfoUIType type)
    {
        _uiList[(int)type].SetHide();
    }
    
    private void RefreshUI(PlayerInfoUIType type)
    {
        _uiList[(int)type].SetHide();
        _uiList[(int)type].SetShow();
    }
}
