using JHT;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicsProbability : MonoBehaviour
{
    private int curTableLevel;

    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI pointText;

    [SerializeField] private RelicsProbabilityItemPanel[] probabilityItemPanel;

    private float upgradePoint;

    RelicsGachaManager _manager;

    private event Action OnUpgradeProbability;

    public void Init(RelicsGachaManager manager)
    {
        curTableLevel = manager.relicsTablelevel;
        _manager = manager;

        InventoryManager.Instance.OnChangeRelicsPoints += AddPoints;

        upgradeCostText.text = _manager.relicsUpgradeCost.ToString();

        ShowProbabilityItemPanel();

        upgradeButton.onClick.RemoveListener(delegate { UpgradeRelicsGacha(manager); });
        upgradeButton.onClick.AddListener(delegate { UpgradeRelicsGacha(manager); });

        closeButton.onClick.RemoveListener(() => gameObject.SetActive(false));
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        OnUpgradeProbability += ShowProbabilityItemPanel;

        AddPoints();
    }

    private void OnDestroy()
    {
        InventoryManager.Instance.OnChangeRelicsPoints -= AddPoints;
        closeButton.onClick.RemoveListener(() => gameObject.SetActive(false));
        upgradeButton.onClick.RemoveListener(delegate { UpgradeRelicsGacha(_manager); });
    }

    private void UpgradeRelicsGacha(RelicsGachaManager manager)
    {
        if (manager == null)
            return;

        if (manager.relicsUpgradeCost > InventoryManager.Instance.relicsPoints)
        {
            UIManager.Instance.ShowToast(
                $"유물 재화가 {manager.relicsUpgradeCost - InventoryManager.Instance.relicsPoints}만큼 부족합니다");

            StartCoroutine(WaitCoroutine());
        }
        else
        {
            UIManager.Instance.ShowConfirm(
            "정말 업그레이드 하시겠습니까?",
            () =>
            {
                _manager.relicsTablelevel++;
                InventoryManager.Instance.relicsPoints -= _manager.relicsUpgradeCost;
                InventoryManager.Instance.OnChangeRelicsPoints?.Invoke();
                OnUpgradeProbability?.Invoke();
                StartCoroutine(WaitCoroutine());
            });
        }
    }

    private void ShowProbabilityItemPanel()
    {

        for (int i = 0; i < probabilityItemPanel.Length; i++)
        {
            probabilityItemPanel[i].Init(_manager, curTableLevel, i);
        }

    }

    private IEnumerator WaitCoroutine()
    {
        closeButton.interactable = false;
        yield return new WaitForSeconds(0.5f);
        closeButton.interactable = true;
    }

    private void AddPoints()
    {
        upgradePoint = InventoryManager.Instance.relicsPoints;
        pointText.text = upgradePoint.ToString();
    }
    
}
