using UnityEngine;
using DG.Tweening;

public class DeathEffect : MonoBehaviour
{
    [Header("연출 대상 오브젝트")]
    [SerializeField] private Transform tombstone;
    [SerializeField] private SpriteRenderer angelRing;

    [Header("연출 설정값")]
    [SerializeField] private float tombstoneDropDuration = 1f;
    [SerializeField] private float tombstoneShakeDuration = 0.5f;
    [SerializeField] private float angelRingRiseDuration = 1.5f;
    [SerializeField] private float angelRingFadeDelay = 1.2f;
    [SerializeField] private float angelRingFadeDuration = 0.3f;
    [SerializeField, Range(-3f, 3f)] private float shaderMaxValue = 1f;

    // 셰이더 제어를 위한 변수들
    private Material tombstoneMaterial;
    private readonly string shaderValueName = "_MotionBlurDist";

    private void Awake()
    {
        // 런타임에 머테리얼을 안전하게 수정하기 위해 인스턴스를 생성합니다.
        if (tombstone != null)
        {
            // .material 프로퍼티에 접근하면 Unity가 자동으로 머테리얼 인스턴스를 만들어줍니다.
            tombstoneMaterial = tombstone.GetComponent<SpriteRenderer>().material;
        }
    }

    public void Play(Vector3 characterHeadPos, Vector3 characterFeetPos)
    {
        // --- 연출 시작 전 초기화 ---
        tombstone.position = characterHeadPos;
        angelRing.transform.position = characterFeetPos;
        
        Color ringColor = angelRing.color;
        ringColor.a = 0;
        angelRing.color = ringColor;

        // 셰이더 값 0으로 초기화
        if (tombstoneMaterial != null)
        {
            tombstoneMaterial.SetFloat(shaderValueName, 0);
        }

        // --- DOTween 시퀀스 생성 ---
        Sequence sequence = DOTween.Sequence();

        // 1. 묘비가 머리 위에서 발 아래로 떨어짐
        sequence.Append(tombstone.DOMove(characterFeetPos, tombstoneDropDuration).SetEase(Ease.InQuad));

        // 2. 묘비가 땅에 부딪힌 후 살짝 흔들림
        sequence.Append(tombstone.DOShakeRotation(tombstoneShakeDuration, new Vector3(0, 0, 10), 15, 90));

        // 3. [추가] 묘비가 흔들리는 동시에 셰이더 효과 발동
        if (tombstoneMaterial != null)
        {
            var shaderTween = DOTween.To(
                () => tombstoneMaterial.GetFloat(shaderValueName),
                x => tombstoneMaterial.SetFloat(shaderValueName, x),
                shaderMaxValue,
                tombstoneShakeDuration / 2
            ).SetLoops(2, LoopType.Yoyo);

            // Insert: 묘비가 떨어지는 애니메이션이 끝나는 시점에 shaderTween을 끼워넣어 동시에 실행
            sequence.Insert(tombstoneDropDuration, shaderTween);
        }

        // 4. 천사 링이 나타나면서 위로 올라감
        sequence.Insert(tombstoneDropDuration, angelRing.DOFade(1f, 0.2f));
        sequence.Insert(tombstoneDropDuration, angelRing.transform.DOLocalMoveY(1.5f, angelRingRiseDuration).SetEase(Ease.OutQuad));

        // 5. 천사 링이 위로 올라가다 서서히 사라짐
        sequence.Insert(tombstoneDropDuration + angelRingFadeDelay, angelRing.DOFade(0f, angelRingFadeDuration));
    }

    [ContextMenu("Test Play")]
    private void TestPlay()
    {
        // Awake가 호출되도록 하기 위해, 실제 플레이 모드에서 테스트하거나
        // 이 함수를 호출하기 전에 수동으로 초기화해야 합니다.
        if (tombstoneMaterial == null && tombstone != null)
        {
            tombstoneMaterial = tombstone.GetComponent<SpriteRenderer>().material;
        }
        Play(transform.position + Vector3.up * 5f, transform.position);
    }
}