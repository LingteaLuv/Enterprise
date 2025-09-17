using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaCharacterPanel : MonoBehaviour
{
    [Header("오브젝트 연결")]
    [Tooltip("카드의 앞면 UI 그룹")]
    public GameObject frontFace;
    [Tooltip("카드의 뒷면 UI 그룹")]
    public GameObject backFace;
    [Tooltip("클릭 이벤트를 받을 버튼 (보통 backFace 자식)")]
    public Button flipButton;

    [Header("앞면 UI 요소")]
    public Image characterImage;
    public Image bg;
    public TextMeshProUGUI nameText;
    public Image crewRoleIcon;

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
    /// 캐릭터 데이터와 등급으로 카드 앞면 UI를 설정합니다.
    /// </summary>
    public void Setup(PlayerCharacterData data, GachaGrade grade)
    {
        characterImage.sprite = data.characterdata.characterSprite;
        nameText.text = data.characterdata.characterName;

        // 등급에 따라 배경색 변경
        switch ((int)grade)
        {
            case 1: bg.color = Color.white; break;
            case 2: bg.color = Color.blue; break;
            case 3: bg.color = Color.magenta; break;
            default: bg.color = Color.white; break;
        }

        crewRoleIcon.sprite = data.crewRoleIcon;
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
        targetRotation = originalRotation * Quaternion.Euler(0, -90, 0); // 다시 원래 각도로

        while (elapsed < flipDuration / 2)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, elapsed / (flipDuration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity; // 최종 각도 보정

        hasFlipped = true;
        isFlipping = false;
    }

}