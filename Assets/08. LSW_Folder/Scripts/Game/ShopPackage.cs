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
        DatabaseManager.Instance.LoadPackageData(_packageId, (price, isPurchased) =>
        {
            _priceText.text = price;
            _purchaseBtn.interactable = !isPurchased;
            UpdatePanel();
        });
    }
    
    private void OnTouchPurchaseBtn()
    {
        DatabaseManager.Instance.SavePackage(_packageId);
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
