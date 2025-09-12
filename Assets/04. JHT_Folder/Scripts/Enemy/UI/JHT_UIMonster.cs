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


        public void ChangeHP(float value)
        {
            if (!hpGuageImage) 
                return;

            hpGuageImage.DOFillAmount(value, 0.5f);
        }

        public void ReleaseMonsterHP()
        {
            hpGuageImage.fillAmount = 1f;
        }
    }
}
