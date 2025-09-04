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


        upgradeCostText.text = _manager.relicsUpgradeCost.ToString();

        //upgradeButton.onClick.RemoveListener(delegate { UpgradeRelicsGacha(manager); });
        upgradeButton.onClick.AddListener(delegate { UpgradeRelicsGacha(_manager); });

        //closeButton.onClick.RemoveListener(() => gameObject.SetActive(false));
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        OnUpgradeProbability += ShowProbabilityItemPanel;
        InventoryManager.Instance.OnChangeRelicsPoints += AddPoints;

        ShowProbabilityItemPanel();
        AddPoints(InventoryManager.Instance.RelicsPoints);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        OnUpgradeProbability -= ShowProbabilityItemPanel;
        InventoryManager.Instance.OnChangeRelicsPoints -= AddPoints;
        closeButton.onClick.RemoveListener(() => gameObject.SetActive(false));
        upgradeButton.onClick.RemoveListener(delegate { UpgradeRelicsGacha(_manager); });
    }

    private void UpgradeRelicsGacha(RelicsGachaManager manager)
    {
        if (manager == null)
            return;

        if (manager.relicsUpgradeCost > InventoryManager.Instance.RelicsPoints)
        {
            UIManager.Instance.ShowToast(
                $"유물 재화가 {manager.relicsUpgradeCost - InventoryManager.Instance.RelicsPoints}만큼 부족합니다");

            StartCoroutine(WaitCoroutine());
        }
        else
        {
            UIManager.Instance.ShowConfirm(
            "정말 업그레이드 하시겠습니까?",
            () =>
            {
                _manager.relicsTablelevel += 1;
                curTableLevel = _manager.relicsTablelevel;
                InventoryManager.Instance.RelicsPoints -= _manager.relicsUpgradeCost;
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

    private void AddPoints(float value)
    {
        upgradePoint = value;
        pointText.text = upgradePoint.ToString();
    }
    
}
