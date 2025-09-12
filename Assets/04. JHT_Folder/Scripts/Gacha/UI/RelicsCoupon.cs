using JHT;
using TMPro;
using UnityEngine;

public class RelicsCoupon : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI relicsCouponCountText;


    private void OnEnable()
    {
        InventoryManager.Instance.OnChangeRelicsCoupon += ChangeValue;
        relicsCouponCountText.text = InventoryManager.Instance.RelicsCoupon.ToString();
    }
    private void OnDisable()
    {
        InventoryManager.Instance.OnChangeRelicsCoupon -= ChangeValue;
    }

    private void ChangeValue(int value)
    {
        relicsCouponCountText.text = InventoryManager.Instance.RelicsCoupon.ToString();
    }
}
