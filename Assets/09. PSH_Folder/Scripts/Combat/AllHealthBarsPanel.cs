
using UnityEngine;

/// <summary>
/// 씬에 있는 모든 CombatCharacter의 체력바를 생성하고 관리하는 패널입니다.
/// </summary>
public class AllHealthBarsPanel : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private GameObject healthBarPrefab; // HealthBarDisplay 스크립트를 가진 프리팹
    [SerializeField] private Transform container;      // 프리팹이 생성될 부모 컨테이너

    void Start()
    {
        InitializePanel();
    }

    /// <summary>
    /// 씬의 모든 전투 캐릭터를 찾아 체력바를 생성합니다.
    /// </summary>
    private void InitializePanel()
    {
        if (healthBarPrefab == null || container == null)
        {
            Debug.LogError("[AllHealthBarsPanel] 체력바 프리팹 또는 컨테이너가 설정되지 않았습니다.");
            return;
        }

        // 참고: FindObjectsOfType은 씬이 크거나 캐릭터가 많을 경우 성능에 부담을 줄 수 있습니다.
        // 더 좋은 방법은 CombatCharacter가 생성될 때마다 리스트에 등록하는 중앙 관리자를 두는 것입니다.
        CombatCharacter[] allCharacters = FindObjectsOfType<CombatCharacter>();

        Debug.Log($"{allCharacters.Length}개의 전투 캐릭터를 찾았습니다. 체력바를 생성합니다.");

        foreach (CombatCharacter character in allCharacters)
        {
            GameObject healthBarObject = Instantiate(healthBarPrefab, container);
            HealthBarDisplay display = healthBarObject.GetComponent<HealthBarDisplay>();

            if (display != null)
            {
                display.Initialize(character);
            }
            else
            {
                Debug.LogError($"{healthBarPrefab.name} 프리팹에 HealthBarDisplay 스크립트가 없습니다.");
                Destroy(healthBarObject); // 스크립트가 없으면 오브젝트 삭제
            }
        }
    }
}
