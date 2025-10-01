using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class ShieldSpin_SkillEffect : SkillEffectAnim
{
    [Header("Curve")]
    [SerializeField] float durationForward = 0.7f;
    [SerializeField] float durationBack = 0.7f;
    [SerializeField] float arc = 1.6f;         // 활 높이
    [SerializeField, Range(8, 72)] int samples = 28;       // 샘플 포인트 수
    [SerializeField, Range(0.5f, 1.5f)] float smoothK = 1f;// 접선 연속 가중치

    [Header("Rotation")]
    [SerializeField] bool rotateAlongPath = false;

    [Header("Fade")]
    [SerializeField] GameObject fadeTarget;
    [SerializeField] float fadeTo = 0.3f;
    [SerializeField] float fadeDuration = 0.5f;

    Tween pathTween, fadeTween, turn1, turn2;
    
    public override void Init(Transform target)
    {
        base.Init(target);

        Vector3 p0 = transform.position;
        Vector3 p3 = target.position;
        if ((p3 - p0).sqrMagnitude < float.MinValue) return;

        // 기본 방향/수직
        Vector3 dir = (p3 - p0);
        Vector3 perp = new Vector3(-dir.y, dir.x, 0f).normalized;

        // 전진 베지어 컨트롤
        Vector3 p1 = Vector3.Lerp(p0, p3, 0.25f) + perp * (+arc);
        Vector3 p2 = Vector3.Lerp(p0, p3, 0.75f) + perp * (+arc);

        // 복귀 베지어 컨트롤 (부메랑: 반대 굽힘)
        // 중간에서 멈춤/튐 방지 위해 '접선 연속' 맞추기
        // forward의 마지막 접선: (p3 - p2) 방향
        // return의 첫 컨트롤(q1)을 그 방향으로 이어 붙임
        Vector3 q0 = p3;
        Vector3 q3 = p0;
        Vector3 q1 = q0 + (q0 - p2) * smoothK;       // p3에서 나갈 때 접선 연속
        Vector3 q2 = q3 + (q3 - p1) * smoothK;       // p0로 들어올 때 접선 연속

        // 복귀는 반대쪽으로 휘게 perp의 부호를 뒤집는 것도 가능
        // (이미 접선 연속으로 자연스러워 보이면 아래는 생략해도 됨)
        //q1 += perp * (-arc * 0.0f); // 필요시 미세 조정
        //q2 += perp * (-arc * 0.0f);

        // 샘플링
        var forward = SampleBezier(p0, p1, p2, p3, samples);
        var back = SampleBezier(q0, q1, q2, q3, samples);

        // 중간에서 정지 느낌 방지: p3를 중복 삽입하지 않기
        // (forward의 마지막 점 == p3, back의 첫 점 == p3)
        var path = new List<Vector3>(forward.Length + back.Length - 1);
        path.AddRange(forward);
        for (int i = 1; i < back.Length; i++)
            path.Add(back[i]);

        float totalDuration = durationForward + durationBack;

        // 단일 DOPath로 왕복 전체 재생 → 중간에 감속 없음 (Linear)
        pathTween = transform
            .DOPath(path.ToArray(), totalDuration, PathType.Linear, rotateAlongPath ? PathMode.Full3D : PathMode.Ignore)
            .SetEase(Ease.Linear) // 중간(타겟)에서 감속/정지 방지
            .OnComplete(() => { pathTween?.Kill(); pathTween = null; });

        // 페이드
        if (fadeTarget != null)
        {
            var sr = fadeTarget.GetComponentInChildren<SpriteRenderer>();

            if (sr != null) 
                fadeTween = sr.DOFade(fadeTo, fadeDuration);

            turn1 = fadeTarget.transform.DORotate(new Vector3(0, 0, 180), 1f);
            turn2 = fadeTarget.transform.DORotate(new Vector3(0, 0, 360), 1f);
            sequence.Append(turn1);
            sequence.Append(turn2);
        }
    }

    private Vector3[] SampleBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int count)
    {
        var pts = new List<Vector3>(count);
        for (int i = 0; i < count; i++)
        {
            float t = i / (count - 1f);
            pts.Add(CubicBezier(p0, p1, p2, p3, t));
        }
        return pts.ToArray();
    }

    private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return (u * u * u) * p0 + 3f * (u * u * t) * p1 + 3f * (u * t * t) * p2 + (t * t * t) * p3;
    }

    protected override void OnDisable()
    {
        pathTween?.Kill(); pathTween = null;
        fadeTween?.Kill(); fadeTween = null;
    }
}
