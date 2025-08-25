using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
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

    private bool isbattleover = false;

    private void Awake()
    {
        Debug.Log("BattleManager.Awake 호출됨");
        Instance = this;
    }

    private void Update()
    {
        // 임시 전투 종료 후 다음 섬 넘어가기위한 키 . 차후 삭제 예정
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isbattleover)
        {
            isbattleover = true;
            battleRoutine = null;
            IslandStageManager.Instance.OnBattleComplete();
            ClearEnemies();
        }
    }

    /// <summary>
    /// 외부에서 호출되는 전투 시작 메서드
    /// </summary>
    public void StartBattle(int stageIndex)
    {
        isbattleover = false;
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

      //  // 임시 전투 종료 후 다음 섬 넘어가기위한 키 . 차후 삭제 예정
      //  if(Keyboard.current.spaceKey.wasPressedThisFrame)
      //  {
      //      battleRoutine = null;
      //      Debug.Log("스페이스 버튼으로 전투 스킵 후 다음 섬 이동.");
      //      IslandStageManager.Instance.OnBattleComplete();
      //  }

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

        if (isbattleover == true)
        {
            yield break;
        }
        else
        {
            IslandStageManager.Instance.OnBattleComplete();
        }
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

    private void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        spawnedEnemies.Clear();
    }
}
