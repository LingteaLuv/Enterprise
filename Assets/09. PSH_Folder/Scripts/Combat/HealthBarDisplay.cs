using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 단일 캐릭터의 체력바 및 스킬 쿨타임 UI를 관리합니다.
/// </summary>
public class HealthBarDisplay : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image characterIcon;
    [SerializeField] private Image healthFillImage; // 체력바 채우기 이미지
    [SerializeField] private TextMeshProUGUI healthText; // 체력 숫자를 표시할 텍스트
    [SerializeField] private TextMeshProUGUI nameText; // 캐릭 이름을 표시할 텍스트
    [SerializeField] private Image skillCooldownImage; // 스킬 쿨타임 표시 이미지
    [SerializeField] private TextMeshProUGUI skillCooldownText; // 스킬 쿨타임 텍스트
    [SerializeField] private TextMeshProUGUI charLevelText;

    private HealthSystem healthSystem;
    private MeleeCharacter meleeCharacter;
    private Coroutine cooldownCoroutine;
    private PlayerCharacterData _playerCharacterData; // 레벨 업데이트를 위한 PlayerCharacterData 참조

    /// <summary>
    /// 캐릭터 레벨 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateCharacterLevelText(int newLevel)
    {
        if (charLevelText != null)
        {
            charLevelText.text = $"레벨 {newLevel}";
        }
    }

    /// <summary>
    /// 특정 CombatCharacter를 대상으로 UI를 초기화합니다.
    /// </summary>
    public void Initialize(CombatCharacter character)
    {
        if (character == null)
        {
            Debug.LogError("[HealthBarDisplay] 대상 캐릭터가 null입니다.");
            gameObject.SetActive(false);
            return;
        }

        // 아이콘 설정
        if (characterIcon != null && character.CharacterStats?.characterdata?.characterSprite != null)
        {
            characterIcon.sprite = character.CharacterStats.characterdata.characterSprite;
        }

        // 이름 설정
        if (nameText != null && character.CharacterStats?.characterdata?.characterName != null)
        {
            nameText.text = character.CharacterStats.characterdata.characterName;
        }

        // 레벨 설정
        _playerCharacterData = character.CharacterStats; // PlayerCharacterData 참조 저장
        if (_playerCharacterData != null)
        {
            _playerCharacterData.Level.OnChanged += UpdateCharacterLevelText; // 레벨 변경 이벤트 구독
            UpdateCharacterLevelText(_playerCharacterData.Level.Value); // 초기 레벨 텍스트 설정
        }
        else
        {
            Debug.LogError($"[HealthBarDisplay] {character.name}에서 PlayerCharacterData를 찾을 수 없습니다.");
        }

        // HealthSystem 참조 및 이벤트 구독
        healthSystem = character.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(healthSystem.currentHealth, healthSystem.maxHealth);
        }
        else
        {
            Debug.LogError($"[HealthBarDisplay] {character.name}에서 HealthSystem을 찾을 수 없습니다.");
        }

        // MeleeCharacter 참조 및 스킬 사용 이벤트 구독
        meleeCharacter = character.GetComponent<MeleeCharacter>();
        if (meleeCharacter != null)
        {
            meleeCharacter.OnSkillUsed += HandleSkillUsed;
        }

        // 쿨타임 UI 초기화
        if (skillCooldownImage != null)
        {
            skillCooldownImage.fillAmount = 0;
        }
        if (skillCooldownText != null)
        {
            skillCooldownText.text = "";
        }
    }

    /// <summary>
    /// HealthSystem의 OnHealthChanged 이벤트에 의해 호출됩니다.
    /// </summary>
    private void UpdateHealthBar(float current, float max)
    {
        if (healthFillImage != null)
        {
            // max가 0일 경우의 나눗셈 오류 방지
            healthFillImage.fillAmount = (max > 0) ? (current / max) : 0;
        }

        // 체력 숫자 텍스트 업데이트
        if (healthText != null)
        {
            healthText.text = $"{DataUtility.FormatNumber(current)} / {DataUtility.FormatNumber(max)}";
        }
    }

    /// <summary>
    /// MeleeCharacter의 OnSkillUsed 이벤트에 의해 호출됩니다.
    /// </summary>
    private void HandleSkillUsed(SkillSO skill)
    {
        if (skill != null && skill.cooldown > 0)
        {
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
            }
            cooldownCoroutine = StartCoroutine(CooldownCoroutine(skill.cooldown));
        }
    }

    /// <summary>
    /// 스킬 쿨타임 UI를 업데이트하는 코루틴입니다.
    /// </summary>
    private IEnumerator CooldownCoroutine(float cooldown)
    {
        if (skillCooldownImage == null) yield break;

        float timer = cooldown;
        skillCooldownImage.fillAmount = 1;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            skillCooldownImage.fillAmount = timer / cooldown;
            if (skillCooldownText != null)
            {
                skillCooldownText.text = Mathf.Ceil(timer).ToString("F0");
            }
            yield return null;
        }

        skillCooldownImage.fillAmount = 0;
        if (skillCooldownText != null)
        {
            skillCooldownText.text = "";
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= UpdateHealthBar;
        }
        if (meleeCharacter != null)
        {
            meleeCharacter.OnSkillUsed -= HandleSkillUsed;
        }
        if (_playerCharacterData != null)
        {
            _playerCharacterData.Level.OnChanged -= UpdateCharacterLevelText; // 레벨 변경 이벤트 구독 해제
        }
    }
}
