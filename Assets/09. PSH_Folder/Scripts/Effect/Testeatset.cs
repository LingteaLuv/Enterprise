using UnityEngine;
using UnityEngine.UI;

public class Testeatset : MonoBehaviour
{
    public CharacterMaterialChanger materialChanger;
    public Button b1;
    public Button b2;
    public Button b3;

    private void Start()
    {
        b1.onClick.AddListener(TakeDamage);
        b2.onClick.AddListener(GetStunned);
        b3.onClick.AddListener(GetFrezzed);
    }
    public void TakeDamage()
    {
        materialChanger.AddEffect(EffectType.Hit ,.2f);
    }

    public void GetStunned()
    {
        materialChanger.AddEffect(EffectType.Stun,1f);
    }

    public void GetFrezzed()
    {
        materialChanger.AddEffect(EffectType.Freeze,.2f);
    }

}
