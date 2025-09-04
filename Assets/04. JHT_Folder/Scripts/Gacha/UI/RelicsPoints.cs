using JHT;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicsPoints : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI relicsPointText;
    [SerializeField] private TextMeshProUGUI relicsMaxPointText;

    [SerializeField] private Image fillImage; 

    private event Action OnChangeValue;

    RelicsGachaManager gachaManager;

    public void Init(RelicsGachaManager manager)
    {
        gachaManager = manager;
        relicsMaxPointText.text = manager.relicsSpecialCost.ToString(); 
        InventoryManager.Instance.OnChangeRelicsPoints += ShowRelicsPoint;
        ShowRelicsPoint(InventoryManager.Instance.RelicsPoints);
        OnChangeValue += ChangeRelicsPoint;
    }

    private void ShowRelicsPoint(float value)
    {
        relicsPointText.text = value.ToString();
        OnChangeValue?.Invoke();
    }

    private void ChangeRelicsPoint()
    {
        fillImage.fillAmount = InventoryManager.Instance.RelicsPoints / gachaManager.relicsSpecialCost;
    }
}
