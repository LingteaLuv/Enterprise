using UnityEngine;
using UnityEditor;
using System.IO;

public class ImageResizer
{
    // 결과물이 들어갈 경로
    private const string INPUT_FOLDER = "Assets/00. Imports/EquipImage";   // 원본 PNG 폴더
    private const string OUTPUT_FOLDER = "Assets/00. Imports/EquipImage_Resized"; // 결과 PNG 폴더
    private const int TARGET_WIDTH = 512;
    private const int TARGET_HEIGHT = 512;

    [MenuItem("Tools/Resize PNGs")]
    public static void ResizePNGs()
    {
        if (!Directory.Exists(OUTPUT_FOLDER))
            Directory.CreateDirectory(OUTPUT_FOLDER);

        // PNG 파일만 검색
        string[] pngFiles = Directory.GetFiles(INPUT_FOLDER, "*.png", SearchOption.AllDirectories);

        int count = 0;
        foreach (var path in pngFiles)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogWarning($"로드 실패: {path}");
                continue;
            }

            // 리사이즈 (비율 유지 + 중앙정렬)
            Texture2D resized = ResizeAndCenter(tex, TARGET_WIDTH, TARGET_HEIGHT);

            // PNG로 저장
            string fileName = Path.GetFileName(path);
            string savePath = Path.Combine(OUTPUT_FOLDER, fileName);
            File.WriteAllBytes(savePath, resized.EncodeToPNG());

            Object.DestroyImmediate(resized);
            count++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"장비 PNG {count}개를 {TARGET_WIDTH}x{TARGET_HEIGHT}로 맞춰 {OUTPUT_FOLDER}에 저장했습니다.");
    }

    private static Texture2D ResizeAndCenter(Texture2D source, int width, int height)
    {
        // 비율 유지해서 최대 크기 계산
        float srcRatio = (float)source.width / source.height;
        float dstRatio = (float)width / height;

        int newW, newH;
        if (srcRatio > dstRatio)
        {
            newW = width;
            newH = Mathf.RoundToInt(width / srcRatio);
        }
        else
        {
            newH = height;
            newW = Mathf.RoundToInt(height * srcRatio);
        }

        // 먼저 소스를 newW,newH로 리샘플링
        RenderTexture rt = RenderTexture.GetTemporary(newW, newH);
        Graphics.Blit(source, rt);

        Texture2D scaled = new Texture2D(newW, newH, TextureFormat.ARGB32, false);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        scaled.ReadPixels(new Rect(0, 0, newW, newH), 0, 0);
        scaled.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        // 최종 캔버스에 중앙 배치
        Texture2D finalTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Color32[] clear = new Color32[width * height];
        for (int i = 0; i < clear.Length; i++) clear[i] = new Color32(0, 0, 0, 0);
        finalTex.SetPixels32(clear);

        int x = (width - newW) / 2;
        int y = (height - newH) / 2;
        finalTex.SetPixels(x, y, newW, newH, scaled.GetPixels());
        finalTex.Apply();

        Object.DestroyImmediate(scaled);
        return finalTex;
    }
}
