using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CharacterDataImporter
{
    // CSV 파일이 위치한 실제 경로
    private static string csvFilePath = Application.dataPath + "/CSVData/CharacterData.csv";

    // ScriptableObject 에셋을 저장할 기본 경로
    private static string soSavePath = "Assets/Resources/CharacterData/";

    [MenuItem("Tools/Import Data/Character")]
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

        if (lines.Length <= 4)
        {
            Debug.LogWarning("CSV 파일에 헤더만 있거나 데이터가 없습니다.");
            return;
        }

        // --- 헤더 파싱(2번째 줄) ---
        string[] headers = lines[1].Split(',').Select(h => h.Trim()).ToArray();
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

        // --- 데이터 파싱 및 SO 생성(4번째 줄부터) ---
        for (int i = 3; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue; // 비어있는 줄은 건너뜀

            List<string> fieldList = ParseCsvLine(line);
            string[] fields = fieldList.Select(f => f.Trim('"')).ToArray();

            if (fields.Length != headers.Length)
            {
                Debug.LogWarning($"CSV 데이터 열 개수 불일치 (줄 {i + 1}): {line}. 이 줄은 건너웁니다.");
                continue;
            }

            //// 필드 값 앞뒤의 큰따옴표를 제거합니다.
            //for (int j = 0; j < fields.Length; j++)
            //{
            //    fields[j] = fields[j].Trim('"');
            //}

            CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
            data.baseStats = new List<StatData>(); // baseStats 리스트 초기화

            try
            {
                // 기본 정보 파싱
                data.characterID = int.Parse(fields[headerMap["Char_ID"]]);
                data.characterName = fields[headerMap["Char_Name"]];
                data.flavorText = fields[headerMap["Flavor_Txt"]];
                data.instruction = fields[headerMap["Instruction"]];

                // Sprite 로드 (Resources 폴더 내에 스프라이트가 있어야 함)
                string spritePath = fields[headerMap["Char_Sprite"]];
                if (!string.IsNullOrEmpty(spritePath))
                {
                    data.characterSprite = Resources.Load<Sprite>(spritePath);
                    if (data.characterSprite == null)
                    {
                        Debug.LogWarning($"캐릭터 스프라이트를 로드할 수 없습니다: {spritePath} (ID: {data.characterID})");
                    }
                }

                // 프리팹 경로로 프리팹을 로드하여 직접 연결
                string prefabPath = fields[headerMap["Prefab_Path"]];
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    data.characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (data.characterPrefab == null)
                    {
                        Debug.LogWarning($"캐릭터 프리팹을 로드할 수 없습니다: {prefabPath} (ID: {data.characterID})");
                    }
                }

                // Enum 파싱
                data.crewRole = (CrewRole)Enum.Parse(typeof(CrewRole), fields[headerMap["Crew_Role"]], true);
                data.faction = (Faction)Enum.Parse(typeof(Faction), fields[headerMap["Type"]], true);
                data.atkRangeType = (AtkRangeType)Enum.Parse(typeof(AtkRangeType), fields[headerMap["AtkRange_Type"]], true);

                // 스킬 정보 파싱
                data.skillPassiveID = data.characterID; // 같은 것도 있지만 다른게 많아서 그냥 각각 하는게 나을듯
                data.skillPassiveMotion = fields[headerMap["Skill_PassiveMotion"]];

                // --- 스탯 정보 파싱 (CSV 헤더와 Stat Enum 매핑) ---
                var statHeaderMapping = new Dictionary<string, Stat>
                {
                    { "Attack", Stat.Attack },
                    { "Health", Stat.Health },
                    { "Defence", Stat.Defense },
                    { "Critical_Prob", Stat.CritChance },
                    { "Critical_Dmg", Stat.CritDamage },
                    { "AtkSpeed", Stat.AttackSpeed }
                };

                foreach (var mapping in statHeaderMapping)
                {
                    string headerName = mapping.Key;
                    Stat statType = mapping.Value;

                    if (headerMap.ContainsKey(headerName))
                    {
                        string statValueStr = fields[headerMap[headerName]].Replace("%", "").Trim();
                        if (float.TryParse(statValueStr, out float statValue))
                        {
                            data.baseStats.Add(new StatData { statName = statType, value = statValue });
                        }
                        else
                        {
                            Debug.LogWarning($"스탯 값 파싱 실패: '{fields[headerMap[headerName]]}' (ID: {data.characterID}, Stat: {headerName})");
                        }
                    }
                }

                //// 스탯 정보 파싱 및 StatData 리스트에 추가
                //string[] statHeaders = new string[] { "Attack", "Health", "Defence", "Critical_Prob", "Critical_Dmg", "AtkSpeed" };
                //foreach (string statName in statHeaders)
                //{
                //    if (headerMap.ContainsKey(statName))
                //    {
                //        StatData stat = new StatData();
                //        stat.statName = (Stat)Enum.Parse(typeof(Stat), statName, true);
                //        stat.value = float.Parse(fields[headerMap[statName]]); // float으로 파싱
                //        data.baseStats.Add(stat);
                //    }
                //    else
                //    {
                //        Debug.LogWarning($"CSV에 스탯 헤더 '{statName}'가 없습니다. (줄 {i + 1})");
                //    }
                //}

                // SO 에셋을 저장할 경로를 지정합니다.
                string assetPath = $"{soSavePath}{data.characterID}.asset";

                // 기존 에셋이 있다면 삭제하고 새로 생성 (덮어쓰기)
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.CreateAsset(data, assetPath);
            }
            catch (Exception e)
            {
                // 에러 메시지를 더 상세하게 수정
                string idInfo = headerMap.ContainsKey("Char_ID") ? fields[headerMap["Char_ID"]] : "(ID 없음)";
                Debug.LogError($"CSV 파싱 중 오류 (줄 {i + 1}, ID: {idInfo}): {e.GetType()} - {e.Message}");
                continue;
            }
        }

        // 변경된 에셋을 저장하고 데이터베이스를 새로고침합니다.
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("캐릭터 데이터 임포트 완료!");
    }
    /// <summary>
    /// 큰따옴표로 묶인 필드 안의 쉼표를 무시하고 CSV 한 줄을 파싱하는 함수입니다.
    /// </summary>
    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString().Trim());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }
        fields.Add(currentField.ToString().Trim());
        return fields;
    }
}
