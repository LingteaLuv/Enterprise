using UnityEngine;
using UnityEngine.Purchasing;
public class IAPManager : MonoBehaviour
{
    public void PurchaseStarterChest()
    {
        DatabaseManager.Instance.AddCurrency("CrewDrawTicket", 3);
        DatabaseManager.Instance.AddCurrency("EquipDrawTicket", 3);
        DatabaseManager.Instance.AddCurrency("RelicsCoupon", 3);
        DatabaseManager.Instance.AddCurrency("Gem", 3000);
    }
    public void PurchaseBiginnerPackage()
    {
        DatabaseManager.Instance.AddCurrency("Gem", 1000);
        DatabaseManager.Instance.AddCurrency("Gold", 1000);
    }
    
    public void PurchaseCrew30()
    {
        DatabaseManager.Instance.AddCurrency("CrewDrawTicket", 30);
    }
    
    public void PurchaseEquip30()
    {
        DatabaseManager.Instance.AddCurrency("EquipDrawTicket", 30);
    }
    
    public void PurchaseRelics30()
    {
        DatabaseManager.Instance.AddCurrency("RelicsCoupon", 30);
    }
    
    public void PurchaseCrew10()
    {
        DatabaseManager.Instance.AddCurrency("CrewDrawTicket", 10);
    }
    
    public void PurchaseEquip10()
    {
        DatabaseManager.Instance.AddCurrency("EquipDrawTicket", 10);
    }
    
    public void PurchaseRelics10()
    {
        DatabaseManager.Instance.AddCurrency("RelicsCoupon", 10);
    }
    
    public void PurchaseDiaPack()
    {
        DatabaseManager.Instance.AddCurrency("Gem", 1000);
    }
    
    public void PurchaseDia1200()
    {
        DatabaseManager.Instance.AddCurrency("Gem", 1200);
    }
    
    public void PurchaseDia2500()
    {
        DatabaseManager.Instance.AddCurrency("Gem", 2500);
    }
    
    public void PurchaseDia5500()
    {
        DatabaseManager.Instance.AddCurrency("Gem", 5500);
    }
}
