using JHT;
using TMPro;
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

    void OnEnable()
    {
        // 패널이 활성화될 때마다 이벤트 구독
        ItemEventManager.Instance.OnClickPlayerItem += HandleSelected;
    }

    void OnDisable()
    {
        // 패널이 비활성화될 때 (또는 파괴될 때) 이벤트 구독 취소
        // 이렇게 하면 씬을 전환해도 유령 참조가 남지 않아요!
        ItemEventManager.Instance.OnClickPlayerItem -= HandleSelected;
    }

    public void SetUp(PlayerCharacterData cd)
    {
        soulImage.sprite = cd.characterdata.characterSprite;
        int currentFragments;
        PlayerDataManager.Instance.characterSoulFragments.TryGetValue(cd.characterdata.characterID, out currentFragments);
        countText.text = currentFragments.ToString();

        // 장현태 추가코드
        data = cd;
        // 버튼 이벤트는 중복 등록을 막기 위해 기존 리스너를 지우고 새로 추가하는 것이 안전합니다.
        itemDetailButton.onClick.RemoveAllListeners();
        itemDetailButton.onClick.AddListener(ShowItem);
    }

    #region 장현태 추가코드
    private void ShowItem()
    {
        ItemEventManager.Instance.ClickPlayerData(data);
    }


    private void HandleSelected(PlayerCharacterData clicked)
    {
        // data가 설정되지 않았을 경우를 대비한 방어 코드
        if (data == null || data.characterdata == null) return;

        PlayerCharacterData obj = (PlayerCharacterData)data;
        bool value = ReferenceEquals(clicked, obj)
                    || (clicked != null && clicked.characterdata != null && clicked.characterdata.characterID == obj.characterdata.characterID);
        curClickItem.SetActive(value);
    }
    #endregion
}
