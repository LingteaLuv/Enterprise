using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions; // 정규식을 위해 추가!
using UnityEditor;
using UnityEngine;

public class PassiveSkillDataImporter
{
    private static string CSV_PATH = "/CSVData/";
    private static string SKILL_SO_PATH = "Assets/Resources/SkillData/PassiveSkills/";
    private static string EFFECT_SO_PATH = "Assets/Resources/SkillData/PassiveEffects/";

    [MenuItem("Tools/Import Data/PassiveSkill")]
    public static void ParseGameData()
    {
        if (!Directory.Exists(SKILL_SO_PATH)) Directory.CreateDirectory(SKILL_SO_PATH);
        if (!Directory.Exists(EFFECT_SO_PATH)) Directory.CreateDirectory(EFFECT_SO_PATH);

        ParseEffects();
        ParseSkills();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("CSV 파싱 완료", "패시브 스킬 데이터 파싱을 완료했습니다.", "확인");
    }

    private static void ParseEffects()
    {
        string filePath = Application.dataPath + CSV_PATH + "Effects.csv";
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[PassiveSkillParser] Effects.csv 파일을 찾을 수 없습니다: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return;

        var headerMap = CreateHeaderMap(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            // ✨ 변경점: 새로운 Split 함수 사용!
            string[] fields = SplitCsvLine(lines[i]);

            try
            {
                string effectID = GetString(fields, headerMap, "EffectID");
                if (string.IsNullOrEmpty(effectID)) continue;

                string effectType = GetString(fields, headerMap, "EffectType");
                string paramString1 = GetString(fields, headerMap, "Param_String1");
                string paramString2 = GetString(fields, headerMap, "Param_String2");
                float paramFloat1 = GetFloat(fields, headerMap, "Param_Float1");
                float paramFloat2 = GetFloat(fields, headerMap, "Param_Float2");
                float paramFloat3 = GetFloat(fields, headerMap, "Param_Float3");

                string assetPath = EFFECT_SO_PATH + effectID + ".asset";

                switch (effectType)
                {
                    case "Buff":
                        BuffEffectSO buffEffect = AssetDatabase.LoadAssetAtPath<BuffEffectSO>(assetPath) ?? CreateAsset<BuffEffectSO>(assetPath);
                        buffEffect.statToBuff = GetEnum<Stat>(paramString1);
                        buffEffect.buffType = GetEnum<BuffType>(paramString2);
                        buffEffect.buffValue = paramFloat1;
                        buffEffect.duration = paramFloat2;
                        EditorUtility.SetDirty(buffEffect);
                        break;

                    case "Heal":
                        HealEffectSO healEffect = AssetDatabase.LoadAssetAtPath<HealEffectSO>(assetPath) ?? CreateAsset<HealEffectSO>(assetPath);
                        healEffect.healAmount = paramFloat1;
                        EditorUtility.SetDirty(healEffect);
                        break;
                    case "Damage":
                        DamageEffectSO damageEffect = AssetDatabase.LoadAssetAtPath<DamageEffectSO>(assetPath) ?? CreateAsset<DamageEffectSO>(assetPath);
                        damageEffect.powerRatio = paramFloat1;
                        damageEffect.hitCount = (int)paramFloat2;
                        damageEffect.delayBetweenHits = (int)paramFloat3;
                        EditorUtility.SetDirty(damageEffect);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PassiveSkillParser] Effects.csv 파싱 오류 (줄 {i + 1}): {ex.Message}");
            }
        }
        Debug.Log("[PassiveSkillParser] Effects.csv 파싱 완료.");
    }

    private static void ParseSkills()
    {
        string filePath = Application.dataPath + CSV_PATH + "Skills.csv";
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[PassiveSkillParser] Skills.csv 파일을 찾을 수 없습니다: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return;

        var headerMap = CreateHeaderMap(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            // ✨ 변경점: 새로운 Split 함수 사용!
            string[] fields = SplitCsvLine(lines[i]);

            try
            {
                string skillID = GetString(fields, headerMap, "skillID");
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
                skillSO.skillName = GetString(fields, headerMap, "skillName");
                skillSO.cooldown = GetFloat(fields, headerMap, "cooldown");
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
