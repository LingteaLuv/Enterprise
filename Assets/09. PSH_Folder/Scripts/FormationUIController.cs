using System.Collections.Generic;
using System.Numerics;
using _05._CSJ_Folder.Scripts.Quest;
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

    // 이 UI가 활성화될 때 호출됩니다 (편성 모드 시작 시).
    void OnEnable()
    {
        // FormationManager의 이벤트 구독: 임시 편성이 변경될 때마다 UI를 새로고침합니다.
        if (FormationManager.Instance != null)
        {
            FormationManager.Instance.OnTempFormationChanged += RefreshFormationDisplay;
        }

        // PlayerDataManager의 이벤트도 계속 구독하여 캐릭터 자체의 스탯 변경(레벨업 등)을 반영합니다.
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnCharacterDataUpdated += HandleCharacterDataUpdated;
        }

        //RefreshFormationDisplay(); // 활성화될 때 한 번 새로고침
    }

    // 비활성화될 때 이벤트 구독을 해제합니다.
    void OnDisable()
    {
        if (FormationManager.Instance != null)
        {
            FormationManager.Instance.OnTempFormationChanged -= RefreshFormationDisplay;
        }
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnCharacterDataUpdated -= HandleCharacterDataUpdated;
        }
    }

    private void Start()
    {
        // 자동 편성 버튼을 누르면 AutoFormating 함수가 실행되도록 연결합니다.
        if (autoFormationButton != null)
        {
            autoFormationButton.onClick.AddListener(AutoFormating);
        }
        
        TutorialTargets.Register("AutoFormationButton", autoFormationButton.transform as RectTransform);
    }

    private void HandleCharacterDataUpdated(PlayerCharacterData updatedCharacter)
    {
        // 특정 캐릭터의 데이터가 업데이트되면 전체 편성 UI를 새로고침합니다.
        RefreshFormationDisplay();
    }

    /// <summary>
    /// FormationManager의 임시 편성 정보를 기반으로 UI를 새로고침합니다.
    /// </summary>
    public void RefreshFormationDisplay()
    {
        if (FormationManager.Instance == null) return;

        var currentFormation = FormationManager.Instance.TempFormation;

        // 각 포지션 슬롯에 해당하는 캐릭터 리스트를 가져와 Setup 함수를 호출합니다.
        List<PlayerCharacterData> characters;

        if (frontSlot != null)
        {
            currentFormation.TryGetValue(CrewRole.Deckhand, out characters);
            frontSlot.Setup(characters ?? new List<PlayerCharacterData>());
        }
        if (middleSlot != null)
        {
            currentFormation.TryGetValue(CrewRole.Sailor, out characters);
            middleSlot.Setup(characters ?? new List<PlayerCharacterData>());
        }
        if (rearSlot != null)
        {
            currentFormation.TryGetValue(CrewRole.Cook, out characters);
            rearSlot.Setup(characters ?? new List<PlayerCharacterData>());
        }
        if (lastSlot != null)
        {
            currentFormation.TryGetValue(CrewRole.Captain, out characters);
            lastSlot.Setup(characters ?? new List<PlayerCharacterData>());
        }

        // 임시 편성에 따라 팀 전투력을 실시간으로 다시 계산합니다.
        if (teamBattlePowerText != null)
        {
            BigInteger tempTeamPower = 0;
            foreach (var list in currentFormation.Values)
            {
                foreach (var character in list)
                {
                    tempTeamPower += character.battlePower;
                }
            }
            teamBattlePowerText.text = $"현상금: {DataUtility.FormatNumber(tempTeamPower)}";
        }
    }

    public void AutoFormating()
    {
        FormationManager.Instance.AutoFormTeam();
        PopManager.Instance.ShowOKPopup("가장 강력한 동료들로 자동 편성되었습니다!", "확인");
    }
}

