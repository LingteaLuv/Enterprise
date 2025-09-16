#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class PortraitGenerator
{
    // --- 사용자님이 설정하신 경로를 그대로 유지합니다 ---
    private const string CSV_PATH = "Assets/CSVData/Portraits.csv";
    private const string SOURCE_PATH = "Assets/00. Imports/CharImage/Portrait";
    private const string ICON_PATH = "Assets/00. Imports/CharImage/Icon";
    private const string OUTPUT_PATH = "Assets/00. Imports/CharImage/GeneratedPortraits";

    private const int IMAGE_WIDTH = 325;
    private const int IMAGE_HEIGHT = 440;

    private static readonly Dictionary<string, Color> factionColors = new Dictionary<string, Color>()
    {
        { "Pirate", new Color(0.8f, 0.2f, 0.2f) },
        { "Marine", new Color(0.2f, 0.3f, 0.8f) },
        { "Monster", new Color(0.3f, 0.8f, 0.2f) }
    };

    [MenuItem("Tools/Generate All Portraits from CSV")]
    public static void GeneratePortraits()
    {
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError($"캐릭터 CSV 파일을 찾을 수 없습니다: {CSV_PATH}");
            return;
        }

        if (!Directory.Exists(OUTPUT_PATH)) Directory.CreateDirectory(OUTPUT_PATH);

        string[] lines = File.ReadAllLines(CSV_PATH);
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV 파일에 데이터가 없습니다.");
            return;
        }

        int generatedCount = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            // 사용자님 요청대로 간단한 Split을 사용합니다.
            string[] fields = lines[i].Split(',');
            if (fields.Length < 3) continue;

            string charID = fields[0].Trim();
            string faction = fields[1].Trim();
            string portraitName = fields[2].Trim();

            // --- 1. 필요한 모든 이미지 로드 (상세 로그 기능 포함) ---
            Texture2D bgTex = LoadTexture($"{ICON_PATH}/Portrait_BG.png", charID, "배경");
            Texture2D iconTex = LoadTexture($"{ICON_PATH}/Icon_{faction}.png", charID, "아이콘");
            Texture2D charTex = LoadTexture($"{SOURCE_PATH}/{portraitName}", charID, "캐릭터");
            Texture2D upperUiTex = LoadTexture($"{ICON_PATH}/Portrait_UpperUI.png", charID, "테두리");

            if (charTex == null)
            {
                continue;
            }

            // --- 2. 픽셀 단위 합성 시작 ---
            Color[] finalPixels = new Color[IMAGE_WIDTH * IMAGE_HEIGHT];

            if (bgTex != null)
            {
                Color tintColor = factionColors.TryGetValue(faction, out Color color) ? color : Color.white;
                Color[] bgPixels = bgTex.GetPixels();
                for (int p = 0; p < bgPixels.Length; p++) { finalPixels[p] = bgPixels[p] * tintColor; }
            }
            if (iconTex != null)
            {
                Color[] iconPixels = iconTex.GetPixels();
                for (int p = 0; p < iconPixels.Length; p++) { finalPixels[p] = Color.Lerp(finalPixels[p], iconPixels[p], iconPixels[p].a * 0.1f); }
            }
            if (charTex != null)
            {
                Color[] charPixels = charTex.GetPixels();
                for (int p = 0; p < charPixels.Length; p++) { finalPixels[p] = Color.Lerp(finalPixels[p], charPixels[p], charPixels[p].a); }
            }
            if (upperUiTex != null)
            {
                Color[] uiPixels = upperUiTex.GetPixels();
                for (int p = 0; p < uiPixels.Length; p++) { finalPixels[p] = Color.Lerp(finalPixels[p], uiPixels[p], uiPixels[p].a); }
            }

            // --- 3. 최종 텍스처 생성 및 저장 ---
            Texture2D finalTexture = new Texture2D(IMAGE_WIDTH, IMAGE_HEIGHT);
            finalTexture.SetPixels(finalPixels);
            finalTexture.Apply();

            byte[] bytes = finalTexture.EncodeToPNG();
            File.WriteAllBytes($"{OUTPUT_PATH}/{charID}.png", bytes);

            generatedCount++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"포트레잇 생성 완료! 총 {generatedCount}개의 이미지가 {OUTPUT_PATH}에 저장되었습니다.");
    }

    private static Texture2D LoadTexture(string fullPath, string charID, string layerName)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
        if (tex == null)
        {
            Debug.LogWarning($"[{layerName} 레이어 누락] charID '{charID}'의 포트레잇 생성 중, 필요한 이미지 파일을 찾을 수 없습니다: {fullPath}");
            return null;
        }

        TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            AssetDatabase.Refresh();
        }

        return tex;
    }
}
#endif
