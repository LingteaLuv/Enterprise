using JHT;
using System.Collections.Generic;
using _05._CSJ_Folder.Scripts.Quest;
using UnityEngine;
using UnityEngine.UI;

public class GachaListUI : MonoBehaviour
{
    [Header("프리팹 연결")]
    [Tooltip("캐릭터 결과 아이템으로 생성할 프리팹 (GachaCharacterPanel 스크립트가 있어야 함)")]
    public GameObject characterResultItemPrefab;
    [Tooltip("장비 결과 아이템으로 생성할 프리팹 (GachaItemPanel 스크립트가 있어야 함)")]
    public GameObject equipmentResultItemPrefab;

    [Header("공통 연결")]
    [Tooltip("생성된 아이템들이 위치할 부모 트랜스폼 (GridLayoutGroup 등)")]
    public Transform contentParent;
    [Tooltip("모든 카드를 한 번에 뒤집는 버튼")]
    public Button revealAllButton;
    public Button closeButton;

    private List<GachaCharacterPanel> _characterCards = new List<GachaCharacterPanel>();
    private List<GachaItemPanel> _equipmentCards = new List<GachaItemPanel>();

    private int _flippedCardCount = 0; // 뒤집힌 카드 수
    private int _totalCards = 0; // 전체 카드 수

    private void Start()
    {
        if (revealAllButton != null)
        {
            revealAllButton.onClick.AddListener(RevealAllCards);
            closeButton.onClick.AddListener(ClosePanel);
            closeButton.gameObject.SetActive(false); // 초기에는 닫기 버튼 숨김
        }
        TutorialTargets.Register("GachaList", transform as RectTransform);
    }

    /// <summary>
    /// 모든 카드(캐릭터, 장비)를 비웁니다.
    /// </summary>
    private void ClearAllCards()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        _characterCards.Clear();
        _equipmentCards.Clear();
    }

    /// <summary>
    /// 캐릭터 뽑기 결과를 받아 화면에 표시합니다.
    /// </summary>
    public void DisplayCharacterResults(List<PlayerCharacterData> characterResults, List<GachaGrade> grades)
    {
        if (characterResultItemPrefab == null)
        {
            Debug.LogError("characterResultItemPrefab이 연결되지 않았습니다!");
            return;
        }

        ClearAllCards();

        _flippedCardCount = 0; // 카드 카운터 초기화
        _totalCards = 0; // 전체 카드 수 초기화
        closeButton.gameObject.SetActive(false); // 닫기 버튼 다시 숨김
        revealAllButton.gameObject.SetActive(true);

        for (int i = 0; i < characterResults.Count; i++)
        {
            GameObject itemGO = Instantiate(characterResultItemPrefab, contentParent);
            GachaCharacterPanel panelUI = itemGO.GetComponent<GachaCharacterPanel>();
            if (panelUI != null)
            {
                panelUI.Setup(characterResults[i], grades[i]);
                panelUI.OnCardFlipped += OnAnyCardFlipped; // 카드 뒤집힘 이벤트 구독
                _characterCards.Add(panelUI);
                _totalCards++; // 전체 카드 수 증가
            }
        }
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

        ClearAllCards();

        _flippedCardCount = 0; // 카드 카운터 초기화
        _totalCards = 0; // 전체 카드 수 초기화
        closeButton.gameObject.SetActive(false); // 닫기 버튼 다시 숨김
        revealAllButton.gameObject.SetActive(true);

        foreach (var resultData in equipmentResults)
        {
            GameObject itemGO = Instantiate(equipmentResultItemPrefab, contentParent);
            if (resultData is WeaponObject itemData)
            {
                GachaItemPanel panelUI = itemGO.GetComponent<GachaItemPanel>();
                if (panelUI != null)
                {
                    PointTier tier = PointTier.Low;
                    if (itemTiers != null)
                    {
                        itemTiers.TryGetValue(itemData.itemNum, out tier);
                    }
                    panelUI.SetUp(itemData, tier);
                    panelUI.OnCardFlipped += OnAnyCardFlipped; // 카드 뒤집힘 이벤트 구독
                    _equipmentCards.Add(panelUI);
                    _totalCards++; // 전체 카드 수 증가
                }
            }
        }
    }

    /// <summary>
    /// 현재 표시된 모든 카드를 뒤집습니다.
    /// </summary>
    public void RevealAllCards()
    {
        revealAllButton.gameObject.SetActive(false);

        foreach (var card in _characterCards)
        {
            card.Flip();
        }
        foreach (var card in _equipmentCards)
        {
            card.Flip();
        }
    }
    private void ClosePanel()
    {
        // GachaUIHandler를 찾아 UI 새로고침을 요청합니다.
        GachaUIHandler gachaUI = FindObjectOfType<GachaUIHandler>();
        if (gachaUI != null)
        {
            gachaUI.RefreshUI();
            Debug.Log("뽑기 결과창이 닫히면서 메인 UI를 새로고침합니다.");
        }
        else
        {
            Debug.LogWarning("씬에서 GachaUIHandler를 찾을 수 없어 UI를 새로고침하지 못했습니다!");
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 개별 카드가 뒤집혔을 때 호출됩니다.
    /// </summary>
    private void OnAnyCardFlipped()
    {
        _flippedCardCount++;
        if (_flippedCardCount >= _totalCards)
        {
            closeButton.gameObject.SetActive(true); // 모든 카드가 뒤집히면 닫기 버튼 활성화
            revealAllButton.gameObject.SetActive(false);
        }
    }
}