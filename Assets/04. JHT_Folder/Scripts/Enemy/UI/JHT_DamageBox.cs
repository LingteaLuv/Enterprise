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
        StartCoroutine(StartShowText(DataUtility.FormatNumber(value)));
        hitBoxText.rectTransform.anchoredPosition = new Vector2(0, 0);
    }


    IEnumerator StartShowText(string value)
    {
        hitBoxText.text = value;
        hitBoxText.color = Color.red;
        hitBoxText.DOFade(0f, 1f);
        hitBoxText.GetComponent<RectTransform>().DOAnchorPosY(0.5f,1f);

        yield return TextDestroyTime;
        Release();
    }

}
