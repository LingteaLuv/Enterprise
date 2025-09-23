using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JHT;
using System.Collections;

public class GachaItemPanel : MonoBehaviour
{
    [Header("오브젝트 연결")]
    [Tooltip("카드의 앞면 UI 그룹")]
    public GameObject frontFace;
    [Tooltip("카드의 뒷면 UI 그룹")]
    public GameObject backFace;
    [Tooltip("클릭 이벤트를 받을 버튼 (보통 backFace 자식)")]
    public Button flipButton;

    [Header("앞면 UI 요소")]
    public Image itemImage;
    public TextMeshProUGUI nameText;
    public Image backgroundImage;
    public Image[] starImages;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI pointText;
    public Slider pointSlider;

    [Header("머테리얼")]
    [Tooltip("등급별 테두리 효과")]
    public Material[] mats;

    [Header("뒤집기 애니메이션 설정")]
    [Tooltip("뒤집기 애니메이션 시간")]
    public float flipDuration = 0.5f;

    private bool isFlipping = false;
    private bool hasFlipped = false;

    void Start()
    {
        // 처음엔 뒷면만 보이도록 설정
        frontFace.SetActive(false);
        backFace.SetActive(true);
        hasFlipped = false;
        isFlipping = false;

        if (flipButton != null)
        {
            flipButton.onClick.AddListener(Flip);
        }
    }

    /// <summary>
    /// 가챠 결과로 나온 아이템의 정보를 받아 UI를 설정합니다.
    /// </summary>
    /// <param name="weapon">표시할 아이템 데이터</param>
    /// <param name="tier">함께 획득한 강화 포인트의 등급</param>
    public void SetUp(WeaponObject weapon, PointTier tier)
    {
        if (weapon != null)
        {
            // 아이템 이름과 아이콘 설정
            if (nameText != null) this.nameText.text = weapon.itemName;
            if (itemImage != null) this.itemImage.sprite = weapon.itemIcon;

            levelText.text = $"LV {weapon.ItemLevel}";

            // 별
            UpdateStarDisplay(weapon.ItemStar);

            // 강화포인트
            UpdatePointDisplay(weapon);
        }

        // 강화 포인트 등급에 따라 배경색 설정
        if (backgroundImage != null)
        {
            backgroundImage.material = mats[(int)tier];
        }
    }

    /// <summary>
    /// 카드를 뒤집는 애니메이션을 실행합니다.
    /// </summary>
    public void Flip()
    {
        if (isFlipping || hasFlipped) return;
        StartCoroutine(FlipCoroutine());
    }

    private IEnumerator FlipCoroutine()
    {
        isFlipping = true;

        // 뒷면에서 앞면으로 절반 뒤집기
        float elapsed = 0f;
        Quaternion originalRotation = transform.rotation;
        Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 90, 0);

        while (elapsed < flipDuration / 2)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, elapsed / (flipDuration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;

        // 앞/뒷면 교체
        backFace.SetActive(false);
        frontFace.SetActive(true);

        // 앞면으로 완전히 뒤집기
        elapsed = 0f;
        originalRotation = transform.rotation;
        targetRotation = originalRotation * Quaternion.Euler(0, -90, 0);

        while (elapsed < flipDuration / 2)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, elapsed / (flipDuration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;

        hasFlipped = true;
        isFlipping = false;
        OnCardFlipped?.Invoke(); // 카드가 뒤집혔음을 알립니다.
    }

    public event System.Action OnCardFlipped; // 카드가 뒤집혔을 때 호출될 이벤트

    private void UpdateStarDisplay(int currentStars)
    {
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            var img = starImages[i];
            if (img == null) continue;  // Destroy되었거나 참조 끊긴 경우 스킵

            img.color = (i < currentStars) ? Color.yellow : Color.grey;
        }
    }

    private void UpdatePointDisplay(WeaponObject curWeapon)
    {
        int requirePoint = InventoryManager.Instance.GetRequiredPointsForLevelUp(curWeapon);
        int currentPoint = InventoryManager.Instance.GetEnhancementPoints(curWeapon.itemNum);
        pointText.text = $"{currentPoint} / {requirePoint}";
        pointSlider.value = Mathf.Min((float)currentPoint / requirePoint, 1);
    }
}
