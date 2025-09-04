using JHT;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaListUI : MonoBehaviour
{
    [Header("프리팹 연결")]
    [Tooltip("캐릭터 결과 아이템으로 생성할 프리팹 (CharacterPanelUI 스크립트가 있어야 함)")]
    public GameObject characterResultItemPrefab;
    [Tooltip("장비 결과 아이템으로 생성할 프리팹 (GachaItemPanel 스크립트가 있어야 함)")]
    public GameObject equipmentResultItemPrefab;

    [Header("공통 연결")]
    [Tooltip("생성된 아이템들이 위치할 부모 트랜스폼 (GridLayoutGroup 등)")]
    public Transform contentParent;

    [Header("연출 설정")]
    [Tooltip("각 아이템이 등장하는 시간 간격")]
    public float delayBetweenItems = 0.1f;

    [Tooltip("개별 아이템의 등장 애니메이션 시간")]
    public float animationDuration = 0.4f;

    /// <summary>
    /// 캐릭터 뽑기 결과를 받아 화면에 표시를 시작하는 함수입니다.
    /// </summary>
    public void DisplayCharacterResults(List<PlayerCharacterData> characterResults, List<GachaGrade> grades)
    {
        if (characterResultItemPrefab == null)
        {
            Debug.LogError("characterResultItemPrefab이 연결되지 않았습니다!");
            return;
        }
        StartCoroutine(ShowCharacterResultsCoroutine(characterResults, grades));
    }

    /// <summary>
    /// 장비 뽑기 결과를 받아 화면에 표시를 시작하는 함수입니다.
    /// </summary>
    public void DisplayEquipmentResults(List<ItemObject> equipmentResults, Dictionary<int, PointTier> itemTiers)
    {
        if (equipmentResultItemPrefab == null)
        {
            Debug.LogError("equipmentResultItemPrefab이 연결되지 않았습니다!");
            return;
        }
        StartCoroutine(ShowEquipmentResultsCoroutine(equipmentResults, itemTiers));
    }

    private IEnumerator ShowCharacterResultsCoroutine(List<PlayerCharacterData> results, List<GachaGrade> grades)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        yield return null;

        for (int i = 0; i < results.Count; i++)
        {
            GameObject itemGO = Instantiate(characterResultItemPrefab, contentParent);
            GachaCharacterPanel panelUI = itemGO.GetComponent<GachaCharacterPanel>();
            if (panelUI != null) panelUI.Setup(results[i], grades[i]);

            StartCoroutine(AnimateItemCoroutine(itemGO));
            yield return new WaitForSeconds(delayBetweenItems);
        }
    }

    private IEnumerator ShowEquipmentResultsCoroutine(List<ItemObject> results, Dictionary<int, PointTier> itemTiers)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        yield return null;

        foreach (var resultData in results)
        {
            GameObject itemGO = Instantiate(equipmentResultItemPrefab, contentParent);
            if (resultData is WeaponObject itemData)
            {
                GachaItemPanel panelUI = itemGO.GetComponent<GachaItemPanel>();
                if (panelUI != null)
                {
                    PointTier tier = PointTier.Low; // 기본값
                    if (itemTiers != null)
                    {
                        itemTiers.TryGetValue(itemData.itemNum, out tier);
                    }
                    panelUI.SetUp(itemData, tier);
                }
            }

            StartCoroutine(AnimateItemCoroutine(itemGO));
            yield return new WaitForSeconds(delayBetweenItems);
        }
    }

    private IEnumerator AnimateItemCoroutine(GameObject itemGO)
    {
        CanvasGroup canvasGroup = itemGO.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = itemGO.AddComponent<CanvasGroup>();
        }

        Transform itemTransform = itemGO.transform;
        canvasGroup.alpha = 0f;
        itemTransform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            itemTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        itemTransform.localScale = Vector3.one;
    }

}
