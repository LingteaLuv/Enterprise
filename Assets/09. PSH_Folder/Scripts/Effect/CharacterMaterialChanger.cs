using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterMaterialChanger : MonoBehaviour
{
    // 원래 머테리얼들을 저장
    private Dictionary<SpriteRenderer, Material> originalMaterials;
    private SpriteRenderer[] spriteRenderers;

    // 현재 적용된 효과들을 순서대로 저장하는 리스트
    private List<EffectType> activeEffects = new List<EffectType>();

    void Awake()
    {
        // 게임이 시작될 때 자식들에게서 모든 SpriteRenderer 컴포넌트를 찾아와요.
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalMaterials = new Dictionary<SpriteRenderer, Material>();

        foreach (var renderer in spriteRenderers)
        {
            // 각 렌더러의 현재 머테리얼을 사전에 저장해둬요.
            originalMaterials[renderer] = renderer.material;
        }
        Debug.Log($"'{gameObject.name}'의 원래 머테리얼들을 모두 저장했어요!");
    }

    // 지정된 시간 후에 효과를 제거하는 코루틴이에요.
    private IEnumerator TimedEffectCoroutine(EffectType effectType, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveEffect(effectType);
    }

    // 새로운 효과를 추가하는 함수예요!
    public void AddEffect(EffectType effectType, float duration = 0f)
    {
        // 이미 적용된 효과는 또 추가하지 않아요.
        if (activeEffects.Contains(effectType)) return;

        Debug.Log($"'{effectType}' 효과를 추가! (지속시간: {duration}초)");
        activeEffects.Add(effectType);

        // 지속시간이 있으면, 시간이 지난 후에 자동으로 효과를 제거하도록 예약해요.
        if (duration > 0f)
        {
            StartCoroutine(TimedEffectCoroutine(effectType, duration));
        }

        UpdateMaterial();
    }

    // 특정 효과를 제거하는 함수예요.
    public void RemoveEffect(EffectType effectType)
    {
        if (activeEffects.Contains(effectType))
        {
            Debug.Log($"'{effectType}' 효과를 제거할게요!");
            activeEffects.Remove(effectType);
            UpdateMaterial();
        }
    }

    // 현재 효과 스택에 맞춰 머테리얼을 업데이트하는 핵심 함수예요!
    private void UpdateMaterial()
    {
        // 적용할 효과가 남아있다면?
        if (activeEffects.Any()) // activeEffects.Count > 0 와 같아요!
        {
            // 리스트의 가장 마지막에 있는(가장 나중에 추가된) 효과를 적용해요.
            EffectType currentEffect = activeEffects.Last();
            Material materialToApply = EffectMaterialManager.Instance.GetMaterial(currentEffect);

            if (materialToApply != null)
            {
                Debug.Log($"'{currentEffect}' 효과를 표시할게요! ('{materialToApply.name}' 머테리얼 사용)");
                foreach (var renderer in spriteRenderers)
                {
                    renderer.material = materialToApply;
                }
            }
        }
        // 적용할 효과가 하나도 없다면?
        else
        {
            RevertToOriginalMaterial();
        }
    }

    // 원래 머테리얼로 되돌리는 함수예요.
    private void RevertToOriginalMaterial()
    {
        foreach (var renderer in spriteRenderers)
        {
            if (originalMaterials.ContainsKey(renderer))
            {
                renderer.material = originalMaterials[renderer];
            }
        }
        Debug.Log("원래 머테리얼로 모두 돌아왔어요!");
    }

    // 머테리얼의 Float 값을 조절하는 함수
    public void SetMaterialFloat(string propertyName, float value)
    {
        Debug.Log($"머테리얼 프로퍼티 '{propertyName}'의 값을 '{value}'(으)로 설정할게요.");
        foreach (var renderer in spriteRenderers)
        {
            // renderer.material은 인스턴스화된 머테리얼이라서 다른 오브젝트에 영향을 주지 않아요!
            renderer.material.SetFloat(propertyName, value);
        }
    }

    // 머테리얼의 Color 값을 조절하는 함수
    public void SetMaterialColor(string propertyName, Color value)
    {
        Debug.Log($"머테리얼 프로퍼티 '{propertyName}'의 값을 '{value}'(으)로 설정할게요.");
        foreach (var renderer in spriteRenderers)
        {
            renderer.material.SetColor(propertyName, value);
        }
    }

    // 머테리얼의 Vector 값을 조절하는 함수
    public void SetMaterialVector(string propertyName, Vector4 value)
    {
        Debug.Log($"머테리얼 프로퍼티 '{propertyName}'의 값을 '{value}'(으)로 설정할게요.");
        foreach (var renderer in spriteRenderers)
        {
            renderer.material.SetVector(propertyName, value);
        }
    }
}
