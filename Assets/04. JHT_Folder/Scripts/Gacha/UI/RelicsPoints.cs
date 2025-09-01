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

    private event Action<float> OnChangeValue;

    RelicsGachaManager gachaManager;
    void Start()
    {
        
    }

    public void Init(RelicsGachaManager manager)
    {
        gachaManager = manager;
        relicsMaxPointText.text = manager.relicsSpecialCost.ToString(); 
        InventoryManager.Instance.OnChangeRelicsPoints += ShowRelicsPoint;
        ShowRelicsPoint(InventoryManager.Instance.myRelicsPoints);
        OnChangeValue += ChangeRelicsPoint;
    }

    private void ShowRelicsPoint(float value)
    {
        relicsPointText.text = value.ToString();
        OnChangeValue?.Invoke(value);
    }

    private void ChangeRelicsPoint(float value)
    {
        fillImage.fillAmount = value / gachaManager.relicsSpecialCost;
    }
}
