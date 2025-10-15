using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest; // OrderBy를 사용하기 위해 추가해주세요!
using System.Linq; // OrderBy를 사용하기 위해 추가해주세요!
using DG.Tweening; // 닷트윈 사용을 위해 추가
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PartyManager로부터 파티 준비 완료 신호를 받아 체력바를 초기화합니다.
/// </summary>
public class AllHealthBarsPanel : MonoBehaviour
{
    private List<HealthBarDisplay> healthBars = new List<HealthBarDisplay>();

    [Header("패널 이동 설정")]
    [SerializeField] private Button toggleMoveButton; // 이동 토글 버튼
    [SerializeField] private float moveDistance = 300f; // 이동할 거리
    [SerializeField] private float moveDuration = 0.5f; // 이동 시간

    private bool isMoved = false; // 현재 패널이 이동했는지 여부
    private Vector3 originalPosition; // 원래 위치

    // Start 대신 Awake를 사용해서 OnEnable보다 먼저 실행되도록 보장합니다.
    private void Awake()
    {
        // 이 오브젝트의 모든 자식들 중에서 HealthBarDisplay 컴포넌트를 찾아서 리스트에 추가합니다.
        // true를 인자로 주면 비활성화된 자식 오브젝트도 모두 찾아옵니다.
        healthBars = GetComponentsInChildren<HealthBarDisplay>(true).ToList();

        // (선택사항) 체력바들을 이름순으로 정렬해서 항상 순서가 같도록 보장할 수 있어요.
        // 예: HealthBar_0, HealthBar_1, HealthBar_2 ...
        healthBars = healthBars.OrderBy(hb => hb.name).ToList();

        Debug.Log($"[AllHealthBarsPanel] 자식 오브젝트에서 {healthBars.Count}개의 체력바를 찾아 리스트에 등록했습니다.");

        TutorialTargets.Register("HpBar", transform as RectTransform);

        // 패널 이동 기능 초기화
        originalPosition = transform.position;
        if (toggleMoveButton != null)
        {
            toggleMoveButton.onClick.AddListener(TogglePanelPosition);
        }
    }

    private void Start()
    {
        // 이 로직은 그대로 두거나, Awake에서 처리해도 좋아요.
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 파티가 준비되었다는 신호를 받으면 SetupHealthBars 함수를 호출하도록 등록합니다.
        PartyManager.OnPartyReady += SetupHealthBars;
        Debug.Log("[AllHealthBarsPanel] PartyManager의 OnPartyReady 신호를 기다립니다.");

        // 만약 이 패널이 활성화되기 전에 PartyManager가 이미 준비를 마쳤다면,
        // 놓친 신호를 보충하기 위해 직접 파티 목록을 가져와 체력바를 설정합니다.
        /*if (PartyManager.Instance != null && PartyManager.Instance.IsPartyReady)
        {
            Debug.Log("[AllHealthBarsPanel] PartyManager가 이미 준비 완료 상태입니다. 직접 체력바를 설정합니다.");
            SetupHealthBars(PartyManager.Instance.ActiveParty);
        }*/
    }

    private void OnDisable()
    {
        // 오브젝트가 사라질 때 신호 수신을 중단합니다. (메모리 누수 방지)
        PartyManager.OnPartyReady -= SetupHealthBars;
    }

    /// <summary>
    /// PartyManager로부터 캐릭터 리스트를 받아 체력바를 설정합니다.
    /// </summary>
    private void SetupHealthBars(List<CombatCharacter> partyCharacters)
    {
        Debug.Log("[AllHealthBarsPanel] OnPartyReady 신호 수신! 체력바 설정을 시작합니다.");

        // Awake에서 리스트를 새로 채우기 때문에 이 코드는 이제 필수는 아니지만,
        // 만약을 위한 안전장치로 남겨둬도 좋아요.
        healthBars.RemoveAll(hb => hb == null);

        // 등록된 체력바 개수만큼 반복 (단, 파티 수보다 많으면 파티 수까지만)
        int count = Mathf.Min(healthBars.Count, partyCharacters.Count);

        for (int i = 0; i < count; i++)
        {
            if (healthBars[i] == null)
            {
                Debug.LogWarning($"체력바 {i}번이 Destroy되어 null 상태입니다. 스킵합니다.");
                continue;
            }

            Debug.Log($"체력바 {i}번에 '{partyCharacters[i].name}' 캐릭터를 연결했습니다.");
            healthBars[i].gameObject.SetActive(true);
            healthBars[i].Initialize(partyCharacters[i]);
        }

        // 만약 체력바 개수가 부족하면 경고 로그
        if (partyCharacters.Count > healthBars.Count)
        {
            Debug.LogWarning($"파티 캐릭터 수({partyCharacters.Count})가 체력바 수({healthBars.Count})보다 많습니다. 일부 캐릭터는 체력바가 없습니다.");
        }
    }

    /// <summary>
    /// 패널의 위치를 토글하는 애니메이션을 재생합니다.
    /// </summary>
    private void TogglePanelPosition()
    {
        // isMoved 상태를 반전시킵니다.
        isMoved = !isMoved;

        if (isMoved)
        {
            // 왼쪽으로 이동
            transform.DOMoveX(originalPosition.x - moveDistance, moveDuration);
        }
        else
        {
            // 원래 위치로 복귀
            transform.DOMoveX(originalPosition.x, moveDuration);
        }
    }
}