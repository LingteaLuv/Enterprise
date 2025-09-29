using JHT;
using System;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicsPoints : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI relicsPointText;
    [SerializeField] private TextMeshProUGUI relicsMaxPointText;

    [SerializeField] private Image fillImage; 

    private event Action OnChangeValue;

    RelicsGachaManager gachaManager;

    public void Init(RelicsGachaManager manager)
    {
        gachaManager = manager;
        relicsMaxPointText.text = manager.relicsSpecialCost.ToString();

        InventoryManager.Instance.OnChangeRelicsPoints -= ShowRelicsPoint;
        InventoryManager.Instance.OnChangeRelicsPoints += ShowRelicsPoint;

        ShowRelicsPoint();

        OnChangeValue -= ChangeRelicsPoint;
        OnChangeValue += ChangeRelicsPoint;
    }

    private void ShowRelicsPoint()
    {
        relicsPointText.text = CurrencyManager.Instance.GetCurrency(CurrencyType.RelicsCoupon).ToString();
        OnChangeValue?.Invoke();
    }

    private void ChangeRelicsPoint()
    {
        fillImage.fillAmount = (float)CurrencyManager.Instance.GetCurrency(CurrencyType.RelicsPoint) / (float)gachaManager.relicsSpecialCost;
    }
}
