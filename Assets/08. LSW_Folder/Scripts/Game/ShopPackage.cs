using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPackage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Button _purchaseBtn;
    
    private string _packageId;

    private void Start()
    {
        _purchaseBtn.onClick.AddListener(OnTouchPurchaseBtn);
    }
    
    public void Init(string packageId)
    {
        _packageId = packageId;
        _priceText.text = $"무료 패키지 : 100 Gem 획득";
        _purchaseBtn.interactable = true;
        UpdatePanel();
    }
    
    private void OnTouchPurchaseBtn()
    {
        DatabaseManager.Instance.SavePackage(_packageId);
        GoogleAdmobTester.Instance.ShowAd();
        DatabaseManager.Instance.LoadPackageData(_packageId, (price, reward) =>
        {
            DatabaseManager.Instance.AddCurrency("Gem", -price);
            foreach (var kvp in reward)
            {
                DatabaseManager.Instance.AddCurrency($"{kvp.Key}", Convert.ToInt32(kvp.Value));
            }
        });
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        DatabaseManager.Instance.LoadPackage(_packageId, (isPurchase) =>
        {
            _purchaseBtn.interactable = !isPurchase;
        });
    }
}
