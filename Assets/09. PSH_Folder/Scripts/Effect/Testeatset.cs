using UnityEngine;
using UnityEngine.UI;

public class Testeatset : MonoBehaviour
{
    public CharacterMaterialChanger materialChanger;
    public Button b1;
    public Button b2;
    public Button b3;
    public Button b4;
    public Button b5;

    private void Start()
    {
        b1.onClick.AddListener(TakeDamage);
        b2.onClick.AddListener(GetStunned);
        b3.onClick.AddListener(GetFrezzed);
        b4.onClick.AddListener(ShowNewMapUnlockEffect);
        b5.onClick.AddListener(D);
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

    // 이펙트를 호출할 스크립트

    // 1. 인스펙터에서 UIUnlockEffect 프리팹을 여기에 연결해주세요.
    public GameObject unlockEffectPrefab;

    // 2. UI를 담을 Canvas의 Transform을 연결해주면 더 좋아요.
    public Transform uiCanvasTransform;


    // '새로운 맵' 기능 해금 이펙트를 보여주고 싶을 때 이 함수를 호출!
    void ShowNewMapUnlockEffect()
    {
        // 3. 풀 매니저의 새로운 함수로 이펙트를 스폰하고, UIUnlockEffect 컴포넌트를 직접 받아옵니다.
        UIUnlockEffect effect = EffectPoolManager.Instance.SpawnObject<UIUnlockEffect>(unlockEffectPrefab);

        if (effect != null)
        {
            // 4. (중요!) 스폰된 UI가 올바른 캔버스에 그려지도록 부모를 설정해줍니다.
            effect.transform.SetParent(uiCanvasTransform, false);

            // 5. 마지막으로, 원하는 기능의 애니메이션을 재생하도록 명령합니다!
            effect.PlayUnlockEffect(UnlockableFeature.TenGacha);
        }
    }

    public GameObject deathEffect;
    void D()
    {
        EffectPoolManager.Instance.SpawnEffect(deathEffect, transform.position, Quaternion.identity, 5);
    }
}
