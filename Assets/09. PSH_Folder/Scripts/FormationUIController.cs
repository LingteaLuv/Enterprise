using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormationUIController : MonoBehaviour
{
    public FormationSlotUI frontSlot; // Deckhand
    public FormationSlotUI middleSlot; // Sailor
    public FormationSlotUI rearSlot; // Cook
    public FormationSlotUI lastSlot; // Captain

    public Button autoFormationButton; // 자동 편성 버튼

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

    /// <summary>
    /// PlayerDataManager.OnCharacterDataUpdated 이벤트를 처리하는 함수입니다.
    /// </summary>
    /// <param name="updatedCharacter">업데이트된 캐릭터 데이터 (여기서는 직접 사용하지 않고 전체 UI를 갱신)</param>
    private void HandleCharacterDataUpdated(PlayerCharacterData updatedCharacter)
    {
        Debug.Log($"[FUC] HandleCharacterDataUpdated 호출됨: {updatedCharacter.characterdata.characterName}");
        // 특정 캐릭터의 데이터가 업데이트되면 전체 편성 UI를 새로고침합니다.
        RefreshFormationDisplay();
    }

    /// <summary>
    /// PlayerDataManager의 편성 데이터를 기반으로 UI를 갱신합니다.
    /// </summary>
    public void RefreshFormationDisplay()
    {
        Debug.Log("[FUC] RefreshFormationDisplay 호출됨.");
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager.Instance is null. Cannot refresh formation display.");
            return;
        }

        // 각 포지션 슬롯에 해당하는 캐릭터 리스트를 가져와 Setup 함수를 호출합니다.
        // TryGetValue를 사용하여 해당 포지션에 캐릭터가 없을 경우 빈 리스트를 전달합니다.
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
    }

    public void AutoFormating()
    {        
        string str = PlayerDataManager.Instance.AutoFormTeam() ? "성공" : "실패";
        Debug.Log("자동 편성"+ str);
    }
}