using System.Collections.Generic;
using UnityEngine;

public class BattleField : MonoBehaviour
{
    [field: SerializeField] public Transform PlayerSpawnPoint { get; private set; }
    [field: SerializeField] public List<Transform> EnemySpawnPoints { get; private set; }

    private void Awake()
    {
        // 자동으로 EnemySpawnPoint 하위에 있는 자식 Transform들을 수집
        if (EnemySpawnPoints == null || EnemySpawnPoints.Count == 0)
        {
            var group = transform.Find("EnemySpawnPoint");
            EnemySpawnPoints = new List<Transform>();
            foreach (Transform t in group)
                EnemySpawnPoints.Add(t);
        }
    }
}
