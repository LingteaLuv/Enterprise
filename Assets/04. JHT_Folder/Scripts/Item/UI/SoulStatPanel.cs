using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoulStatPanel : MonoBehaviour
{
    private int level;
    private int star;
    private BigInteger power;

    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI powerText;

    public void Init(PlayerCharacterData data)
    {
        level = data.characterLevel;
        star = data.stars;
        power = data.battlePower;

        characterImage.sprite = data.characterdata.characterSprite;
        powerText.text = data.battlePower.ToString();
    }
}
