using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoulFragmentPanel : MonoBehaviour
{
    public Image soulImage;
    public TextMeshProUGUI countText;

    public void SetUp(PlayerCharacterData cd)
    {
        soulImage.sprite = cd.characterdata.characterSprite;
        int currentFragments;
        PlayerDataManager.Instance.characterSoulFragments.TryGetValue(cd.characterdata.characterID, out currentFragments);
        countText.text = currentFragments.ToString();
    }
}
