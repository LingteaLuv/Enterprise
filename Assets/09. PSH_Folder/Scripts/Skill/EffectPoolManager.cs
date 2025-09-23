using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

/// <summary>
/// 스킬 이펙트 등 자주 생성되고 파괴되는 게임 오브젝트를 위한 범용 오브젝트 풀 매니저입니다.
/// 싱글톤으로 구현되어 어디서든 쉽게 접근할 수 있습니다.
/// [v2] 활성화된 오브젝트 추적 및 일괄 반납 기능이 추가되었습니다.
/// </summary>
public class EffectPoolManager : Singleton<EffectPoolManager>
{
    // 프리팹을 Key로, 해당 프리팹으로 생성된 인스턴스들의 큐를 Value로 갖는 딕셔너리
    private Dictionary<GameObject, Queue<GameObject>> _poolDictionary;

    // 현재 활성화되어 사용 중인 오브젝트들을 추적하기 위한 리스트
    private List<GameObject> _activeObjects;

    protected override void Awake()
    {
        base.Awake();
        _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        _activeObjects = new List<GameObject>();
    }

    /// <summary>
    /// 지정된 프리팹의 이펙트를 풀에서 가져와 사용합니다.
    /// </summary>
    /// <param name="prefab">스폰할 이펙트의 프리팹</param>
    /// <param name="position">스폰될 위치</param>
    /// <param name="rotation">스폰될 회전값</param>
    /// <param name="duration">이펙트가 활성화될 시간(초). 이 시간이 지나면 자동으로 풀에 반납됩니다.</param>
    /// <returns>스폰된 게임 오브젝트의 인스턴스</returns>
    public GameObject SpawnEffect(GameObject prefab, Vector3 position, Quaternion rotation, float duration)
    {
        Debug.Log($"[EffectPoolManager] SpawnEffect 호출됨! Prefab: {prefab.name}");
        if (prefab == null)
        {
            Debug.LogWarning("스폰하려는 이펙트 프리팹이 null입니다.");
            return null;
        }

        // 해당 프리팹을 위한 풀이 없으면 새로 생성합니다.
        if (!_poolDictionary.ContainsKey(prefab))
        {
            _poolDictionary[prefab] = new Queue<GameObject>();
        }

        GameObject objectToSpawn;

        // 풀에 사용 가능한 오브젝트가 있으면 가져옵니다.
        if (_poolDictionary[prefab].Count > 0)
        {
            objectToSpawn = _poolDictionary[prefab].Dequeue();
        }
        // 풀에 오브젝트가 없으면 새로 생성합니다.
        else
        {
            objectToSpawn = Instantiate(prefab);
            // 새로 생성된 오브젝트에 PooledObject 컴포넌트를 추가하여 원본 프리팹 정보를 저장합니다.
            var pooledComp = objectToSpawn.AddComponent<PooledObject>();
            pooledComp.Prefab = prefab;
        }

        // 오브젝트 활성화 및 설정
        objectToSpawn.transform.SetParent(null);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        // 활성화 리스트에 추가
        _activeObjects.Add(objectToSpawn);

        // 지정된 시간 후에 자동으로 풀에 반납하는 코루틴 시작
        StartCoroutine(ReturnToPoolAfter(objectToSpawn, duration));

        return objectToSpawn;
    }

    /// <summary>
    /// [신규] 현재 활성화된 모든 이펙트를 즉시 풀에 반납합니다.
    /// 전투 종료 등, 화면의 모든 이펙트를 정리해야 할 때 호출합니다.
    /// </summary>
    public void ReturnAllEffectsToPool()
    {
        Debug.Log($"[EffectPoolManager] 활성화된 모든 이펙트({_activeObjects.Count}개)를 풀에 반납합니다.");

        // 진행중인 모든 반납 코루틴을 중지합니다. (이중 반납 방지)
        StopAllCoroutines();

        // _activeObjects 리스트를 복사해서 순회합니다.
        // (원본 리스트는 ReturnToPool에서 수정되기 때문에 복사본 사용이 필수입니다)
        foreach (var obj in _activeObjects.ToList())
        {
            ReturnToPool(obj);
        }
    }

    private IEnumerator ReturnToPoolAfter(GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(instance);
    }

    /// <summary>
    /// 사용이 끝난 이펙트 인스턴스를 비활성화하고 풀에 반납합니다.
    /// </summary>
    private void ReturnToPool(GameObject instance)
    {
        if (instance == null) return;

        // 활성화된 오브젝트 리스트에서 제거
        _activeObjects.Remove(instance);

        var pooledComp = instance.GetComponent<PooledObject>();
        if (pooledComp == null || pooledComp.Prefab == null)
        {
            Debug.LogError($"{instance.name}에서 PooledObject 컴포넌트나 원본 Prefab 정보를 찾을 수 없어 풀에 반납할 수 없습니다. 오브젝트를 파괴합니다.");
            Destroy(instance);
            return;
        }

        GameObject originalPrefab = pooledComp.Prefab;

        instance.SetActive(false);
        // 씬이 복잡해지지 않도록 풀 매니저의 자식으로 넣어 정리합니다.
        instance.transform.SetParent(transform);

        if (_poolDictionary.ContainsKey(originalPrefab))
        {
            _poolDictionary[originalPrefab].Enqueue(instance);
        }
        // 만약 씬 전환 등으로 풀이 사라졌다면, 그냥 오브젝트를 파괴합니다.
        else
        {
            Debug.LogWarning($"씬 전환 등으로 {originalPrefab.name}의 풀이 사라져, {instance.name}을 파괴합니다.");
            Destroy(instance);
        }
    }
}