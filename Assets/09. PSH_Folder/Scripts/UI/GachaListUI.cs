using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GachaListUI : MonoBehaviour
{
    [Header("필수 연결")]
    [Tooltip("결과 아이템으로 생성할 프리팹 (CharacterPanelUI 스크립트가 있어야 함)")]
    public GameObject resultItemPrefab;

    [Tooltip("생성된 아이템들이 위치할 부모 트랜스폼 (GridLayoutGroup 등)")]
    public Transform contentParent;

    [Header("연출 설정")]
    [Tooltip("각 아이템이 등장하는 시간 간격")]
    public float delayBetweenItems = 0.1f;

    [Tooltip("개별 아이템의 등장 애니메이션 시간")]
    public float animationDuration = 0.4f;

    /// <summary>
    /// 캐릭터 뽑기 결과를 받아 화면에 표시를 시작하는 함수입니다.
    /// GachaUIHandler가 이 함수를 호출해줍니다.
    /// </summary>
    /// <param name="characterResults">표시할 캐릭터 데이터 리스트</param>
    public void DisplayCharacterResults(List<PlayerCharacterData> characterResults)
    {
        if (characterResults == null || characterResults.Count == 0)
        {
            Debug.LogWarning("표시할 가챠 결과가 없습니다.");
            return;
        }

        // 기존에 실행중인 코루틴이 있다면 중지 (연속 뽑기 시 중복 방지)
        StopAllCoroutines();
        StartCoroutine(ShowResultsCoroutine(characterResults));
    }

    private IEnumerator ShowResultsCoroutine(List<PlayerCharacterData> results)
    {
        // 기존 아이템들 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        yield return null; // 한 프레임 대기하여 삭제가 반영되도록 함

        // 새 아이템들 생성 및 연출
        foreach (var resultData in results)
        {
            GameObject itemGO = Instantiate(resultItemPrefab, contentParent);

            CharacterPanelUI panelUI = itemGO.GetComponent<CharacterPanelUI>();
            if (panelUI != null)
            {
                panelUI.Setup(resultData);
            }
            else
            {
                Debug.LogWarning($"{resultItemPrefab.name} 프리팹에 CharacterPanelUI 스크립트가 없습니다.", itemGO);
            }

            // 등장 애니메이션 실행
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
