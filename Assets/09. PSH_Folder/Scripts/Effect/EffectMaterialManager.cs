using UnityEngine;
using System.Collections.Generic;

public enum EffectType
{
    None, // 기본 상태
    Hit,  // 피격
    Stun, // 스턴
    Freeze, // 빙결
    ForgeFlare, // 장비 뽑기 연출
    // 나중에 추가
}

public class EffectMaterialManager : Singleton<EffectMaterialManager>
{
    [System.Serializable]
    public class EffectMaterialPair
    {
        public EffectType effectType;
        public Material material;
    }

    // 이펙트 머테리얼 목록을 인스펙터에서 설정할 수 있어요!
    public List<EffectMaterialPair> effectMaterials;

    // 실제 게임에서는 리스트보다 딕셔너리(사전)로 찾는게 훨씬 빨라요!
    private Dictionary<EffectType, Material> materialDictionary;

    protected override void Awake()
    {
        base.Awake();

        // 리스트를 딕셔너리로 변환해서 검색 속도를 빠르게 만들어줘요.
        materialDictionary = new Dictionary<EffectType, Material>();
        foreach (var pair in effectMaterials)
        {
            if (!materialDictionary.ContainsKey(pair.effectType))
            {
                materialDictionary.Add(pair.effectType, pair.material);
            }
        }
    }

    // 이펙트 타입을 주면 해당하는 머테리얼을 찾아주는 똑똑한 함수예요.
    public Material GetMaterial(EffectType effectType)
    {
        Material mat = null;
        if (materialDictionary.TryGetValue(effectType, out mat))
        {
            return mat;
        }
        else
        {
            Debug.LogWarning($"'{effectType}'에 해당하는 머테리얼을 찾을 수 없어요!");
            return null;
        }
    }
}
