using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GachaListUI : MonoBehaviour
{
    [Header("필수 연결")]
    [Tooltip("씬에 있는 GachaManager 오브젝트")]
    public GachaManager gachaManager;

    [Tooltip("결과 아이템으로 생성할 프리팹 (CharacterPanelUI 스크립트가 있어야 함)")]
    public GameObject resultItemPrefab;

    [Tooltip("생성된 아이템들이 위치할 부모 트랜스폼 (GridLayoutGroup 등)")]
    public Transform contentParent;

    [Header("연출 설정")]
    [Tooltip("각 아이템이 등장하는 시간 간격")]
    public float delayBetweenItems = 0.2f;

    [Tooltip("개별 아이템의 등장 애니메이션 시간")]
    public float animationDuration = 0.5f;

    // GachaManager로부터 받아온 실제 PlayerCharacterData 리스트
    private List<PlayerCharacterData> gachaResultsData = new List<PlayerCharacterData>();

    void OnEnable()
    {
        if (gachaManager == null)
        {
            Debug.LogError("GachaListUI에 GachaManager가 연결되지 않았습니다!");
            return;
        }

        gachaResultsData = gachaManager.lastGachaResults;

        if (gachaResultsData == null || gachaResultsData.Count == 0)
        {
            Debug.LogWarning("GachaManager에 표시할 가챠 결과가 없습니다. 연출을 시작하지 않습니다.");
            return;
        }

        StartCoroutine(ShowResultsCoroutine());
    }

    private IEnumerator ShowResultsCoroutine()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        yield return null;

        for (int i = 0; i < gachaResultsData.Count; i++)
        {
            GameObject itemGO = Instantiate(resultItemPrefab, contentParent);

            // resultItemPrefab에 있는 CharacterPanelUI 컴포넌트를 가져옵니다.
            CharacterPanelUI panelUI = itemGO.GetComponent<CharacterPanelUI>();
            if (panelUI != null)
            {
                // CharacterPanelUI의 Setup 함수에 PlayerCharacterData를 넘겨줍니다.
                panelUI.Setup(gachaResultsData[i]);
            }
            else
            {
                Debug.LogWarning($"{resultItemPrefab.name} 프리팹에 CharacterPanelUI 스크립트가 없습니다.", itemGO);
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