using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormationUIController : MonoBehaviour
{
    [Header("편성 슬롯")]
    public FormationSlotUI frontSlot; // Deckhand
    public FormationSlotUI middleSlot; // Sailor
    public FormationSlotUI rearSlot; // Cook
    public FormationSlotUI lastSlot; // Captain

    [Header("UI 요소")]
    public Button autoFormationButton; // 자동 편성 버튼
    public TextMeshProUGUI teamBattlePowerText; // 팀 전투력 텍스트

    private void Start()
    {
        autoFormationButton.onClick.AddListener(AutoFormating);
        gameObject.SetActive(false); // 초기에는 비활성화
    }
    void OnEnable()
    {
        // PlayerDataManager의 이벤트 구독
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnOwnedCharactersChanged += RefreshFormationDisplay;
            PlayerDataManager.Instance.OnCharacterDataUpdated += HandleCharacterDataUpdated;
            Debug.Log("[FUC] PlayerDataManager 이벤트 구독 성공.");
        }
        else
        {
            Debug.LogWarning("FormationUIController: PlayerDataManager.Instance is null. Cannot subscribe to events.");
        }

        RefreshFormationDisplay(); // 활성화될 때 한 번 새로고침
    }

    void OnDisable()
    {
        // 이벤트 구독 해제
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnOwnedCharactersChanged -= RefreshFormationDisplay;
            // OnCharacterDataUpdated 이벤트 구독 해제
            PlayerDataManager.Instance.OnCharacterDataUpdated -= HandleCharacterDataUpdated;
        }
    }

    private void HandleCharacterDataUpdated(PlayerCharacterData updatedCharacter)
    {
        Debug.Log($"[FUC] HandleCharacterDataUpdated 호출됨: {updatedCharacter.characterdata.characterName}");
        // 특정 캐릭터의 데이터가 업데이트되면 전체 편성 UI를 새로고침합니다.
        RefreshFormationDisplay();
    }

    public void RefreshFormationDisplay()
    {
        Debug.Log("[FUC] RefreshFormationDisplay 호출됨.");
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager.Instance is null. Cannot refresh formation display.");
            return;
        }

        // 각 포지션 슬롯에 해당하는 캐릭터 리스트를 가져와 Setup 함수를 호출합니다.
        List<PlayerCharacterData> characters;

        if (frontSlot != null)
        {
            PlayerDataManager.Instance.formation.TryGetValue(CrewRole.Deckhand, out characters);
            frontSlot.Setup(characters ?? new List<PlayerCharacterData>());
        }
        if (middleSlot != null)
        {
            PlayerDataManager.Instance.formation.TryGetValue(CrewRole.Sailor, out characters);
            middleSlot.Setup(characters ?? new List<PlayerCharacterData>());
        }
        if (rearSlot != null)
        {
            PlayerDataManager.Instance.formation.TryGetValue(CrewRole.Cook, out characters);
            rearSlot.Setup(characters ?? new List<PlayerCharacterData>());
        }
        if (lastSlot != null)
        {
            PlayerDataManager.Instance.formation.TryGetValue(CrewRole.Captain, out characters);
            lastSlot.Setup(characters ?? new List<PlayerCharacterData>());
        }

        // 팀 전투력 UI 갱신
        if (teamBattlePowerText != null)
        {
            teamBattlePowerText.text = $"팀 전투력: {DataUtility.FormatNumber(PlayerDataManager.Instance.teamBattlePower)}";
        }
    }

    public void AutoFormating()
    {
        string str = PlayerDataManager.Instance.AutoFormTeam() ? "성공" : "실패";
        Debug.Log("자동 편성" + str);
    }
}
