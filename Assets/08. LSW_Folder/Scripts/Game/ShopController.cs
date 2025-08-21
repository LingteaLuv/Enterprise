using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ShopController : MonoBehaviour
{
    [SerializeField] private AssetReferenceT<GameObject> _packagePrefab;
    [SerializeField] private Transform _parent;

    private void Start()
    {
        DatabaseManager.Instance.LoadAllPackageData(packageId =>
        {
            CreatePackage(packageId);
        });
    }
    
    
    private void CreatePackage(string packageId)
    {
        var packageHandle = Addressables.InstantiateAsync(_packagePrefab, _parent);
        packageHandle.Completed += (operation) =>
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                var shopPackage = operation.Result.GetComponent<ShopPackage>();
                shopPackage.Init(packageId);
            }
        };
    }
}
