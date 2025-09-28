using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

#if TMP_PRESENT || UNITY_TEXTMESHPRO
using TMPro;   // TextMeshPro 지원
#endif

using UnityEngine.UI;

/// <summary>
/// Back(처음) → Front(결과)로 종이를 휘며 뒤집는 가챠 카드 연출
/// - FrontFaceRoot/BackFaceRoot 하위 오브젝트 자동 활성/비활성
/// - Front 표시 타이밍부터 알파 0→1 페이드 (CanvasGroup + Renderer 머티리얼 알파)
/// - TryPlayFlip(): 이미 재생 중/완료된 카드는 무시 (1회 실행 보장)
/// </summary>
[ExecuteAlways]
public class PaperFlip : MonoBehaviour
{
    // ===== Mesh =====
    [Header("Mesh Settings")]
    [Range(2, 200)] public int widthSegments = 48;
    [Range(2, 200)] public int heightSegments = 24;
    public Vector2 size = new Vector2(1.5f, 2.0f);

    // ===== Texturing =====
    [Header("Sprites (Optional)")]
    public Sprite frontSprite;   // Front 면 기본 그림
    public Sprite backSprite;    // Back 면 기본 그림

    [Header("Materials (Optional)")]
    public Material frontMaterial;
    public Material backMaterial;

    // ===== Motion =====
    [Header("Flip Motion")]
    public float duration = 0.8f;             // 총 연출 시간(초)
    public float maxCurl = 1.8f;              // 휘어짐 강도(라디안)
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool flipLeftToRight = true;       // true: 왼→오, false: 오→왼
    [Range(0.3f, 0.7f)] public float halfToggleAt = 0.5f; // Front 켜질 타이밍(k기준)

    // ===== Faces =====
    [Header("Face Roots (attach children here)")]
    public Transform frontFaceRoot; // 결과 이름/초상/TMP 등
    public Transform backFaceRoot;  // 뒷면 표현 (보통 비어둠)

    // ===== Front Binding (선택) =====
#if TMP_PRESENT || UNITY_TEXTMESHPRO
    public TMP_Text frontNameText;
    public TMP_Text frontRarityText;
#else
    public Text frontNameText;
    public Text frontRarityText;
#endif
    public Image frontPortraitImage;

    // ===== Events =====
    [Header("Events")]
    public UnityEvent onFlipStart;
    public UnityEvent onFlipHalf; // Front 켜질 때
    public UnityEvent onFlipEnd;

    // ===== State =====
    [Header("API / State")]
    public bool autoCreateOnPlay = true;
    [System.NonSerialized] public bool isPlaying = false;
    [System.NonSerialized] public bool hasFlipped = false; //  1회 실행 가드

    // internal
    Mesh _meshFront, _meshBack;
    Vector3[] _baseVerts, _workVerts;
    MeshFilter _frontMF, _backMF;
    MeshRenderer _frontMR, _backMR;
    Vector2[] _frontUVCache;

    // fade cache
    CanvasGroup _frontCanvasGroup;
    readonly List<Renderer> _frontRenderers = new List<Renderer>();

    void OnEnable()
    {
        if (autoCreateOnPlay) CreateOrRefresh();

        // 시작은 Back 상태
        SetFaceVisibility(false);
        float dir = flipLeftToRight ? 1f : -1f;
        transform.localRotation = Quaternion.Euler(0f, 180f * dir, 0f);
        SetBackUVFlip(false);
    }

    void OnValidate()
    {
        if (isActiveAndEnabled) CreateOrRefresh();
    }

    // ===== 생성/세팅 =====
    public void CreateOrRefresh()
    {
        var cr = GetComponent<CanvasRenderer>();
        if (cr) DestroyImmediate(cr); // UI용 제거

        EnsureChild("Front", out _frontMF, out _frontMR);
        EnsureChild("Back", out _backMF, out _backMR);

        if (!frontFaceRoot)
        {
            var go = new GameObject("FrontFaceRoot");
            go.transform.SetParent(transform, false);
            frontFaceRoot = go.transform;
        }
        if (!backFaceRoot)
        {
            var go = new GameObject("BackFaceRoot");
            go.transform.SetParent(transform, false);
            backFaceRoot = go.transform;
        }

        if (frontMaterial == null) frontMaterial = CreateDefaultMat();
        if (backMaterial == null) backMaterial = CreateDefaultMat();
        if (frontSprite) ApplySprite(frontMaterial, frontSprite);
        if (backSprite) ApplySprite(backMaterial, backSprite);

        _frontMR.sharedMaterial = frontMaterial;
        _backMR.sharedMaterial = backMaterial;

        BuildGridMesh(ref _meshFront, widthSegments, heightSegments, size);
        _frontMF.sharedMesh = _meshFront;

        _meshBack = Instantiate(_meshFront);
        InvertMesh(_meshBack);
        _backMF.sharedMesh = _meshBack;

        _frontUVCache = _meshFront.uv;
        _baseVerts = _meshFront.vertices;
        _workVerts = new Vector3[_baseVerts.Length];

        _frontMR.sortingOrder = 0;
        _backMR.sortingOrder = -1;

        CacheFrontFadeTargets();

        // 초기 Back 상태 모습 세팅
        SetFaceVisibility(false);
        float dir = flipLeftToRight ? 1f : -1f;
        transform.localRotation = Quaternion.Euler(0f, 180f * dir, 0f);
        SetBackUVFlip(false);
    }

    void EnsureChild(string name, out MeshFilter mf, out MeshRenderer mr)
    {
        var t = transform.Find(name);
        if (!t)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            mf = go.AddComponent<MeshFilter>();
            mr = go.AddComponent<MeshRenderer>();
        }
        else
        {
            mf = t.GetComponent<MeshFilter>() ?? t.gameObject.AddComponent<MeshFilter>();
            mr = t.GetComponent<MeshRenderer>() ?? t.gameObject.AddComponent<MeshRenderer>();
        }
    }

    static void BuildGridMesh(ref Mesh mesh, int segX, int segY, Vector2 size)
    {
        if (mesh == null) mesh = new Mesh();
        mesh.name = "PaperGrid";

        int vX = segX + 1, vY = segY + 1;
        var verts = new Vector3[vX * vY];
        var uv = new Vector2[verts.Length];
        var tris = new int[segX * segY * 6];

        float w = size.x, h = size.y;

        for (int y = 0; y < vY; y++)
        {
            float vy = Mathf.Lerp(-h * 0.5f, h * 0.5f, (float)y / segY);
            float uvy = (float)y / segY;
            for (int x = 0; x < vX; x++)
            {
                float vx = Mathf.Lerp(-w * 0.5f, w * 0.5f, (float)x / segX);
                float uvx = (float)x / segX;
                int i = y * vX + x;
                verts[i] = new Vector3(vx, vy, 0);
                uv[i] = new Vector2(uvx, uvy);
            }
        }

        int ti = 0;
        for (int y = 0; y < segY; y++)
        {
            for (int x = 0; x < segX; x++)
            {
                int i = y * vX + x;
                tris[ti++] = i;
                tris[ti++] = i + vX;
                tris[ti++] = i + 1;

                tris[ti++] = i + 1;
                tris[ti++] = i + vX;
                tris[ti++] = i + vX + 1;
            }
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    static void InvertMesh(Mesh m)
    {
        var t = m.triangles;
        for (int i = 0; i < t.Length; i += 3)
        {
            int tmp = t[i];
            t[i] = t[i + 1];
            t[i + 1] = tmp;
        }
        m.triangles = t;

        var n = m.normals;
        if (n != null && n.Length == m.vertexCount)
        {
            for (int i = 0; i < n.Length; i++) n[i] = -n[i];
            m.normals = n;
        }
    }

    // 뒤면 UV를 앞면과 동일/반전으로 토글
    void SetBackUVFlip(bool flip)
    {
        if (_meshBack == null || _frontUVCache == null) return;
        var uv = new Vector2[_frontUVCache.Length];
        for (int i = 0; i < uv.Length; i++)
        {
            float u = _frontUVCache[i].x;
            uv[i] = _frontUVCache[i];
            uv[i].x = flip ? (1f - u) : u;
        }
        _meshBack.uv = uv;
    }

    void SetFaceVisibility(bool frontVisible)
    {
        if (frontFaceRoot) frontFaceRoot.gameObject.SetActive(frontVisible);
        if (backFaceRoot) backFaceRoot.gameObject.SetActive(!frontVisible);
    }

    // ===== 실행 API =====

    /// <summary>외부에서 안전 실행: 이미 재생 중/완료면 false</summary>
    public bool TryPlayFlip()
    {
        if (!isActiveAndEnabled) return false;
        if (isPlaying || hasFlipped) return false;
        PlayFlip();
        return true;
    }

    /// <summary>강제 실행(가드 없음)</summary>
    public void PlayFlip()
    {
        if (_meshFront == null) CreateOrRefresh();
        StopAllCoroutines();
        StartCoroutine(CoFlipFromBackToFront());
    }

    /// <summary>상태 초기화(테스트/재사용)</summary>
    public void ResetFlipState()
    {
        StopAllCoroutines();
        isPlaying = false;
        hasFlipped = false;
        SetFaceVisibility(false);
        float dir = flipLeftToRight ? 1f : -1f;
        transform.localRotation = Quaternion.Euler(0f, 180f * dir, 0f);
        SetBackUVFlip(false);
        // 알파 초기화
        if (_frontCanvasGroup) _frontCanvasGroup.alpha = 0f;
        SetRenderersAlpha(_frontRenderers, 0f);
    }

    IEnumerator CoFlipFromBackToFront()
    {
        isPlaying = true;
        onFlipStart?.Invoke();

        float w = size.x;
        float dir = flipLeftToRight ? 1f : -1f;
        bool halfToggled = false;

        // 시작: Back만 보임
        SetFaceVisibility(false);
        SetBackUVFlip(false);

        if (_frontCanvasGroup) _frontCanvasGroup.alpha = 0f;
        SetRenderersAlpha(_frontRenderers, 0f);

        float t = 0f;
        while (t < 1f)
        {
            float k = curve.Evaluate(t);
            float curl = maxCurl * Mathf.Sin(k * Mathf.PI);

            float sumX = 0f;                    //  추가: X 평균 계산용
            var src = _baseVerts;
            for (int i = 0; i < src.Length; i++)
            {
                Vector3 p = src[i];
                float nx = (p.x / (w * 0.5f));  // -1..1
                float theta = curl * nx * dir;

                // (기존과 동일한 원통 말림 근사)
                float r = w / Mathf.Max(0.001f, maxCurl);
                float newX = p.x * Mathf.Cos(theta);
                float newZ = r * (1f - Mathf.Cos(theta)) * dir;

                _workVerts[i] = new Vector3(newX, p.y, newZ);
                sumX += newX;                   //  누적
            }

            //  중심 보정: X 평균만큼 모두 반대로 이동 → “제자리에서” 뒤집힘
            float offsetX = sumX / _workVerts.Length;
            for (int i = 0; i < _workVerts.Length; i++)
            {
                var v = _workVerts[i];
                v.x -= offsetX;
                _workVerts[i] = v;
            }

            _meshFront.vertices = _workVerts;
            _meshFront.RecalculateNormals();
            _meshFront.RecalculateBounds();

            _meshBack.vertices = _workVerts;
            _meshBack.RecalculateNormals();
            _meshBack.RecalculateBounds();

            // 회전: 180°(Back) → 0°(Front)
            transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(180f * dir, 0f, k), 0f);

            // 중간 전환: Front 표시 + 페이드 시작
            if (!halfToggled && k >= halfToggleAt)
            {
                SetFaceVisibility(true);
                onFlipHalf?.Invoke();
                halfToggled = true;
            }

            // Front 페이드: halfToggleAt ~ 1.0
            if (halfToggled)
            {
                float u = Mathf.InverseLerp(halfToggleAt, 1f, k); // 0→1
                // 원하는 감속/가속 적용 가능: u = Mathf.SmoothStep(0,1,u);
                if (_frontCanvasGroup) _frontCanvasGroup.alpha = u;
                SetRenderersAlpha(_frontRenderers, u);
            }

            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            yield return null;
        }

        // 종료: Front 고정
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        SetFaceVisibility(true);
        if (_frontCanvasGroup) _frontCanvasGroup.alpha = 1f;
        SetRenderersAlpha(_frontRenderers, 1f);

        isPlaying = false;
        hasFlipped = true; //  1회 완료
        onFlipEnd?.Invoke();
    }

    // ===== Fade Utils =====
    void CacheFrontFadeTargets()
    {
        _frontCanvasGroup = frontFaceRoot ? frontFaceRoot.GetComponent<CanvasGroup>() : null;
        _frontRenderers.Clear();
        if (frontFaceRoot)
        {
            foreach (var r in frontFaceRoot.GetComponentsInChildren<Renderer>(true))
                _frontRenderers.Add(r);
        }
    }

    void SetRenderersAlpha(List<Renderer> renderers, float alpha)
    {
        foreach (var r in renderers)
        {
            if (!r) continue;
            var mats = r.materials; // 개별 인스턴스(런타임) 주의
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = alpha;
                    m.color = c;
                }
            }
        }
    }

    // ===== Material Utils =====
    bool IsURP() => GraphicsSettings.currentRenderPipeline != null;

    Material CreateDefaultMat()
    {
        Shader sh = IsURP()
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Transparent");
        return new Material(sh);
    }

    void ApplySprite(Material m, Sprite s)
    {
        if (!m || !s) return;
        string prop = m.HasProperty("_BaseMap") ? "_BaseMap"
                     : m.HasProperty("_MainTex") ? "_MainTex" : null;
        if (prop == null) return;
        m.SetTexture(prop, s.texture);
    }

    // ===== Front 데이터 바인딩 =====
    /// <summary>Front에 표시할 데이터 바인딩</summary>
    public void SetFrontContent(string charName, Sprite portrait, int rarity, string rarityFormat = "★{0}")
    {
        if (frontNameText) frontNameText.text = charName ?? "";
        if (frontPortraitImage)
        {
            frontPortraitImage.sprite = portrait;
            if (frontPortraitImage.type == Image.Type.Simple)
                frontPortraitImage.preserveAspect = true;
            frontPortraitImage.enabled = (portrait != null);
        }
        if (frontRarityText) frontRarityText.text = (rarity >= 0) ? string.Format(rarityFormat, rarity) : "";
    }
}

