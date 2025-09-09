
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 단일 캐릭터의 체력바 UI를 관리합니다.
/// 아이콘, 슬라이더 등의 UI 요소를 업데이트합니다.
/// </summary>
public class HealthBarDisplay : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private Image characterIcon;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private HealthSystem healthSystem;

    /// <summary>
    /// 특정 CombatCharacter를 대상으로 체력바를 초기화합니다.
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

        // HealthSystem 참조 및 이벤트 구독
        healthSystem = character.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += UpdateHealthBar;
            // 초기 체력으로 한 번 업데이트
            UpdateHealthBar(healthSystem.currentHealth, healthSystem.maxHealth);
        }
        else
        {
            Debug.LogError($"[HealthBarDisplay] {character.name}에서 HealthSystem을 찾을 수 없습니다.");
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// HealthSystem의 OnHealthChanged 이벤트에 의해 호출됩니다.
    /// </summary>
    private void UpdateHealthBar(float current, float max)
    {
        if (healthSlider != null)
        {
            // max가 0일 경우의 나눗셈 오류 방지
            healthSlider.value = (max > 0) ? (current / max) : 0;
            healthText.text = DataUtility.FormatNumber(current);
        }
    }

    /// <summary>
    /// 이 오브젝트가 파괴될 때 이벤트 구독을 해제합니다.
    /// </summary>
    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= UpdateHealthBar;
        }
    }
}
