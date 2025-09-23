using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유물 효과를 실제로 적용하는 인터페이스
/// </summary>
public interface IRelicEffect
{
    void ApplyEffect(float value, RangeBuffDebuffType range);
    void RemoveEffect(float value, RangeBuffDebuffType range);
    string GetEffectDescription(float value);
}

/// <summary>
/// 유물 시스템 전체를 관리하는 메인 매니저 클래스
/// </summary>
public class RelicCollectionManager : MonoBehaviour
{
    public static RelicCollectionManager Instance { get; private set; }

    [Header("유물 데이터베이스")]
    [SerializeField] private List<CollectionData> allRelicData = new List<CollectionData>();

    private Dictionary<int, RelicInstance> _ownedRelics = new Dictionary<int, RelicInstance>();
    private Dictionary<BuffDebuffType, IRelicEffect> _effectHandlers = new Dictionary<BuffDebuffType, IRelicEffect>();

    // 유물 상태 변화 이벤트
    public event Action<RelicInstance> OnRelicAcquired;
    public event Action<RelicInstance> OnRelicLevelUp;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEffectHandlers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 효과 처리기들을 초기화
    /// </summary>
    private void InitializeEffectHandlers()
    {

    }

    /// <summary>
    /// 모든 유물 데이터 반환
    /// </summary>
    public List<CollectionData> GetAllRelics()
    {
        return new List<CollectionData>(allRelicData);
    }

    /// <summary>
    /// 획득한 유물들만 반환
    /// </summary>
    public List<RelicInstance> GetOwnedRelics()
    {
        return new List<RelicInstance>(_ownedRelics.Values);
    }

    /// <summary>
    /// 특정 유물의 획득 여부 확인
    /// </summary>
    public bool IsRelicOwned(int relicId)
    {
        return _ownedRelics.ContainsKey(relicId) && _ownedRelics[relicId].isAcquired;
    }

    /// <summary>
    /// 유물 획득 조건 체크 및 획득 처리
    /// </summary>
    public void CheckAcquisitionCondition(AcquisitionType conditionType, object currentValue)
    {
        var condition = AcquisitionConditionFactory.GetCondition(conditionType);
        if (condition == null) return;

        foreach (var relicData in allRelicData)
        {
            if (relicData.acquisitionType != conditionType) continue;
            if (IsRelicOwned(relicData.id)) continue;

            if (condition.CheckCondition(relicData.acquisitionValue, currentValue))
            {
                AcquireRelic(relicData.id);
            }
        }
    }

    /// <summary>
    /// 유물 획득 처리
    /// </summary>
    public bool AcquireRelic(int relicId)
    {
        var relicData = allRelicData.Find(r => r.id == relicId);
        if (relicData == null) return false;

        if (!_ownedRelics.ContainsKey(relicId))
        {
            _ownedRelics[relicId] = new RelicInstance { data = relicData };
        }

        var relic = _ownedRelics[relicId];
        if (!relic.isAcquired)
        {
            relic.Acquire();
            ApplyRelicEffect(relic);
            OnRelicAcquired?.Invoke(relic);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 유물 레벨업 처리
    /// </summary>
    public bool LevelUpRelic(int relicId)
    {
        if (!_ownedRelics.ContainsKey(relicId)) return false;

        var relic = _ownedRelics[relicId];
        if (!relic.isAcquired) return false;

        // 기존 효과 제거
        RemoveRelicEffect(relic);

        // 레벨업 실행
        if (relic.LevelUp())
        {
            // 새로운 레벨의 효과 적용
            ApplyRelicEffect(relic);
            OnRelicLevelUp?.Invoke(relic);
            return true;
        }
        else
        {
            // 레벨업 실패시 기존 효과 다시 적용
            ApplyRelicEffect(relic);
            return false;
        }
    }

    /// <summary>
    /// 유물 효과 적용
    /// </summary>
    private void ApplyRelicEffect(RelicInstance relic)
    {
        if (_effectHandlers.TryGetValue(relic.data.effectType, out IRelicEffect effectHandler))
        {
            effectHandler.ApplyEffect(relic.GetCurrentEffectValue(), relic.data.targetRange);
        }
    }

    /// <summary>
    /// 유물 효과 제거
    /// </summary>
    private void RemoveRelicEffect(RelicInstance relic)
    {
        if (_effectHandlers.TryGetValue(relic.data.effectType, out IRelicEffect effectHandler))
        {
            effectHandler.RemoveEffect(relic.GetCurrentEffectValue(), relic.data.targetRange);
        }
    }

    /// <summary>
    /// 특정 유물의 상세 정보 반환
    /// </summary>
    public RelicInstance GetRelicInstance(int relicId)
    {
        _ownedRelics.TryGetValue(relicId, out RelicInstance relic);
        return relic;
    }

    /// <summary>
    /// 유물 컬렉션 진행률 계산
    /// </summary>
    public float GetCollectionProgress()
    {
        if (allRelicData.Count == 0) return 0f;

        int acquiredCount = 0;
        foreach (var relic in _ownedRelics.Values)
        {
            if (relic.isAcquired) acquiredCount++;
        }

        return (float)acquiredCount / allRelicData.Count;
    }
}