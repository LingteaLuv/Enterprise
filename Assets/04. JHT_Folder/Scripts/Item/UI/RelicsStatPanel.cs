using System.Collections.Generic;
using JHT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class RelicsStatPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] powerText;
        [SerializeField] private TextMeshProUGUI[] powerTypeText;

        List<RelicsObject> list;
        public void OnEnable()
        {
            InventoryManager.Instance.OnChangeAddItem += OnPanelReset;
            OnPanelReset(null);
            list = new();
        }

        private void OnDestroy()
        {
            InventoryManager.Instance.OnChangeAddItem -= OnPanelReset;
        }

        private void OnPanelReset(RelicsObject obj)
        {
            list = InventoryManager.Instance.relicsList;

            list.Sort((a,b) => a.itemSO.itemNum.CompareTo(b.itemSO.itemNum));
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                    continue;

                powerText[i].text = list[i].itemPower.ToString();

                powerTypeText[i].text = InventoryManager.Instance.SetItemPowerType(list[i].itemPowerType);
            }
        }
    }
}
