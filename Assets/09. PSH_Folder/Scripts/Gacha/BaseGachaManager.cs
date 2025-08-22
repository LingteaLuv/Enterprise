using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

[System.Serializable]
public class RarityChance
{
    public Rarity rarity;
    [Range(0, 100)]
    public float chance; // 등급별 확률
}

public abstract class BaseGachaManager<T> : MonoBehaviour where T : class
{
    [Header("UI (공통)")]
    [Tooltip("뽑기 결과를 보여줄 UI 컴포넌트")]
    public GameObject resultPanel; // 예시: 결과 패널

    [Header("가챠 확률 (공통)")]
    [Tooltip("각 등급(Rarity)이 나올 확률")]
    public List<RarityChance> rarityChances;

    [Header("가챠 비용 (공통)")]
    public CurrencyType currencyType = CurrencyType.Gem;
    public int singleGachaCost = 100;

    // 마지막 뽑기 결과 (타입은 자식 클래스가 결정)
    public List<T> LastGachaResults { get; protected set; }

    /// <summary>
    /// 자식 클래스에서 구현해야 할 핵심 뽑기 로직입니다.
    /// 주어진 등급(rarity)에 해당하는 아이템 1개를 뽑아 반환합니다.
    /// </summary>
    /// <param name="rarity">선택된 등급</param>
    /// <returns>뽑힌 아이템의 플레이어 데이터</returns>
    protected abstract T DrawItem(Rarity rarity);

    /// <summary>
    /// 가챠를 여러 번 실행하는 공통 메소드입니다.
    /// </summary>
    /// <param name="count">뽑을 횟수</param>
    public virtual bool PerformMultipleGacha(int count)
    {
        // 1. 재화 소모 (공통)
        BigInteger totalCost = singleGachaCost * count;
        if (!CurrencyManager.Instance.SpendCurrency(currencyType, totalCost))
        {
            Debug.Log($"가챠 실패: 재화({currencyType})가 부족합니다.");
            return false;
        }

        // 2. 뽑기 실행 (개별 로직 호출)
        LastGachaResults = new List<T>();
        for (int i = 0; i < count; i++)
        {
            Rarity chosenRarity = GetRandomRarity();
            T drawnItem = DrawItem(chosenRarity);
            if (drawnItem != null)
            {
                LastGachaResults.Add(drawnItem);
            }
        }

        // 3. 결과 처리 및 UI 갱신 (공통)
        Debug.Log($"{count}회 뽑기 완료! {LastGachaResults.Count}개의 아이템 획득.");
        // 예시: 결과 UI를 활성화하고 결과를 표시
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            // resultPanel.GetComponent<ResultUI>().DisplayResults(LastGachaResults);
        }
        CurrencyManager.Instance.UpdateCurrencyUI();
        return true;
    }

    /// <summary>
    /// 설정된 확률에 따라 등급을 뽑는 공통 함수입니다.
    /// </summary>
    protected Rarity GetRandomRarity()
    {
        float totalChance = rarityChances.Sum(rc => rc.chance);
        if (totalChance <= 0)
        {
            Debug.LogError("가챠 확률의 총합이 0보다 작거나 같습니다!");
            return rarityChances.FirstOrDefault()?.rarity ?? default;
        }
        float randomPoint = Random.Range(0, totalChance);

        foreach (var rarityInfo in rarityChances)
        {
            if (randomPoint < rarityInfo.chance)
            {
                return rarityInfo.rarity;
            }
            else
            {
                randomPoint -= rarityInfo.chance;
            }
        }
        // 예외 상황 방지
        return rarityChances.Last().rarity;
    }
}
