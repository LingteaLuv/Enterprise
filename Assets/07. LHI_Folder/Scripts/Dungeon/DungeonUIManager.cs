using DungeonSystem;
using Google.Impl;
using System.Collections.Generic;
using UnityEngine;

public class DungeonUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject normalBattlePanel;
    public GameObject eliteBattlePanel;
    public GameObject bossBattlePanel;
    public GameObject restRoomPanel;
    public GameObject treasureRoomPanel;

    private Dictionary<ZoneType, GameObject> panelDictionary;
    private GameObject currentActivePanel;

    private void Awake()
    {
        InitializePanels();
    }

    void Start()
    {
        DungeonStage.Instance.OnZoneChanged += OnZoneChanged;
    }

    private void InitializePanels()
    {
        panelDictionary = new Dictionary<ZoneType, GameObject>
        {
            { ZoneType.NormalBattle, normalBattlePanel },
            { ZoneType.EliteBattle, eliteBattlePanel },
            { ZoneType.BossBattle, bossBattlePanel },
            { ZoneType.RestRoom, restRoomPanel },
            { ZoneType.TreasureRoom, treasureRoomPanel }
        };

        // 모든 패널을 초기에는 비활성화 상태로 둡니다.
        foreach (var panel in panelDictionary.Values)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        currentActivePanel = normalBattlePanel;
        OpenPanel();
    }

    /// <summary>
    /// 구역 변경 알림을 받았을 때 호출될 메서드.
    /// </summary>
    public void OnZoneChanged(Zone newZone)
    {
        // 현재 활성화된 패널이 있다면 비활성화
        CloseCurrentPanel();
        // 지정된 구역 타입에 맞는 패널을 찾아 활성화
        if (panelDictionary.TryGetValue(newZone.zoneType, out GameObject targetPanel))
        {
            if (targetPanel != null)
            {
                currentActivePanel = targetPanel;
            }
        }

        // 새로운 구역 타입에 맞는 패널을 열기
        OpenPanel();
    }

    public void OpenPanel()
    {
        // 지정된 구역 타입에 맞는 패널을 찾아 활성화
        if (currentActivePanel != null)
            currentActivePanel.SetActive(true);
    }

    /// <summary>
    /// 현재 활성화된 패널을 닫는 메서드
    /// </summary>
    public void CloseCurrentPanel()
    {
        if (currentActivePanel != null)
        {
            currentActivePanel.SetActive(false);
        }
    }
}