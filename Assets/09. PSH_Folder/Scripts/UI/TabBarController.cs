using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TabBarController : MonoBehaviour
{
    [Header("하단 버튼")]
    [SerializeField] public Button characterListBtn;
    [SerializeField] Button invenBtn;
    [SerializeField] Button gachaBtn;
    [SerializeField] Button shopBtn;

    [Header("버튼에 연결된 UI (UIBase를 상속받아야 함)")]
    [SerializeField] public UIBase characterListPanel;
    [SerializeField] UIBase invenPanel;
    [SerializeField] UIBase gachaPanel;
    [SerializeField] UIBase shopPanel;

    [Header("공용 닫기 버튼")]
    [SerializeField] Button closeBtn;
    [SerializeField] Button homeBtn;
    [SerializeField] Button blocker;

    private Dictionary<Button, UIBase> tabToPanel;
    private Dictionary<UIBase, Vector3> panelOriginalPositions;
    private UIBase currentOpenPanel;

    // 애니메이션을 위한 변수
    private float animationDuration = 0.4f; // 애니메이션 시간
    private float closeBtnOffsetY = 150f; // 버튼이 사라지거나 나타날 때의 Y 오프셋
    private float panelOffsetY = 1000f; // 패널이 사라지거나 나타날 때의 Y 오프셋

    private void Awake()
    {
        tabToPanel = new Dictionary<Button, UIBase>
        {
            { characterListBtn, characterListPanel },
            { invenBtn, invenPanel },
            { gachaBtn, gachaPanel },
            { shopBtn, shopPanel },
        };

        // 패널의 원래 위치 저장
        panelOriginalPositions = new Dictionary<UIBase, Vector3>();
        foreach (var panel in tabToPanel.Values)
        {
            if (panel != null)
                panelOriginalPositions[panel] = panel.transform.localPosition;
        }

        Init();
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 모든 트윈을 정리하여 메모리 누수 방지
        closeBtn.transform.DOKill();
        foreach (var panel in tabToPanel.Values)
        {
            if (panel != null)
                panel.transform.DOKill();
        }
        Debug.Log($"{name} - {GetType().Name} Disabled", this);
    }

    void Init()
    {
        // 초기 상태: 모든 패널 끄기, 닫기 버튼 숨김
        foreach (var panel in tabToPanel.Values)
        {
            if (panel != null)
                panel.gameObject.SetActive(false);
        }
        closeBtn.gameObject.SetActive(false);

        // 탭 버튼 이벤트 연결
        foreach (var item in tabToPanel)
        {
            Button tabBtn = item.Key;
            UIBase panel = item.Value;

            if (tabBtn != null && panel != null)
                tabBtn.onClick.AddListener(() => { OnTabSelected(tabBtn, panel); });
        }

        // 공용 닫기 버튼 이벤트 연결
        closeBtn.onClick.AddListener(ClosePanelAndAnimateButtonOut);
        homeBtn.onClick.AddListener(ClosePanelAndAnimateButtonOut);
        blocker.onClick.AddListener(ClosePanelAndAnimateButtonOut);
    }

    public void OnTabSelected(Button tabBtn, UIBase panel)
    {
        panel.ResetPanel();
        bool isFirstOpen = currentOpenPanel == null;
        bool isDifferentTab = currentOpenPanel != panel;

        if (isFirstOpen)
        {
            AnimateCloseButtonIn(tabBtn);
            AnimatePanelIn(panel);
        }
        else if (isDifferentTab)
        {
            // 다른 탭으로 전환하기 전, 현재 열린 패널이 캐릭터 목록이고 편성 모드라면 유효성 검사를 수행합니다.
            if (currentOpenPanel is CharacterScrollViewUI characterListPanel && characterListPanel.isFormationMode)
            {
                if (!characterListPanel.TryDisableFormationMode())
                {
                    return; // 모드 해제에 실패했으므로, 탭 전환을 중단합니다.
                }
            }

            AnimateCloseButtonMove(tabBtn);
            AnimatePanelOut(currentOpenPanel);
            AnimatePanelIn(panel);
        }

        currentOpenPanel = panel;
        blocker.gameObject.SetActive(true);
    }

    #region Close Button Animations
    private void AnimateCloseButtonIn(Button targetTab)
    {
        closeBtn.transform.DOKill(); // 진행중인 트윈 중단
        Vector3 startPos = targetTab.transform.position - new Vector3(0, closeBtnOffsetY, 0);
        closeBtn.transform.position = startPos;
        closeBtn.gameObject.SetActive(true);

        closeBtn.transform.DOMove(targetTab.transform.position, animationDuration).SetEase(Ease.OutQuad);
    }

    private void AnimateCloseButtonMove(Button targetTab)
    {
        closeBtn.transform.DOKill(); // 진행중인 트윈 중단

        // Y축 위치를 즉시 보정합니다.
        Vector3 pos = closeBtn.transform.position;
        pos.y = targetTab.transform.position.y;
        closeBtn.transform.position = pos;

        // X축으로 수평 애니메이션을 실행합니다.
        closeBtn.transform.DOMoveX(targetTab.transform.position.x, animationDuration).SetEase(Ease.OutQuad);
    }

    private void AnimateCloseButtonOut()
    {
        closeBtn.transform.DOKill(); // 진행중인 트윈 중단
        Vector3 endPos = closeBtn.transform.position - new Vector3(0, closeBtnOffsetY, 0);

        closeBtn.transform.DOMove(endPos, animationDuration).SetEase(Ease.InQuad).OnComplete(() =>
        {
            closeBtn.gameObject.SetActive(false);
        });
    }
    #endregion

    #region Panel Animations
    private void AnimatePanelIn(UIBase panel)
    {
        panel.transform.DOKill(); // 진행중인 트윈 중단
        Vector3 originalPos = panelOriginalPositions[panel];
        Vector3 startPos = originalPos - new Vector3(0, panelOffsetY, 0);

        panel.transform.localPosition = startPos;
        panel.SetShow(); // SetActive(true) 및 RefreshUI() 호출

        panel.transform.DOLocalMove(originalPos, animationDuration).SetEase(Ease.OutBack);
    }

    private void AnimatePanelOut(UIBase panel)
    {
        if (panel == null) return;
        panel.transform.DOKill(); // 진행중인 트윈 중단

        Vector3 originalPos = panelOriginalPositions[panel];
        Vector3 endPos = originalPos - new Vector3(0, panelOffsetY, 0);

        panel.transform.DOLocalMove(endPos, animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            panel.SetHide(); // SetActive(false) 호출
        });
    }
    #endregion

    void ClosePanelAndAnimateButtonOut()
    {
        // 만약 현재 열린 패널이 캐릭터 목록이고 편성 모드 중이라면,
        // 모드 해제를 시도하고 실패 시 창을 닫지 않습니다.
        if (currentOpenPanel is CharacterScrollViewUI characterListPanel && characterListPanel.isFormationMode)
        {
            if (!characterListPanel.TryDisableFormationMode())
            {
                return; // 모드 해제에 실패했으므로, 창 닫기 프로세스를 중단합니다.
            }
        }

        AnimatePanelOut(currentOpenPanel);
        AnimateCloseButtonOut();
        currentOpenPanel = null;
        blocker.gameObject.SetActive(false);
    }
}


