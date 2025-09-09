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


        private JHT_BaseMonsterStat curSO; 
        
        public void Init(JHT_BaseMonsterStat so)
        {
            curSO = so;
        }

        public void ChangeHP(float value)
        {
            if (!hpGuageImage) return;
            if (curSO == null || curSO.maxHp <= 0f) return;

            hpGuageImage.DOFillAmount(value,1f);
        }

    }
}
