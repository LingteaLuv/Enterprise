using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 스킬 이펙트 등 자주 생성되고 파괴되는 게임 오브젝트를 위한 범용 오브젝트 풀 매니저입니다.
/// 싱글톤으로 구현되어 어디서든 쉽게 접근할 수 있습니다.
/// [v3] 제네릭 스폰 함수 및 수동 반납 기능이 추가되었습니다.
/// </summary>
public class EffectPoolManager : Singleton<EffectPoolManager>
{
    private Dictionary<GameObject, Queue<GameObject>> _poolDictionary;
    private List<GameObject> _activeObjects;
    private static Canvas _mainCanvas; // 캔버스 캐싱을 위한 변수

    protected override void Awake()
    {
        base.Awake();
        _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        _activeObjects = new List<GameObject>();
        _mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
    }

    /// <summary>
    /// [신규] 지정된 프리팹을 풀에서 가져와 컴포넌트를 반환합니다.
    /// 이 버전은 자동 반납 기능이 없으므로, 사용 후 수동으로 ReturnToPool을 호출해야 합니다.
    /// </summary>
    public T SpawnObject<T>(GameObject prefab) where T : Component
    {
        if (prefab == null)
        {
            Debug.LogWarning("스폰하려는 프리팹이 null입니다.");
            return null;
        }
        if (prefab.GetComponent<RectTransform>() == null)
        {
            Debug.LogError($"'{prefab.name}'은 UI 프리팹이 아니므로 SpawnObject를 사용할 수 없습니다. 일반 오브젝트는 SpawnEffect를 사용해주세요.");
            return null;
        }

        GameObject objectToSpawn = GetPooledObject(prefab);

        if (_mainCanvas == null)
        {
            Debug.LogError("씬에서 Canvas를 찾을 수 없습니다. UI 오브젝트를 스폰할 수 없습니다.");
            ReturnToPool(objectToSpawn);
            return null;
        }

        objectToSpawn.transform.SetParent(_mainCanvas.transform, false);
        objectToSpawn.transform.localScale = Vector3.one;

        objectToSpawn.SetActive(true);
        _activeObjects.Add(objectToSpawn);

        T component = objectToSpawn.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"스폰된 오브젝트 '{objectToSpawn.name}'에서 '{typeof(T)}' 컴포넌트를 찾을 수 없습니다. 즉시 풀에 반납합니다.");
            ReturnToPool(objectToSpawn);
            return null;
        }

        return component;
    }

    /// <summary>
    /// 지정된 프리팹의 이펙트를 풀에서 가져와 사용합니다. (시간 기반 자동 반납)
    /// </summary>
    public GameObject SpawnEffect(GameObject prefab, Vector3 position, Quaternion rotation, float duration)
    {
        if (prefab == null) { /* ... null check ... */ return null; }

        // 이 부분의 로직은 SpawnObject와 매우 유사하지만, 코루틴 호출 부분이 다릅니다.
        // 여기서는 설명을 위해 생략하지만, 실제 코드에서는 SpawnObject의 로직을 기반으로 구성됩니다.
        GameObject objectToSpawn = GetPooledObject(prefab);

        objectToSpawn.transform.SetParent(null);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        _activeObjects.Add(objectToSpawn);

        StartCoroutine(ReturnToPoolAfter(objectToSpawn, duration));

        return objectToSpawn;
    }

    // SpawnObject와 SpawnEffect의 공통 로직을 담는 도우미 함수
    private GameObject GetPooledObject(GameObject prefab)
    {
        if (!_poolDictionary.ContainsKey(prefab))
        {
            _poolDictionary[prefab] = new Queue<GameObject>();
        }

        GameObject objectToSpawn;
        if (_poolDictionary[prefab].Count > 0)
        {
            objectToSpawn = _poolDictionary[prefab].Dequeue();
        }
        else
        {
            objectToSpawn = Instantiate(prefab);
            var pooledComp = objectToSpawn.AddComponent<PooledObject>();
            pooledComp.Prefab = prefab;
        }
        return objectToSpawn;
    }

    /// <summary>
    /// 사용이 끝난 인스턴스를 비활성화하고 풀에 반납합니다. (Public으로 변경됨)
    /// </summary>
    public void ReturnToPool(GameObject instance)
    {
        if (instance == null || !instance.activeSelf) return; // 이미 반납되었거나 null이면 무시

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
        instance.transform.SetParent(transform);

        if (_poolDictionary.ContainsKey(originalPrefab))
        {
            _poolDictionary[originalPrefab].Enqueue(instance);
        }
        else
        {
            Debug.LogWarning($"씬 전환 등으로 {originalPrefab.name}의 풀이 사라져, {instance.name}을 파괴합니다.");
            Destroy(instance);
        }
    }

    public void ReturnAllEffectsToPool()
    {
        StopAllCoroutines();
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
}
