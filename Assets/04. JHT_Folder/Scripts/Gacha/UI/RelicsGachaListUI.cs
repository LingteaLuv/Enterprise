using JHT;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RelicsGachaListUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RelicsGachaItemPanel relicsPanelItem;
    [SerializeField] private GameObject backPanel;
    [SerializeField] private Transform relicsItemParent;

    [Header("PopUp")]
    [SerializeField] private ChoosePopUp choosePopup;

    [Header("Button")]
    [SerializeField] private Button closeButton;



    public void Init(RelicsGachaManager relicsGachaManager)
    {
        relicsGachaManager.OnChooseItem -= ShowChooseItem;
        relicsGachaManager.OnChooseItem += ShowChooseItem;
        closeButton.onClick.AddListener(ClosePanel);

        ClearChildren(relicsItemParent);

        StartCoroutine(DisplayPanel(relicsGachaManager));
    }

    private IEnumerator DisplayPanel(RelicsGachaManager relicsGachaManager)
    {
        yield return new WaitUntil(() =>
        relicsGachaManager.relicsResult != null &&
        relicsGachaManager.rarityResult != ItemRarity.None &&
        relicsGachaManager.levelResult > 0);

        backPanel.SetActive(true);

        RelicsGachaItemPanel panel = Instantiate(relicsPanelItem);
        panel.transform.SetParent(relicsItemParent);
        panel.Init(relicsGachaManager);
        
    }

    private void ClosePanel()
    {
        if(this.gameObject.activeSelf)
        {
            if(backPanel.activeSelf)
                backPanel.SetActive(false);

            closeButton.onClick.RemoveListener(ClosePanel);
            gameObject.SetActive(false);
        }
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child != null) Destroy(child.gameObject);
        }
    }

    private void ShowChooseItem(RelicsObject obj1, RelicsObject obj2)
    {
        choosePopup.gameObject.SetActive(true);
        choosePopup.Init(obj1, obj2);
    }

}
