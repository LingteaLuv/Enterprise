using System.Numerics;
using JHT;
using TMPro;
using UnityEngine;

public class RelicsCoupon : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI relicsCouponCountText;


    private void OnEnable()
    {
        InventoryManager.Instance.OnChangeRelicsCoupon += ChangeValue;
        ChangeValue();
    }
    private void OnDisable()
    {
        InventoryManager.Instance.OnChangeRelicsCoupon -= ChangeValue;
    }

    private void ChangeValue()
    {
        relicsCouponCountText.text = CurrencyManager.Instance.GetCurrency(CurrencyType.RelicsCoupon).ToString();
    }
}
