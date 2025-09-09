using DG.Tweening;
using JHT;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JHT_DamageBox : JHT_PooledObject
{

    [SerializeField] private TextMeshProUGUI hitBoxText;


    private WaitForSeconds TextDestroyTime = new WaitForSeconds(1.5f);
    public void ShowDamageText(float value)
    {
        StartCoroutine(StartShowText(value));
    }


    IEnumerator StartShowText(float value)
    {
        hitBoxText.text = value.ToString();
        hitBoxText.color = Color.red;
        hitBoxText.DOFade(0f, 2f);

        yield return TextDestroyTime;
        Release();
    }

}
