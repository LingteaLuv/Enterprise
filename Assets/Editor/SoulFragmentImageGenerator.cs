using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SoulFragmentImageGenerator
{
    // --- 경로 설정 (사용자님이 변경한 경로 유지) ---
    private const string CSV_PATH = "Assets/CSVData/Portraits.csv";
    private const string SOURCE_PATH = "Assets/00. Imports/CharImage/Portrait";
    private const string COMMON_ICON_PATH = "Assets/00. Imports/CharImage/Icon";
    private const string OUTPUT_PATH = "Assets/00. Imports/CharImage/SoulFragmentImage";

    // --- 생성될 이미지 크기 설정 ---
    private const int IMAGE_WIDTH = 325;
    private const int IMAGE_HEIGHT = 440;

    [MenuItem("Tools/Generate Soul Fragment Images from CSV")]
    public static void GenerateImages()
    {
        // --- 1. 경로 및 파일 확인 ---
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {CSV_PATH}");
            return;
        }
        if (!Directory.Exists(OUTPUT_PATH))
        {
            Directory.CreateDirectory(OUTPUT_PATH);
        }

        // --- 2. 재료 준비 ---
        Material maskMaterial = new Material(Shader.Find("Custom/MaskedShader"));
        // 셰이더의 _Color 속성 알파를 설정하여 전체 투명도 조절
        maskMaterial.SetColor("_Color", new Color(1, 1, 1, .8f));

        Material blendMaterial = new Material(Shader.Find("Sprites/Default"));

        if (maskMaterial == null || blendMaterial == null)
        {
            Debug.LogError("필요한 셰이더(Custom/MaskedShader, Sprites/Default)를 찾을 수 없습니다.");
            return;
        }

        // 공통으로 사용할 배경 및 프레임 로드
        Texture2D backgroundTex = LoadTexture($"{COMMON_ICON_PATH}/BG.png");
        Texture2D frameTex = LoadTexture($"{COMMON_ICON_PATH}/FRAME.png");
        Texture2D glassTex = LoadTexture($"{COMMON_ICON_PATH}/GLASS.png");

        if (backgroundTex == null || frameTex == null)
        {
            Debug.LogError("공용 배경(Portrait_BG.png) 또는 프레임(Frame.png) 이미지를 찾을 수 없습니다.");
            return;
        }

        RenderTexture rt = RenderTexture.GetTemporary(IMAGE_WIDTH, IMAGE_HEIGHT, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);

        // --- 3. CSV 헤더를 분석해서 데이터 파싱 ---
        string[] lines = File.ReadAllLines(CSV_PATH);
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV 파일에 데이터가 없습니다.");
            return;
        }

        string[] headers = lines[0].Split(',');
        var headerMap = new Dictionary<string, int>();
        for (int h = 0; h < headers.Length; h++)
        {
            headerMap[headers[h].Trim()] = h;
        }

        if (!headerMap.ContainsKey("charID") || !headerMap.ContainsKey("portrait"))
        {
            Debug.LogError("CSV 파일에 'charID' 또는 'portrait' 헤더가 없습니다.");
            return;
        }

        int generatedCount = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');

            string charID = fields[headerMap["charID"]].Trim();
            string sourceImageName = fields[headerMap["portrait"]].Trim();

            // --- 4. CSV 데이터로 초상화 이미지 로드 ---
            Texture2D sourceTex = LoadTexture($"{SOURCE_PATH}/{sourceImageName}");
            if (sourceTex == null)
            {
                Debug.LogWarning($"[{charID}] 원본 이미지를 찾을 수 없습니다: {sourceImageName}");
                continue;
            }

            // --- 5. GPU를 이용해 레이어 순서대로 합성 ---
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear); // 렌더 텍스처를 투명하게 비움

            // 1. 배경
            maskMaterial.SetTexture("_Mask", backgroundTex);

            Graphics.Blit(backgroundTex, rt, maskMaterial);

            // 2. 캐릭터는 마스크 머티리얼로 그리기
            Graphics.Blit(sourceTex, rt, maskMaterial);

            Graphics.Blit(glassTex, rt, blendMaterial);
            // 3. 마지막에 프레임 PNG를 마스크 없이 올리기
            Graphics.Blit(frameTex, rt, blendMaterial);

            // --- 6. 최종 결과물을 Texture2D로 변환 및 저장 ---
            Texture2D finalTexture = new Texture2D(IMAGE_WIDTH, IMAGE_HEIGHT, TextureFormat.ARGB32, false);
            finalTexture.ReadPixels(new Rect(0, 0, IMAGE_WIDTH, IMAGE_HEIGHT), 0, 0);
            finalTexture.Apply();
            RenderTexture.active = null;

            byte[] bytes = finalTexture.EncodeToPNG();
            File.WriteAllBytes($"{OUTPUT_PATH}/{charID}.png", bytes);

            Object.DestroyImmediate(finalTexture);
            generatedCount++;
        }

        // --- 7. 마무리 ---
        RenderTexture.ReleaseTemporary(rt);
        AssetDatabase.Refresh();
        Debug.Log($"영혼 조각 이미지 생성 완료! 총 {generatedCount}개의 이미지가 {OUTPUT_PATH}에 저장되었습니다.");
    }

    private static Texture2D LoadTexture(string fullPath)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
        if (tex == null) return null;

        TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
        if (importer != null && (!importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed))
        {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            AssetDatabase.Refresh();
        }

        return tex;
    }
}
