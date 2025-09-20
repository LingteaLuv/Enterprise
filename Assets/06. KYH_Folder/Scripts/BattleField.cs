using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class BattleField : MonoBehaviour
{
    [field: SerializeField] public Transform PlayerSpawnPoint { get; private set; }
    [field: SerializeField] public List<Transform> EnemySpawnPoints { get; private set; }

    private SpriteRenderer[] spriteRenderers;
    private TilemapRenderer[] tilemapRenderers;

    private void Awake()
    {
        // 자동 할당
        AutoAssignEnemySpawnPoints();

        // 렌더러 수집
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        tilemapRenderers = GetComponentsInChildren<TilemapRenderer>(true);

        // 시작 시 전체 알파값 0
        SetAlpha(0f);
    }

    private void OnEnable()
    {
        Debug.Log("[BattleField] 활성화됨 → GridManager 그리드 재생성 시도");

        // 타일맵 기반 A* 그리드 생성
        if (GridManager.Instance != null)
        {
            GridManager.Instance.CreateGrid();
        }
    }

    public void SetAlpha(float alpha)
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        foreach (var tr in tilemapRenderers)
        {
            if (tr != null && tr.material != null && tr.material.HasProperty("_Color"))
            {
                Color c = tr.material.color;
                c.a = alpha;
                tr.material.color = c;
            }
        }
    }

    public void FadeIn(float duration)
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.DOFade(1f, duration);
            }
        }

        foreach (var tr in tilemapRenderers)
        {
            if (tr != null && tr.material.HasProperty("_Color"))
            {
                Color start = tr.material.color;
                DOTween.To(() => start.a, a =>
                {
                    Color c = start;
                    c.a = a;
                    tr.material.color = c;
                }, 1f, duration);
            }
        }
    }

    public void FadeOut(float duration)
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.DOFade(0f, duration);
            }
        }

        foreach (var tr in tilemapRenderers)
        {
            if (tr != null && tr.material.HasProperty("_Color"))
            {
                Color start = tr.material.color;
                DOTween.To(() => start.a, a =>
                {
                    Color c = start;
                    c.a = a;
                    tr.material.color = c;
                }, 0f, duration);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (EnemySpawnPoints == null || EnemySpawnPoints.Count == 0)
        {
            AutoAssignEnemySpawnPoints();
        }
    }
#endif

    private void AutoAssignEnemySpawnPoints()
    {
        if (EnemySpawnPoints != null && EnemySpawnPoints.Count > 0)
            return;

        EnemySpawnPoints = new List<Transform>();

        var enemyGroup = transform.Find("Point/Enemy_Spawn");

        if (enemyGroup != null)
        {
            foreach (Transform child in enemyGroup)
            {
                EnemySpawnPoints.Add(child);
            }

            Debug.Log($"[BattleField] 자동으로 {EnemySpawnPoints.Count}개의 EnemySpawnPoint를 등록했습니다.");
        }
        else
        {
            Debug.LogWarning("[BattleField] 'Point/Enemy_Spawn' 경로를 찾을 수 없습니다.");
        }
    }
}
