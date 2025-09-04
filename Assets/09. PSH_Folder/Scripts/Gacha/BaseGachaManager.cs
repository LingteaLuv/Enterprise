using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public abstract class BaseGachaManager<T> : MonoBehaviour where T : class
{
    [Header("UI (공통)")]
    [Tooltip("뽑기 결과를 보여줄 UI 컴포넌트")]
    public GameObject resultPanel;

    [Header("가챠 비용 (공통)")]
    public CurrencyType currencyType = CurrencyType.Gem;
    public int singleGachaCost = 100;

    public List<T> LastGachaResults { get; protected set; }

    protected virtual void Start()
    {
        if (resultPanel == null)
        {
            Debug.Log("[BaseGachaManager] Result Panel이 할당되지 않아, 씬에서 'GachaListPanel'을 찾습니다.");
            resultPanel = GameObject.Find("GachaListPanel");

            if (resultPanel == null)
            {
                Debug.LogError("[BaseGachaManager] 'GachaListPanel'이라는 이름의 GameObject를 씬에서 찾을 수 없습니다. Gacha Manager의 인스펙터에서 Result Panel을 수동으로 할당해주세요.");
            }
            else
            {
                Debug.Log("[BaseGachaManager] 'GachaListPanel'을 성공적으로 찾아 할당했습니다.");
                resultPanel.SetActive(false);
            }
        }
        else
        {
            resultPanel.SetActive(false);
        }
    }

    protected abstract T DrawItem();

    public virtual bool PerformMultipleGacha(int count)
    {
        BigInteger totalCost = singleGachaCost * count;
        if (!CurrencyManager.Instance.SpendCurrency(currencyType, totalCost))
        {
            Debug.Log($"가챠 실패: 재화({currencyType})가 부족합니다.");
            return false;
        }

        LastGachaResults = new List<T>();
        for (int i = 0; i < count; i++)
        {
            T drawnItem = DrawItem();
            if (drawnItem != null)
            {
                LastGachaResults.Add(drawnItem);
            }
        }

        Debug.Log($"{count}회 뽑기 완료! {LastGachaResults.Count}개의 아이템 획득.");
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        CurrencyManager.Instance.UpdateCurrencyUI();
        return true;
    }
}