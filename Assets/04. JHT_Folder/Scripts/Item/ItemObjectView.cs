using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public class ItemObjectView : MonoBehaviour
    {
        [SerializeField] private Button itemObject;


        private void Start()
        {
            itemObject.onClick.AddListener(ShowItem);
        }

        private void ShowItem()
        {

        }
    }
}
