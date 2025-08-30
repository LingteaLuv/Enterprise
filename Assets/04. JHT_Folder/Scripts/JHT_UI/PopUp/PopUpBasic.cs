using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JHT
{
    public enum JHT_PopUpType
    {
        NotChoose,
        CantChoose,
        CantGacha
    }

    [RequireComponent(typeof(Image))]
    public class PopUpBasic : MonoBehaviour
    {
        public JHT_PopUpType popUpType;

        public Image popUpBackGround;
        public Color endBackGroundColor;
        public Color curPopUpColor;
        public float showTime;

        public Button closeButton = null;

        public Animator animator;

        public void Init()
        {
            if(closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
            popUpBackGround.color = curPopUpColor;
        }

        private void Close()
        {
            this.gameObject.SetActive(false);
        }

        public void StartAnim()
        {
            animator.SetTrigger("UpAnim");
        }
    }
}
