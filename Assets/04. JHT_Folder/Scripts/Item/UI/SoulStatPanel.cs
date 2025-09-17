using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoulStatPanel : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private Button btn; // 강화창으로 바로 가기 버튼
    [SerializeField] private CharacterScrollViewUI characterScrollViewUI;
    [SerializeField] private TabBarController tabBarController;

    private void Start()
    {
        characterScrollViewUI = FindAnyObjectByType<CharacterScrollViewUI>(FindObjectsInactive.Include);
        tabBarController = FindAnyObjectByType<TabBarController>(FindObjectsInactive.Include);
    }

    public void Init(PlayerCharacterData data)
    {

        characterImage.sprite = data.characterdata.characterSoul;
        nameText.text = data.characterdata.characterName + "의 영혼조각";

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { 
            //gameObject.SetActive(false);
            tabBarController.OnTabSelected(tabBarController.characterListBtn, tabBarController.characterListPanel);
            characterScrollViewUI.ShowCharacterInfo(data); });
    }
}
