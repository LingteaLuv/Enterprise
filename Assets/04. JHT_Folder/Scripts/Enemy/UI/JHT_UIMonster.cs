using JHT;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


namespace JHT
{
    public class JHT_UIMonster : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image hpGuageImage;
        private void OnEnable()
        {
            if (!hpGuageImage) 
                return;

            hpGuageImage.DOKill();
            hpGuageImage.fillAmount = 1f;
        }
        private void OnDisable()
        {
            hpGuageImage.fillAmount = 0f;
            if (hpGuageImage) hpGuageImage.DOKill();
        }

        public void ChangeHP(float value)
        {
            if (!hpGuageImage) 
                return;

            hpGuageImage.DOFillAmount(value, 0.5f);
        }

    }
}
