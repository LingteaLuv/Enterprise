using UnityEngine;

public class IAPManager : MonoBehaviour
{
    public void PurchaseTest_id()
    {
        DatabaseManager.Instance.AddCurrency("Gem", 100);
    }
}
