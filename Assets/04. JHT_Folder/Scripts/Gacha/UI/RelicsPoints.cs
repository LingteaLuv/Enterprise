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
    void Start()
    {
        
    }

    public void Init(RelicsGachaManager manager)
    {
        gachaManager = manager;
        relicsMaxPointText.text = manager.relicsSpecialCost.ToString(); 
        InventoryManager.Instance.OnChangeRelicsPoints += ShowRelicsPoint;
        ShowRelicsPoint();
        OnChangeValue += ChangeRelicsPoint;
    }

    private void ShowRelicsPoint()
    {
        relicsPointText.text = InventoryManager.Instance.relicsPoints.ToString();
        OnChangeValue?.Invoke();
    }

    private void ChangeRelicsPoint()
    {
        fillImage.fillAmount = InventoryManager.Instance.relicsPoints / gachaManager.relicsSpecialCost;
    }
}
