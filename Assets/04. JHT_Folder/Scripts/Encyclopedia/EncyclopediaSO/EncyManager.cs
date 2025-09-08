using System.Collections.Generic;
using UnityEngine;

public class EncyManager : MonoBehaviour
{
    [Header("")]
    [SerializeField] private EncyPanelItem encyPanelItem;


    [Header("Data Setting")]
    [SerializeField] Transform parent;
    [SerializeField] private EncyCharacter[] encyData;
    private Dictionary<int, EncyCharacter> encyDic;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < encyData.Length; i++)
        {
            encyDic.Add(encyData[i].curSO.ID, encyData[i]);
        }

    }


}
