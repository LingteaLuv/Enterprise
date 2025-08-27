using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    private Coroutine battleRoutine;

    [SerializeField] private Button _skipBtn;
    [SerializeField] private CameraFollow cameraFollow;


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

    private void Start()
    {
        _skipBtn.onClick.AddListener(Skip);
    }

    private void Skip()
    {
        if (isbattleover) return; // 이미 스킵됨 → 무시

        isbattleover = true;

        // 코루틴 중복 방지
        if (battleRoutine != null)
        {
            StopCoroutine(battleRoutine);
            battleRoutine = null;
        }

        // 클리어 처리
        IslandStageManager.Instance.OnBattleComplete();
        ClearEnemies();
        ClearPlayer();
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

           // AutoControlUnits(); // 유닛 자동 행동 (구현 예정)
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
            ClearPlayer();
            ClearEnemies(); // 혹시라도 남아있으면 같이 제거
            IslandStageManager.Instance.OnBattleComplete();
        }
    }

    private void SpawnPlayer(BattleField field)
    {
        if (currentPlayer != null)
            Destroy(currentPlayer);

        currentPlayer = Instantiate(playerPrefab, field.PlayerSpawnPoint.position, Quaternion.identity);

        if (cameraFollow != null)
            cameraFollow.SetTarget(currentPlayer.transform);
    }

    private void SpawnEnemies(BattleField field, int stageIndex)
    {
        int count = baseEnemyCount + (stageIndex * growthPerStage);
        var spawnPoints = field.EnemySpawnPoints;
       // count = Mathf.Min(count, spawnPoints.Count);

        for (int i = 0; i < count; i++)
        {
            int randIndex = Random.Range(0, spawnPoints.Count);
            var enemy = Instantiate(enemyPrefab, spawnPoints[randIndex].position, Quaternion.identity);
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

  //  private void AutoControlUnits()
  //  {
  //      // TODO: 추후 유닛 FSM이나 AI 구현 시 연결
  //  }

    private void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        spawnedEnemies.Clear();
    }

    private void ClearPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }
    }
}
