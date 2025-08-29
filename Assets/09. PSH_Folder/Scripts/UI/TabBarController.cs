using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    // 애니메이션을 위한 변수
    private float animationDuration = 0.25f; // 애니메이션 시간
    private float closeBtnOffsetY = 150f; // 버튼이 사라지거나 나타날 때의 Y 오프셋

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

            tabBtn.onClick.AddListener(() => { OnTabSelected(tabBtn, panel); });
        }

        // 공용 닫기 버튼 이벤트 연결
        closeBtn.onClick.AddListener(ClosePanelAndAnimateButtonOut);
        homeBtn.onClick.AddListener(ClosePanelAndAnimateButtonOut);
    }

    private void OnTabSelected(Button tabBtn, GameObject panel)
    {
        bool isFirstOpen = currentOpenPanel == null;
        bool isDifferentTab = currentOpenPanel != panel;

        if (isFirstOpen)
        {
            AnimateCloseButtonIn(tabBtn);
        }
        else if (isDifferentTab)
        {
            AnimateCloseButtonMove(tabBtn);
        }

        // 패널 관리
        CloseAllPanels(false);
        panel.SetActive(true);
        currentOpenPanel = panel;
    }
    // 닫기 버튼 등장
    private void AnimateCloseButtonIn(Button targetTab)
    {
        Vector3 startPos = targetTab.transform.position - new Vector3(0, closeBtnOffsetY, 0);
        closeBtn.transform.position = startPos;
        closeBtn.gameObject.SetActive(true);

        closeBtn.transform.DOMove(targetTab.transform.position, animationDuration).SetEase(Ease.InOutQuad);
    }

    // 닫기 버튼 x이동
    private void AnimateCloseButtonMove(Button targetTab)
    {
        closeBtn.transform.DOMoveX(targetTab.transform.position.x, animationDuration).SetEase(Ease.OutQuad);
    }

    // 닫기 버튼 퇴장
    private void AnimateCloseButtonOut()
    {
        Vector3 endPos = closeBtn.transform.position - new Vector3(0, closeBtnOffsetY, 0);

        closeBtn.transform.DOMove(endPos, animationDuration).SetEase(Ease.InQuad).OnComplete(() =>
        {
            closeBtn.gameObject.SetActive(false);
        });
    }

    void ClosePanelAndAnimateButtonOut()
    {
        if (currentOpenPanel != null)
        {
            currentOpenPanel.SetActive(false);
            currentOpenPanel = null;
        }
        AnimateCloseButtonOut();
    }

    void CloseAllPanels(bool hideButton = true)
    {
        foreach (var panel in tabToPanel.Values)
        {
            if (panel.activeSelf)
                panel.SetActive(false);
        }

        if (hideButton)
            closeBtn.gameObject.SetActive(false);
    }
}
