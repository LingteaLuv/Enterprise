using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabBarController : MonoBehaviour
{
    [Header("하단 버튼")]
    [SerializeField] Button basicStatBtn;
    [SerializeField] Button characterListBtn;
    [SerializeField] Button gachaBtn;

    [Header("버튼에 연결된 UI")]
    [SerializeField] GameObject basicStatPanel;
    [SerializeField] GameObject characterListPanel;
    [SerializeField] GameObject gachaPanel;

    // 버튼과 판넬을 연결하는 딕셔너리
    private Dictionary<Button, GameObject> dic;

    private void Awake()
    {
        dic = new Dictionary<Button, GameObject>
        {
            { basicStatBtn, basicStatPanel },
            { characterListBtn, characterListPanel },
            { gachaBtn, gachaPanel }
        };

        Init();
    }

    void Init()
    {
        basicStatPanel.SetActive(false);
        characterListPanel.SetActive(false);
        gachaPanel.SetActive(false);

        // 버튼에 연결
        foreach (var item in dic)
        {
            item.Key.onClick.AddListener(CloseAllPanel);
            // 나중에 ui 등장 애니메이션 연결
            item.Key.onClick.AddListener(() => item.Value.SetActive(true));
        }
    }

    //다른거 끄기
    public void CloseAllPanel()
    {
        foreach (var item in dic.Values)
        {
            item.SetActive(false);
        }        
    }
}
