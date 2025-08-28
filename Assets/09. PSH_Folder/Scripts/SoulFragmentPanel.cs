using JHT;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class SoulFragmentPanel : MonoBehaviour
{
    public Image soulImage;
    public TextMeshProUGUI countText;

    // 장현태 추가코드
    private PlayerCharacterData data;
    public Button itemDetailButton;
    public GameObject curClickItem;
    //
    public void SetUp(PlayerCharacterData cd)
    {
        soulImage.sprite = cd.characterdata.characterSprite;
        int currentFragments;
        PlayerDataManager.Instance.characterSoulFragments.TryGetValue(cd.characterdata.characterID, out currentFragments);
        countText.text = currentFragments.ToString();

        // 장현태 추가코드
        data = cd;
        itemDetailButton.onClick.AddListener(ShowItem);
        ItemEventManager.Instance.OnClickPlayerItem -= HandleSelected;
        ItemEventManager.Instance.OnClickPlayerItem += HandleSelected;
    }

    #region 장현태 추가코드
    private void ShowItem()
    {
        ItemEventManager.Instance.ClickPlayerData(data);
    }


    private void HandleSelected(PlayerCharacterData clicked)
    {
        PlayerCharacterData obj = (PlayerCharacterData)data;
        bool value = ReferenceEquals(clicked, obj)
                    || (clicked != null && clicked.characterdata.characterID == obj.characterdata.characterID);
        curClickItem.SetActive(value);
    }
    #endregion
}
