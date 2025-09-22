using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 스킬 이펙트 등 자주 생성되고 파괴되는 게임 오브젝트를 위한 범용 오브젝트 풀 매니저입니다.
/// 싱글톤으로 구현되어 어디서든 쉽게 접근할 수 있습니다.
/// </summary>
public class EffectPoolManager : Singleton<EffectPoolManager>
{
    // 프리팹을 Key로, 해당 프리팹으로 생성된 인스턴스들의 큐를 Value로 갖는 딕셔너리
    private Dictionary<GameObject, Queue<GameObject>> _poolDictionary;

    protected override void Awake()
    {
        base.Awake();
        _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
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
        }

        // 오브젝트를 활성화하고 위치와 회전값을 설정합니다.
        objectToSpawn.transform.SetParent(null); // 월드 공간에서 자유롭게 위치하도록 부모를 해제합니다.
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        // 지정된 시간 후에 자동으로 풀에 반납하는 코루틴을 시작합니다.
        StartCoroutine(ReturnToPoolAfter(objectToSpawn, prefab, duration));

        return objectToSpawn;
    }

    /// <summary>
    /// 지정된 시간(delay)이 지난 후, 이펙트 인스턴스를 풀에 반납하는 코루틴입니다.
    /// </summary>
    private IEnumerator ReturnToPoolAfter(GameObject instance, GameObject prefab, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(instance, prefab);
    }

    /// <summary>
    /// 사용이 끝난 이펙트 인스턴스를 비활성화하고 풀에 반납합니다.
    /// </summary>
    private void ReturnToPool(GameObject instance, GameObject prefab)
    {
        if (instance == null || prefab == null) return;

        instance.SetActive(false);
        // 씬이 복잡해지지 않도록 풀 매니저의 자식으로 넣어 정리합니다.
        instance.transform.SetParent(transform);
        
        // 딕셔너리에 해당 프리팹의 풀이 아직 존재하는지 확인합니다.
        if (_poolDictionary.ContainsKey(prefab))
        {
            _poolDictionary[prefab].Enqueue(instance);
        }
        // 만약 씬 전환 등으로 풀이 사라졌다면, 그냥 오브젝트를 파괴합니다.
        else
        {
            Destroy(instance);
        }
    }
}
