using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 버튼을 통해 캐릭터의 스킬 사용을 테스트하기 위한 클래스입니다.
/// </summary>
public class SkillTest : MonoBehaviour
{
    [Header("테스트 대상")]
    [Tooltip("스킬을 사용할 아군 캐릭터의 이름(Name)을 입력하세요.")]
    public string characterNameToTest;

    [Tooltip("사용할 스킬의 인덱스 (캐릭터의 skills 리스트 기준)")]
    public int skillIndex = 0;

    public Button btn;
    private void Start()
    {
        btn.onClick.AddListener(UseSkill);
    }
    /// <summary>
    /// UI 버튼의 OnClick 이벤트에 연결하여 호출할 메소드입니다.
    /// </summary>
    public void UseSkill()
    {
        // 1. 필수 정보 확인
        if (string.IsNullOrEmpty(characterNameToTest))
        {
            Debug.LogError("[SkillTest] 스킬을 사용할 캐릭터의 이름(characterNameToTest)이 입력되지 않았습니다!");
            return;
        }

        GameObject charGO = GameObject.Find(characterNameToTest);
        if (charGO == null)
        {
            Debug.LogError($"[SkillTest] 씬에서 '{characterNameToTest}' 이름의 캐릭터를 찾을 수 없습니다.");
            return;
        }

        CombatCharacter character = charGO.GetComponent<CombatCharacter>();
        if (character == null)
        {
            Debug.LogError($"[SkillTest] '{characterNameToTest}' 오브젝트에서 CombatCharacter 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        // 2. 스킬 정보 가져오기
        if (skillIndex < 0 || skillIndex >= character.skills.Count)
        {
            Debug.LogError($"[SkillTest] 잘못된 스킬 인덱스({skillIndex})입니다. '{character.charName}' 캐릭터는 {character.skills.Count}개의 스킬을 가지고 있습니다.");
            return;
        }

        SkillSO skillToUse = character.skills[skillIndex];
        if (skillToUse == null)
        {
            Debug.LogError($"[SkillTest] 인덱스 {skillIndex}에 해당하는 스킬이 null입니다.");
            return;
        }

        // 3. 스킬 직접 호출 (타겟 없이)
        Debug.Log($"[SkillTest] '{character.charName}'이(가) '{skillToUse.skillName}' 버프 스킬 사용을 시도합니다.");
        skillToUse.Use(character, null);
        Debug.Log($"[SkillTest] 스킬 사용 완료.");
    }
}
