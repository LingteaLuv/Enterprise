using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopAdmobPackage : MonoBehaviour
{
    [SerializeField] private Button _purchaseBtn;
    [SerializeField] private GameObject _purchasedImage;
    private string _packageId;
    
    private void Start()
    {
        DatabaseManager.Instance.LoadAllPackageData(packageId =>
        {
            Debug.LogError("[ShopAdmobPackage] : package 정보 DB에서 불러옴");
            Init(packageId);
        });
        
        _purchaseBtn.onClick.AddListener(OnTouchPurchaseBtn);
    }
    
    private void Init(string packageId)
    {
        _packageId = packageId;
        _purchaseBtn.interactable = true;
        UpdatePanel();
    }
    
    private void OnTouchPurchaseBtn()
    {
        Debug.LogError("[ShopAdmobPackage] : OnTouchPurchaseBtn 진입");
        GoogleAdmobTester.Instance.ShowRewardedAd((reward) =>
        {
            Debug.LogError("[ShopAdmobPackage] : ShowRewardedAd 진입");
            DatabaseManager.Instance.LoadPackageData(_packageId, (price, rewardGem) =>
            {
                Debug.LogError("[ShopAdmobPackage] : LoadPackageData 진입");
                DatabaseManager.Instance.AddCurrency("Gem", -price);
                foreach (var kvp in rewardGem)
                {
                    DatabaseManager.Instance.AddCurrency($"{kvp.Key}", Convert.ToInt32(kvp.Value));
                    DatabaseManager.Instance.SavePackage(_packageId);
                    UpdatePanel();
                }
            });
        });
    }
    
    private void UpdatePanel()
    {
        DatabaseManager.Instance.LoadPackage(_packageId, (isPurchase) =>
        {
            _purchaseBtn.interactable = !isPurchase;
            _purchasedImage.SetActive(isPurchase);
        });
    }
}
