using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : UIController<GameUIController.GameUIType>
{
    [SerializeField] private List<Toggle> _toggleGroup;
    [SerializeField] private TextMeshProUGUI _gold;
    
    public enum GameUIType
    {
        DungeonPanel,
        ShopPanel
    }
    
    private void Start()
    {
        for (int i = 0; i < _toggleGroup.Count; i++)
        {
            int index = i;
            _toggleGroup[index].onValueChanged.AddListener((isOn) =>
            {
                _uiList[index].gameObject.SetActive(isOn);
            });
        }
        
        foreach (var ui in _uiList)
        {
            if (ui is DungeonPanel dungeonPanel)
            {
            }
            
            else if (ui is ShopPanel shopPanel)
            {
            }
        }
        DatabaseManager.Instance.DisplayCreditData();
        DatabaseManager.Instance.OnChangedCreditData += SetText;
    }

    private void SetText()
    {
        _gold.text = $"{DatabaseManager.Instance.PlayerData.CreditData.Gold.ToString()} Gold";
    }
}
