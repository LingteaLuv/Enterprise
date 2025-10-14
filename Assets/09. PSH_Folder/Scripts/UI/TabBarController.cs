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
    [SerializeField] GameObject closeFrame;
    [SerializeField] Button closeBtn;
    [SerializeField] Button homeBtn;
    [SerializeField] Button blocker;

    [Header("애니메이션 블로커")]
    [SerializeField] GameObject animationBlocker; // 애니메이션 중 입력을 막을 UI
    [SerializeField] float blockDuration = 0.3f; // 블로커 활성화 시간

    private Dictionary<Button, UIBase> tabToPanel;
    private Dictionary<UIBase, Vector3> panelOriginalPositions;
    private UIBase currentOpenPanel;

    // 애니메이션을 위한 변수
    [SerializeField] private float animationDuration = 0.2f; // 애니메이션 시간
    [SerializeField] private float closeBtnOffsetY = 250f; // 버튼이 사라지거나 나타날 때의 Y 오프셋
    [SerializeField] private float panelOffsetY = 1000f; // 패널이 사라지거나 나타날 때의 Y 오프셋

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
        closeFrame.transform.DOKill();
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

        if (animationBlocker != null) animationBlocker.SetActive(false); // 블로커 비활성화

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
        // 애니메이션 중복 실행 방지를 위해 블로커 활성화
        if (animationBlocker != null)
        {
            animationBlocker.SetActive(true);
            DOVirtual.DelayedCall(blockDuration, () => animationBlocker.SetActive(false));
        }

        // 기존 트윈 중단
        closeFrame.transform.DOKill();

        // 프레임을 선택된 탭의 위치로 애니메이션과 함께 이동
        closeFrame.transform.DOMove(tabBtn.transform.position, animationDuration).SetEase(Ease.OutQuad);

        panel.ResetPanel();
        bool isFirstOpen = currentOpenPanel == null;
        bool isDifferentTab = currentOpenPanel != panel;

        if (isFirstOpen)
        {
            AnimateCloseButtonIn();
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

            AnimatePanelOut(currentOpenPanel);
            AnimatePanelIn(panel);
        }

        currentOpenPanel = panel;
        blocker.gameObject.SetActive(true);
    }

    #region Close Button Animations
    private void AnimateCloseButtonIn()
    {
        closeBtn.transform.DOKill(); // 진행중인 트윈 중단
        closeBtn.gameObject.SetActive(true);

        // 부모인 closeFrame을 기준으로 로컬 애니메이션을 실행합니다.
        closeBtn.transform.localPosition = new Vector3(0, -closeBtnOffsetY, 0);
        closeBtn.transform.DOLocalMove(Vector3.zero, animationDuration).SetEase(Ease.OutQuad);
    }

    private void AnimateCloseButtonOut()
    {
        closeBtn.transform.DOKill(); // 진행중인 트윈 중단

        // 부모인 closeFrame을 기준으로 로컬 애니메이션을 실행합니다.
        Vector3 endPos = new Vector3(0, -closeBtnOffsetY, 0);

        closeBtn.transform.DOLocalMove(endPos, animationDuration).SetEase(Ease.InQuad).OnComplete(() =>
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
        // 애니메이션 중복 실행 방지를 위해 블로커 활성화
        if (animationBlocker != null)
        {
            animationBlocker.SetActive(true);
            DOVirtual.DelayedCall(blockDuration, () => animationBlocker.SetActive(false));
        }

        // 만약 현재 열린 패널이 캐릭터 목록이고 편성 모드 중이라면,
        // 모드 해제를 시도하고 실패 시 창을 닫지 않습니다.
        if (currentOpenPanel is CharacterScrollViewUI characterListPanel && characterListPanel.isFormationMode)
        {
            if (!characterListPanel.TryDisableFormationMode())
            {
                return; // 모드 해제에 실패했으므로, 창 닫기 프로세스를 중단합니다.
            }
        }

        // 기존 트윈 중단
        closeFrame.transform.DOKill();

        // 프레임을 홈 버튼 위치로 애니메이션과 함께 이동
        closeFrame.transform.DOMove(homeBtn.transform.position, animationDuration).SetEase(Ease.OutQuad);

        AnimatePanelOut(currentOpenPanel);
        AnimateCloseButtonOut();
        currentOpenPanel = null;
        blocker.gameObject.SetActive(false);
    }
}


