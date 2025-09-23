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

    private void OnEnable()
    {
        Play(transform.position + Vector3.up * 5f, transform.position);
    }
    public void Play(Vector3 characterHeadPos, Vector3 characterFeetPos)
    {
        // --- 연출 시작 전 초기화 ---
        tombstone.position = characterHeadPos;
        angelRing.transform.position = characterFeetPos;
        
        Color ringColor = angelRing.color;
        ringColor.a = 0;
        angelRing.color = ringColor;

        // --- DOTween 시퀀스 생성 ---
        Sequence sequence = DOTween.Sequence();

        // 1. 묘비가 머리 위에서 발 아래로 떨어짐
        sequence.Append(tombstone.DOMove(characterFeetPos, tombstoneDropDuration).SetEase(Ease.InQuad));

        // 2. 묘비가 땅에 부딪힌 후 살짝 흔들림
        sequence.Append(tombstone.DOShakeRotation(tombstoneShakeDuration, new Vector3(0, 0, 10), 15, 90));

        // 4. 천사 링이 나타나면서 위로 올라감
        sequence.Insert(tombstoneDropDuration, angelRing.DOFade(1f, 0.2f));
        sequence.Insert(tombstoneDropDuration, angelRing.transform.DOLocalMoveY(1.5f, angelRingRiseDuration).SetEase(Ease.OutQuad));

        // 5. 천사 링이 위로 올라가다 서서히 사라짐
        sequence.Insert(tombstoneDropDuration + angelRingFadeDelay, angelRing.DOFade(0f, angelRingFadeDuration));
    }

    [ContextMenu("Test Play")]
    private void TestPlay()
    {
        Play(transform.position + Vector3.up * 5f, transform.position);
    }
}