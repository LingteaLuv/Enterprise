using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private Coroutine battleRoutine;

    [Header("스폰 프리팹")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("전투 필드")]
    [SerializeField] private List<BattleField> battleFields;

    [Header("스폰 수 설정")]
    [SerializeField] private int baseEnemyCount = 3;
    [SerializeField] private int growthPerStage = 1;

    private GameObject currentPlayer;
    private List<GameObject> spawnedEnemies = new();

    private int currentStageIndex = 0;

    private void Awake()
    {
        Debug.Log("BattleManager.Awake 호출됨");
        Instance = this;
    }

    /// <summary>
    /// 외부에서 호출되는 전투 시작 메서드
    /// </summary>
    public void StartBattle(int stageIndex)
    {
        currentStageIndex = stageIndex;
        battleRoutine = StartCoroutine(BattleRoutine());
        Debug.Log("battleRoutine 시작됨");
    }

    /// <summary>
    /// 전투의 전체 흐름을 담당하는 메인 코루틴
    /// </summary>
    private IEnumerator BattleRoutine()
    {
        Debug.Log("전투 시작");

        var field = battleFields[currentStageIndex];

        // 1. 유닛 스폰
        SpawnPlayer(field);
        SpawnEnemies(field, currentStageIndex);

        yield return new WaitForSeconds(0.5f); // 연출 시간

        // 2. 전투 루프
        while (true)
        {
            if (AllEnemiesDefeated())
                break;

            AutoControlUnits(); // 유닛 자동 행동 (구현 예정)
            yield return null;
        }

        yield return new WaitForSeconds(1f); // 종료 연출 대기

        battleRoutine = null;
        IslandStageManager.Instance.OnBattleComplete();
    }

    private void SpawnPlayer(BattleField field)
    {
        if (currentPlayer != null)
            Destroy(currentPlayer);

        currentPlayer = Instantiate(playerPrefab, field.PlayerSpawnPoint.position, Quaternion.identity);
    }

    private void SpawnEnemies(BattleField field, int stageIndex)
    {
        int count = baseEnemyCount + (stageIndex * growthPerStage);
        var spawnPoints = field.EnemySpawnPoints;
        count = Mathf.Min(count, spawnPoints.Count);

        for (int i = 0; i < count; i++)
        {
            var enemy = Instantiate(enemyPrefab, spawnPoints[i].position, Quaternion.identity);
            enemy.tag = "Enemy"; // 전투 종료 체크용
            spawnedEnemies.Add(enemy);
        }
    }

    public void OnEnemyDead(GameObject enemy)
    {
        if (spawnedEnemies.Contains(enemy))
            spawnedEnemies.Remove(enemy);

        if (AllEnemiesDefeated())
        {
            Debug.Log("모든 적 제거 → 전투 종료 예정");
            // 전투 루프에서 자동 감지됨 → 따로 종료 호출 안 해도 됨
        }
    }

    private bool AllEnemiesDefeated()
    {
        return spawnedEnemies.Count == 0;
    }

    private void AutoControlUnits()
    {
        // TODO: 추후 유닛 FSM이나 AI 구현 시 연결
    }
}
