using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Threading.Tasks;

public static class GoogleSheetImporter
{
    // 여러 CSV를 배열로 관리
    private static readonly (string url, string path)[] csvList = new (string, string)[]
    {
        (
            "https://docs.google.com/spreadsheets/d/1bkHFHadz8Dh2NA0E9lrFMsCoei2Ae3_MMPfBAfyrXhs/export?format=csv&gid=0",
            "Assets/CSVData/CharacterData.csv"
        ),
        (
            "https://docs.google.com/spreadsheets/d/1bkHFHadz8Dh2NA0E9lrFMsCoei2Ae3_MMPfBAfyrXhs/export?format=csv&gid=923429344",
            "Assets/CSVData/EquipData.csv"
        ),
        (
            "https://docs.google.com/spreadsheets/d/1bkHFHadz8Dh2NA0E9lrFMsCoei2Ae3_MMPfBAfyrXhs/export?format=csv&gid=725504099",
            "Assets/CSVData/Skills.csv"
        ),
        (
            "https://docs.google.com/spreadsheets/d/1bkHFHadz8Dh2NA0E9lrFMsCoei2Ae3_MMPfBAfyrXhs/export?format=csv&gid=1113506832",
            "Assets/CSVData/Effects.csv"
        ),
        // 필요하면 더 추가 가능
    };

    [MenuItem("Tools/Download All CSV Google Sheets")]
    public static async void DownloadAllCsv()
    {
        foreach (var csv in csvList)
        {
            Debug.Log($"다운로드 시작: {csv.path}");
            await DownloadSingleCsv(csv.url, csv.path);
        }

        Debug.Log("모든 CSV 다운로드 완료!");
        AssetDatabase.Refresh();

        //// 각 CSV별로 파싱 함수 호출
        //CharacterDataImporter.ImportData(); // 캐릭터
        //EquipDataImporter.ImportData();    // 무기
        //// 필요하면 다른 CSV 파서도 호출
        //Debug.Log("모든 데이터 파싱 완료!");
    }

    private static async Task DownloadSingleCsv(string url, string path)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"다운로드 실패: {www.error}");
                return;
            }

            File.WriteAllText(path, www.downloadHandler.text, new System.Text.UTF8Encoding(false));
            Debug.Log($"CSV 저장 완료: {path}");
        }
    }
}
