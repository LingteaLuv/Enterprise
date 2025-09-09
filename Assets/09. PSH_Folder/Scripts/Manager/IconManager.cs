using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 제네릭 아이콘 매핑 클래스입니다.
/// 어떤 Enum 타입이든 키로 사용할 수 있습니다.
/// </summary>
[System.Serializable]
public class IconMapping<T> where T : Enum
{
    public T key;
    public Sprite icon;
}

/// <summary>
/// 제네릭 아이콘 매니저의 기본 클래스입니다.
/// 이 클래스를 상속받아 특정 Enum에 대한 아이콘 매니저를 만들 수 있습니다.
/// </summary>
public abstract class IconManager<TManager, TEnum, TMapping> : Singleton<TManager>
    where TManager : MonoBehaviour
    where TEnum : Enum
    where TMapping : IconMapping<TEnum>
{
    [Header("아이콘 매핑 리스트")]
    [SerializeField] private List<TMapping> iconMappings = new List<TMapping>();

    private Dictionary<TEnum, Sprite> iconCache = new Dictionary<TEnum, Sprite>();

    protected override void Awake()
    {
        base.Awake();
        InitializeCache();
    }

    private void InitializeCache()
    {
        iconCache.Clear();
        foreach (var mapping in iconMappings)
        {
            if (!iconCache.ContainsKey(mapping.key))
            {
                iconCache.Add(mapping.key, mapping.icon);
            }
            else
            {
                Debug.LogWarning($"[IconManager] {mapping.key}에 대한 아이콘 키가 중복되었습니다.");
            }
        }
    }

    public Sprite GetIcon(TEnum key)
    {
        if (iconCache.TryGetValue(key, out Sprite icon))
        {
            return icon;
        }

        Debug.LogWarning($"[IconManager] {key}에 해당하는 아이콘을 찾을 수 없습니다.");
        return null;
    }
}