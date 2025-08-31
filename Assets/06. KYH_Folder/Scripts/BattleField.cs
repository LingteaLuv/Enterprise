using System.Collections.Generic;
using UnityEngine;

public class BattleField : MonoBehaviour
{
    //  플레이어가 스폰될 위치
    // - 인스펙터에서 직접 할당 가능
    // - 외부에서 참조만 가능 (private set)
    [field: SerializeField] public Transform PlayerSpawnPoint { get; private set; }

    //  적들이 스폰될 위치 리스트
    // - 인스펙터에서 할당 가능
    // - 만약 비어있으면 자동으로 자식 오브젝트를 수집
    [field: SerializeField] public List<Transform> EnemySpawnPoints { get; private set; }

    private void Awake()
    {
        // EnemySpawnPoints가 비어있거나 null이라면 자동으로 찾기
        if (EnemySpawnPoints == null || EnemySpawnPoints.Count == 0)
        {
            // 이 오브젝트의 자식 중에서 "EnemySpawnPoint"라는 이름을 가진 Transform을 찾음
            var group = transform.Find("EnemySpawnPoint");

            // 새로운 리스트 생성
            EnemySpawnPoints = new List<Transform>();

            // group 아래에 있는 모든 자식 Transform을 리스트에 추가
            foreach (Transform t in group)
                EnemySpawnPoints.Add(t);
        }
    }
}
