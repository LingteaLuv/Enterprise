using JHT;
using TMPro;
using UnityEngine;

public class RelicsCoupon : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI relicsCouponCountText;


    private void Start()
    {
        relicsCouponCountText.text = InventoryManager.Instance.relicsCoupon.ToString();
        InventoryManager.Instance.OnChangeRelicsCoupon += ChangeValue;
    }

    private void ChangeValue(int value)
    {
        relicsCouponCountText.text = InventoryManager.Instance.relicsCoupon.ToString();
    }
}
