using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PartyManager로부터 파티 준비 완료 신호를 받아 체력바를 초기화합니다.
/// </summary>
public class AllHealthBarsPanel : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("씬에 미리 배치한 HealthBarDisplay 오브젝트들을 여기에 등록하세요.")]
    [SerializeField] private List<HealthBarDisplay> healthBars = new List<HealthBarDisplay>();

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 파티가 준비되었다는 신호를 받으면 SetupHealthBars 함수를 호출하도록 등록합니다.
        PartyManager.OnPartyReady += SetupHealthBars;
        Debug.Log("[AllHealthBarsPanel] PartyManager의 OnPartyReady 신호를 기다립니다.");

        // 만약 이 패널이 활성화되기 전에 PartyManager가 이미 준비를 마쳤다면,
        // 놓친 신호를 보충하기 위해 직접 파티 목록을 가져와 체력바를 설정합니다.
        if (PartyManager.Instance != null && PartyManager.Instance.IsPartyReady)
        {
            Debug.Log("[AllHealthBarsPanel] PartyManager가 이미 준비 완료 상태입니다. 직접 체력바를 설정합니다.");
            SetupHealthBars(PartyManager.Instance.ActiveParty);
        }
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

        // 🔥 Destroy된 오브젝트 정리
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

            healthBars[i].gameObject.SetActive(true);
            healthBars[i].Initialize(partyCharacters[i]);
            Debug.Log($"체력바 {i}번에 '{partyCharacters[i].name}' 캐릭터를 연결했습니다.");
        }

        // 만약 체력바 개수가 부족하면 경고 로그
        if (partyCharacters.Count > healthBars.Count)
        {
            Debug.LogWarning($"파티 캐릭터 수({partyCharacters.Count})가 체력바 수({healthBars.Count})보다 많습니다. 일부 캐릭터는 체력바가 없습니다.");
        }
    }
}
