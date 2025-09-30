using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BossBattleManager : MonoBehaviour
{
    [Header("스폰 포인트")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("프리팹")]
    [SerializeField] private GameObject bossPrefab;

    private List<GameObject> currentPlayers = new();
    private GameObject currentBoss;

    private CameraFollow cameraFollow;

    public static bool IsBossBattle = false;

    private void Start()
    {
        IsBossBattle = true;
        cameraFollow = Camera.main?.GetComponent<CameraFollow>();

        SpawnPlayers();
        SpawnBoss();
    }

    private void SpawnPlayers()
    {
        var party = PartyManager.Instance.GetAllPartyMembers();

        foreach (var character in party.Select((ch, i) => new { ch, i }))
        {
            var c = character.ch;
            c.Initialize(c.CharacterStats);
            c.transform.position = playerSpawnPoint.position + new Vector3(character.i * 0.3f, 0, 0);
            c.gameObject.SetActive(true);

            var fsm = c.GetComponent<BaseCharacterFSM>();
            if (fsm != null)
            {
                fsm.enabled = true;
                fsm.ChangeStateIdleForce();
            }

            currentPlayers.Add(c.gameObject);
        }

        if (cameraFollow != null)
            cameraFollow.SetTargets(currentPlayers.Select(p => p.transform).ToList());
    }

    private void SpawnBoss()
    {
        currentBoss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
    }

    public void OnBossBattleEnd()
    {
        IsBossBattle = false;
        // 다음 스테이지 로직 실행
    }
}
