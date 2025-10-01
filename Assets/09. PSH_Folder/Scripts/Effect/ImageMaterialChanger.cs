using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Image 컴포넌트의 재질을 변경하여 시각적 효과를 적용하는 클래스입니다.
/// </summary>
public class ImageMaterialChanger : MonoBehaviour
{
    private Image _image; // 현재 오브젝트의 Image 컴포넌트
    private Material _originalMaterial; // Image의 원래 머테리얼

    // 현재 적용된 효과들을 순서대로 저장하는 리스트
    private List<EffectType> activeEffects = new List<EffectType>();

    void Awake()
    {
        _image = GetComponent<Image>();
        if (_image != null)
        {
            _originalMaterial = _image.material;
        }
        else
        {
            Debug.LogWarning($"'{gameObject.name}'에 Image 컴포넌트가 없습니다. ImageMaterialChanger가 작동하지 않습니다.");
            enabled = false; // Image 컴포넌트가 없으면 스크립트 비활성화
        }
        Debug.Log($"'{gameObject.name}'의 원래 머테리얼을 저장했어요!");
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
        if (_image == null || activeEffects.Contains(effectType)) return;

        Debug.Log($"'{effectType}' 효과를 추가! (지속시간: {duration}초)");
        activeEffects.Add(effectType);

        if (duration > 0f)
        {
            StartCoroutine(TimedEffectCoroutine(effectType, duration));
        }

        UpdateMaterial();
    }

    // 특정 효과를 제거하는 함수예요.
    public void RemoveEffect(EffectType effectType)
    {
        if (_image == null || !activeEffects.Contains(effectType)) return;

        Debug.Log($"'{effectType}' 효과를 제거할게요!");
        activeEffects.Remove(effectType);
        UpdateMaterial();
    }

    // 현재 효과 스택에 맞춰 머테리얼을 업데이트하는 핵심 함수예요!
    private void UpdateMaterial()
    {
        if (_image == null) return;

        if (activeEffects.Any())
        {
            EffectType currentEffect = activeEffects.Last();
            Material materialToApply = EffectDatabase.Instance.GetMaterial(currentEffect);

            if (materialToApply != null)
            {
                Debug.Log($"'{currentEffect}' 효과를 표시할게요! ('{materialToApply.name}' 머테리얼 사용)");
                _image.material = materialToApply;
            }
        }
        else
        {
            RevertToOriginalMaterial();
        }
    }

    // 원래 머테리얼로 되돌리는 함수예요.
    private void RevertToOriginalMaterial()
    {
        if (_image == null) return;

        _image.material = _originalMaterial;
        Debug.Log("원래 머테리얼로 모두 돌아왔어요!");
    }

    // 머테리얼의 Float 값을 조절하는 함수
    public void SetMaterialFloat(string propertyName, float value)
    {
        if (_image == null) return;
        Debug.Log($"머테리얼 프로퍼티 '{propertyName}'의 값을 '{value}'(으)로 설정할게요.");
        _image.material.SetFloat(propertyName, value);
    }

    // 머테리얼의 Color 값을 조절하는 함수
    public void SetMaterialColor(string propertyName, Color value)
    {
        if (_image == null) return;
        Debug.Log($"머테리얼 프로퍼티 '{propertyName}'의 값을 '{value}'(으)로 설정할게요.");
        _image.material.SetColor(propertyName, value);
    }

    // 머테리얼의 Vector 값을 조절하는 함수
    public void SetMaterialVector(string propertyName, Vector4 value)
    {
        if (_image == null) return;
        Debug.Log($"머테리얼 프로퍼티 '{propertyName}'의 값을 '{value}'(으)로 설정할게요.");
        _image.material.SetVector(propertyName, value);
    }
}
