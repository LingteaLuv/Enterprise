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

    [Header("뒤집기 애니메이션 설정")]
    [Tooltip("뒤집기 애니메이션 시간")]
    public float flipDuration = 0.5f;

    [Header("등급별 색상 설정")]
    [Tooltip("PointTier의 Low, Mid, High 순서에 맞게 색상을 지정합니다.")]
    public Color[] tierColors = new Color[3];

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
        }

        // 강화 포인트 등급에 따라 배경색 설정
        if (backgroundImage != null && tierColors.Length > (int)tier)
        {
            backgroundImage.color = tierColors[(int)tier];
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
    }
}
