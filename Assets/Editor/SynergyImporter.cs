using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions; // 정규식을 위해 추가!
using UnityEditor;
using UnityEngine;

public class SynergyImporter
{
    private static string CSV_PATH = "/CSVData/Synergy.csv";
    private static string SKILL_SO_PATH = "Assets/Resources/SynergyData/SynergySkills/";
    private static string EFFECT_SO_PATH = "Assets/Resources/SkillData/PassiveEffects/";
    private static string SYNERGY_SO_PATH = "Assets/Resources/SynergyData/Synergy/";

    [MenuItem("Tools/Import Data/Synergy")]
    public static void ParseSynergyData()
    {
        if (!Directory.Exists(SKILL_SO_PATH)) Directory.CreateDirectory(SKILL_SO_PATH);
        if (!Directory.Exists(EFFECT_SO_PATH)) Directory.CreateDirectory(EFFECT_SO_PATH);
        if (!Directory.Exists(SYNERGY_SO_PATH)) Directory.CreateDirectory(SYNERGY_SO_PATH);

        ParseSkills();
        ParseSynergy();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("CSV 파싱 완료", "시너지 데이터 파싱을 완료했습니다.", "확인");
    }

    private static void ParseSkills()
    {
        string filePath = Application.dataPath + CSV_PATH;
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[SynergyImporter] csv 파일을 찾을 수 없습니다: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 3) return;

        var headerMap = CreateHeaderMap(lines[1]);

        for (int i = 3; i < lines.Length; i++)
        {
            // ✨ 변경점: 새로운 Split 함수 사용!
            string[] fields = SplitCsvLine(lines[i]);

            try
            {
                string skillID = GetString(fields, headerMap, "synergy_ID");
                if (string.IsNullOrEmpty(skillID)) continue;

                string assetPath = SKILL_SO_PATH + skillID + ".asset";
                SkillSO skillSO = AssetDatabase.LoadAssetAtPath<SkillSO>(assetPath) ?? CreateAsset<SkillSO>(assetPath);

                // --- 유효성 검사를 위해 Enum 값을 먼저 변수에 저장 ---
                ESkillTargetType type = GetEnum<ESkillTargetType>(GetString(fields, headerMap, "skillTargetType"));
                ETargetLogic logic = GetEnum<ETargetLogic>(GetString(fields, headerMap, "targetLogic"));

                // ✨ 변경점: 유효성 검사 로직 호출!
                if (!IsCombinationValid(type, logic))
                {
                    Debug.LogError($"[CSV 파싱 유효성 오류] 스킬 ID: {skillID} - TargetType '{type}'과 TargetLogic '{logic}'은 잘못된 조합입니다. CSV 파일을 확인해주세요!");
                    continue; // 잘못된 데이터는 건너뛰기
                }

                // --- 검사를 통과한 데이터만 최종 할당 ---
                skillSO.skillID = int.Parse(skillID);
                skillSO.name = skillID;
                skillSO.skillName = GetString(fields, headerMap, "synergy_name");
                skillSO.cooldown = 999f;
                skillSO.skillTargetType = type;
                skillSO.targetLogic = logic;
                skillSO.targetRole = GetEnum<CrewRole>(GetString(fields, headerMap, "targetRole"));

                skillSO.effects.Clear();
                string effectIDsStr = GetString(fields, headerMap, "EffectIDs");
                if (!string.IsNullOrEmpty(effectIDsStr))
                {
                    string[] effectIDs = effectIDsStr.Split(';');
                    foreach (string effectID in effectIDs)
                    {
                        if (string.IsNullOrEmpty(effectID)) continue;
                        string effectAssetPath = EFFECT_SO_PATH + effectID.Trim() + ".asset";
                        SkillEffectSO effectSO = AssetDatabase.LoadAssetAtPath<SkillEffectSO>(effectAssetPath);
                        if (effectSO != null)
                        {
                            skillSO.effects.Add(effectSO);
                        }
                        else
                        {
                            Debug.LogWarning($"[PassiveSkillParser] Skill '{skillID}'에 연결된 Effect '{effectID}' 에셋을 찾을 수 없습니다.");
                        }
                    }
                }
                EditorUtility.SetDirty(skillSO);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PassiveSkillParser] Skills.csv 파싱 오류 (줄 {i + 1}): {ex.Message}");
            }
        }
        Debug.Log("[PassiveSkillParser] Skills.csv 파싱 완료.");
    }

    private static void ParseSynergy()
    {
        string filePath = Application.dataPath + CSV_PATH;
        if (!File.Exists(filePath)) return;

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 3) return;

        var headerMap = CreateHeaderMap(lines[1]);

        for (int i = 3; i < lines.Length; i++)
        {
            string[] fields = SplitCsvLine(lines[i]);

            try
            {
                string synergyID = GetString(fields, headerMap, "synergy_ID");
                if (string.IsNullOrEmpty(synergyID)) continue;

                // --- 1. ParseSkills에서 생성된 기존 SkillSO를 찾아 로드 --- //
                // ParseSkills가 synergy_ID를 skillID로 사용한다고 가정합니다.
                string skillAssetPath = SKILL_SO_PATH + synergyID + ".asset";
                SkillSO skillSO = AssetDatabase.LoadAssetAtPath<SkillSO>(skillAssetPath);

                if (skillSO == null)
                {
                    Debug.LogWarning($"[SynergyImporter] Synergy '{synergyID}'에 연결된 SkillSO 에셋을 찾을 수 없습니다. ParseSkills가 먼저 실행되었는지, CSV에 skillID가 올바른지 확인해주세요.");
                    continue; // 연결할 스킬이 없으면 이 시너지는 건너뜁니다.
                }

                // --- 2. 시너지 SO 생성 및 데이터 채우기 ---
                string synergyAssetPath = SYNERGY_SO_PATH + "Synergy_" + synergyID + ".asset";
                SynergySO synergySO = AssetDatabase.LoadAssetAtPath<SynergySO>(synergyAssetPath) ?? CreateAsset<SynergySO>(synergyAssetPath);

                synergySO.synergyName = GetString(fields, headerMap, "synergy_name");

                // 필요한 캐릭터 ID 목록 파싱 (crewID1 ~ crewID5)
                synergySO.requiredCharacterIDs = new List<int>();
                for (int j = 1; j <= 5; j++)
                {
                    string crewIDStr = GetString(fields, headerMap, "crewID" + j);
                    if (int.TryParse(crewIDStr, out int crewId))
                    {
                        synergySO.requiredCharacterIDs.Add(crewId);
                    }
                }

                // 위에서 찾아온 SkillSO를 연결
                synergySO.buffToApply = skillSO;

                EditorUtility.SetDirty(synergySO);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SynergyImporter] Synergy.csv 파싱 오류 (줄 {i + 1}): {ex.Message}");
            }
        }
        Debug.Log("[SynergyImporter] Synergy.csv 파싱 완료.");
    }

    // ✨ 새로운 기능: 따옴표 안의 쉼표를 무시하는 CSV 라인 분리기
    private static string[] SplitCsvLine(string line)
    {
        // 따옴표 안 쉼표는 무시, 따옴표 밖 쉼표에서만 Split
        // 정규식 설명:
        // ,(?=(?:[^"]*"[^"]*")*[^"]*$)
        //  └→ 따옴표의 짝수 개수 뒤의 쉼표만 매칭
        var pattern = @",(?=(?:[^""]*""[^""]*"")*[^""]*$)";

        return Regex.Split(line, pattern)
                    .Select(s => s.Trim().Trim('"'))
                    .ToArray();
    }

    // ✨ 새로운 기능: TargetType과 TargetLogic 조합 유효성 검사기
    private static bool IsCombinationValid(ESkillTargetType type, ETargetLogic logic)
    {
        if (type == ESkillTargetType.Supportive)
        {
            return logic == ETargetLogic.Self ||
                   logic == ETargetLogic.AllAllies ||
                   logic == ETargetLogic.SingleAlly_ByRole ||
                   logic == ETargetLogic.SingleLowestAlly_ByRole ||
                   logic == ETargetLogic.AllAllies_ByRole;
        }
        else // Offensive
        {
            return logic == ETargetLogic.PrimaryTarget ||
                   logic == ETargetLogic.ClosestEnemy ||
                   logic == ETargetLogic.LowestHealthEnemy ||
                   logic == ETargetLogic.AllEnemiesInRadius;
        }
    }

    private static Dictionary<string, int> CreateHeaderMap(string headerLine)
    {
        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        // ✨ 변경점: 새로운 Split 함수 사용!
        string[] headers = SplitCsvLine(headerLine);
        for (int i = 0; i < headers.Length; i++)
        {
            string trimmedHeader = headers[i].Trim();
            if (!string.IsNullOrEmpty(trimmedHeader) && !headerMap.ContainsKey(trimmedHeader))
            {
                headerMap[trimmedHeader] = i;
            }
        }
        return headerMap;
    }

    // --- 아래의 헬퍼 함수들은 기존과 동일 ---
    private static string GetString(string[] fields, Dictionary<string, int> map, string name)
    {
        return map.TryGetValue(name, out int i) && i < fields.Length ? fields[i].Trim() : "";
    }

    private static float GetFloat(string[] fields, Dictionary<string, int> map, string name, float defaultValue = 0f)
    {
        if (map.TryGetValue(name, out int i) && i < fields.Length && float.TryParse(fields[i], out float result))
        {
            return result;
        }
        return defaultValue;
    }

    private static T GetEnum<T>(string value, T defaultValue = default) where T : struct
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return Enum.TryParse(value, true, out T result) ? result : defaultValue;
    }

    private static T CreateAsset<T>(string path) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}
