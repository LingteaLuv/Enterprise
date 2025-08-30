using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace JHT
{
    public class JHT_PopUpManager : MonoBehaviour
    {
        [SerializeField] private PopUpBasic[] datas;
        Dictionary<JHT_PopUpType, PopUpBasic> popUpDic;

        Coroutine showCor;
        private void Awake()
        {
            popUpDic = new Dictionary<JHT_PopUpType, PopUpBasic>();
            popUpDic.Clear();

            foreach (var d in datas)
            {
                if (d == null) 
                    continue;

                PopUpBasic inst = Instantiate(d,transform);
                inst.Init();
                inst.gameObject.SetActive(false);
                popUpDic.Add(inst.popUpType, inst);
            }
        }

        public void ShowPopUp(JHT_PopUpType type)
        {
            if (showCor == null)
            {
                showCor = StartCoroutine(StartShow(type));
            }
        }

        private IEnumerator StartShow(JHT_PopUpType type)
        {
            WaitForSeconds delay = new WaitForSeconds(popUpDic[type].showTime);
            
            if (popUpDic[type] == null)
                yield break;

            popUpDic[type].gameObject.SetActive(true);

            popUpDic[type].StartAnim();

            yield return delay;

            

            popUpDic[type].gameObject.SetActive(false);

            if (showCor != null)
            {
                StopCoroutine(showCor);
                showCor = null;
            }
        }

    }
}
