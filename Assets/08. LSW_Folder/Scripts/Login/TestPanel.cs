using UnityEngine;
using UnityEngine.UI;

public class TestPanel : UIBase
{
    [SerializeField] private Button _plusGold;
    [SerializeField] private Button _minusGold;

    private void Start()
    {
        _plusGold.onClick.AddListener(OnTouchedPlusGold);
        _minusGold.onClick.AddListener(OnTouchedMinusGold);
    }

    private void OnTouchedPlusGold()
    {
        DatabaseManager.Instance.PlusGold();
    }
    
    private void OnTouchedMinusGold()
    {
        DatabaseManager.Instance.MinusGold();
    }
}
