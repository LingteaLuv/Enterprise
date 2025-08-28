using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabBarController : MonoBehaviour
{
    [Header("하단 버튼")]
    [SerializeField] Button basicStatBtn;
    [SerializeField] Button characterListBtn;
    [SerializeField] Button invenBtn;
    [SerializeField] Button gachaBtn;
    [SerializeField] Button shopBtn;
    [SerializeField] Button questBtn;

    [Header("버튼에 연결된 UI")]
    [SerializeField] GameObject basicStatPanel;
    [SerializeField] GameObject characterListPanel;
    [SerializeField] GameObject invenPanel;
    [SerializeField] GameObject gachaPanel;
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject questPanel;

    [Header("공용 닫기 버튼")]
    [SerializeField] Button closeBtn;
    [SerializeField] Button homeBtn;

    private Dictionary<Button, GameObject> tabToPanel;
    private GameObject currentOpenPanel;

    private void Awake()
    {
        tabToPanel = new Dictionary<Button, GameObject>
        {
            { basicStatBtn, basicStatPanel },
            { characterListBtn, characterListPanel },
            { invenBtn, invenPanel },
            { gachaBtn, gachaPanel },
            { shopBtn, shopPanel },
            { questBtn, questPanel },
        };

        Init();
    }

    private void OnDisable()
    {
        Debug.Log($"111111111111111111111{name} - {GetType().Name} Disabled", this);
    }
    
    void Init()
    {
        // 초기 상태: 모든 패널 끄기, 닫기 버튼 숨김
        foreach (var panel in tabToPanel.Values)
            panel.SetActive(false);
        closeBtn.gameObject.SetActive(false);

        // 탭 버튼 이벤트 연결
        foreach (var item in tabToPanel)
        {
            Button tabBtn = item.Key;
            GameObject panel = item.Value;

            tabBtn.onClick.AddListener(() =>
            {
                CloseAllPanels();
                panel.SetActive(true);
                currentOpenPanel = panel;
                closeBtn.transform.position = tabBtn.transform.position;
                closeBtn.gameObject.SetActive(true);
            });
        }

        // 공용 닫기 버튼 이벤트 연결
        closeBtn.onClick.AddListener(() =>
        {
            if (currentOpenPanel != null)
            {
                currentOpenPanel.SetActive(false);
                currentOpenPanel = null;
            }
            closeBtn.gameObject.SetActive(false);
        });

        homeBtn.onClick.AddListener(() =>
        {
            if (currentOpenPanel != null)
            {
                currentOpenPanel.SetActive(false);
                currentOpenPanel = null;
            }
            closeBtn.gameObject.SetActive(false);
        });
    }

    void CloseAllPanels()
    {
        foreach (var panel in tabToPanel.Values)
            panel.SetActive(false);
        currentOpenPanel = null;
        closeBtn.gameObject.SetActive(false);
    }
}
