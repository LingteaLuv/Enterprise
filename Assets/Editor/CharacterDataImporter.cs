using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CharacterDataImporter
{
    // CSV 파일이 위치한 실제 경로
    private static string csvFilePath = Application.dataPath + "/Editor/CSVData/CharacterData.csv";

    // ScriptableObject 에셋을 저장할 기본 경로
    private static string soSavePath = "Assets/Resources/CharacterData/";

    // 상단 메뉴에 "Tools/Import Character Data" 메뉴를 추가합니다.
    [MenuItem("Tools/Import Character Data")]
    public static void ImportData()
    {
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {csvFilePath}. 경로를 확인해주세요.");
            return;
        }

        // CSV 파일의 모든 라인을 읽어옵니다.
        // BOM이 포함된 UTF-8 파일을 위해 Encoding.UTF8을 명시적으로 사용
        string[] lines = File.ReadAllLines(csvFilePath, System.Text.Encoding.UTF8);

        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 파일에 헤더만 있거나 데이터가 없습니다.");
            return;
        }

        // --- 헤더 파싱 ---
        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        Dictionary<string, int> headerMap = new Dictionary<string, int>();
        for (int i = 0; i < headers.Length; i++)
        {
            if (!headerMap.ContainsKey(headers[i]))
            {
                headerMap.Add(headers[i], i);
            }
            else
            {
                Debug.LogWarning($"CSV 헤더에 중복된 이름이 있습니다: {headers[i]}");
            }
        }

        // SO 저장 경로 폴더가 없으면 생성합니다.
        if (!Directory.Exists(soSavePath))
        {
            Directory.CreateDirectory(soSavePath);
        }

        // --- 데이터 파싱 및 SO 생성 ---
        for (int i = 1; i < lines.Length; i++) // 첫 번째 줄(헤더)은 건너뛰고 두 번째 줄부터 시작합니다.
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue; // 비어있는 줄은 건너뜀

            string[] fields = line.Split(','); // 쉼표(,)로 데이터를 분리합니다.

            if (fields.Length != headers.Length)
            {
                Debug.LogWarning($"CSV 데이터 열 개수 불일치 (줄 {i + 1}): {line}. 이 줄은 건너웁니다.");
                continue;
            }

            CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
            data.baseStats = new List<StatData>(); // baseStats 리스트 초기화

            try
            {
                // 기본 정보 파싱
                data.characterID = int.Parse(fields[headerMap["characterID"]]);
                data.characterName = fields[headerMap["characterName"]];
                data.description = fields[headerMap["description"]];

                // Sprite 로드 (Resources 폴더 내에 스프라이트가 있어야 함)
                string spritePath = fields[headerMap["spritePath"]];
                data.characterSprite = Resources.Load<Sprite>(spritePath);
                if (data.characterSprite == null)
                {
                    Debug.LogWarning($"스프라이트를 로드할 수 없습니다: {spritePath} (줄 {i + 1})");
                }

                // Enum 파싱
                // Rarity는 CharacterData.cs에 정의된 Enum을 사용합니다.
                data.rarity = (Rarity)Enum.Parse(typeof(Rarity), fields[headerMap["rarity"]], true); // 대소문자 무시
                data.crewRole = (CrewRole)Enum.Parse(typeof(CrewRole), fields[headerMap["crewRole"]], true);
                data.faction = (Faction)Enum.Parse(typeof(Faction), fields[headerMap["faction"]], true);

                // 스탯 정보 파싱 및 StatData 리스트에 추가
                string[] statHeaders = new string[] { "attackPower", "health", "defensePower", "critChance", "critDamage", "attackSpeed" };
                foreach (string statName in statHeaders)
                {
                    if (headerMap.ContainsKey(statName))
                    {
                        StatData stat = new StatData();
                        stat.statName = statName;
                        stat.value = float.Parse(fields[headerMap[statName]]); // float으로 파싱
                        // StatData에 Validate() 메서드가 있다면 호출 (현재 float 버전 StatData에는 필요 없음)
                        // stat.Validate();
                        data.baseStats.Add(stat);
                    }
                    else
                    {
                        Debug.LogWarning($"CSV에 스탯 헤더 '{statName}'가 없습니다. (줄 {i + 1})");
                    }
                }

                // SO 에셋을 저장할 경로를 지정합니다.
                string assetPath = $"{soSavePath}{data.characterID}.asset";

                // 기존 에셋이 있다면 삭제하고 새로 생성 (덮어쓰기)
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.CreateAsset(data, assetPath);
            }
            catch (FormatException e)
            {
                Debug.LogError($"CSV 데이터 형식 오류 (줄 {i + 1}, ID: {fields[headerMap["characterID"]] ?? "N/A"}): 숫자/Enum 파싱 실패. 에러: {e.Message}");
                continue;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogError($"CSV 헤더 오류 (줄 {i + 1}, ID: {fields[headerMap["characterID"]] ?? "N/A"}): 필수 헤더를 찾을 수 없습니다. 에러: {e.Message}");
                continue;
            }
            catch (Exception e)
            {
                Debug.LogError($"CSV 파싱 중 알 수 없는 오류 (줄 {i + 1}, ID: {fields[headerMap["characterID"]] ?? "N/A"}): 에러: {e.Message}");
                continue;
            }
        }

        // 변경된 에셋을 저장하고 데이터베이스를 새로고침합니다.
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("캐릭터 데이터 임포트 완료!");
    }
}