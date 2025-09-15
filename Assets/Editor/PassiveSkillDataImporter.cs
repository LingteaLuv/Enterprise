using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

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
            string[] fields = lines[i].Split(',');

            try
            {
                string effectID = GetString(fields, headerMap, "EffectID");
                if (string.IsNullOrEmpty(effectID)) continue;

                string effectType = GetString(fields, headerMap, "EffectType");
                string paramString1 = GetString(fields, headerMap, "Param_String1");
                string paramString2 = GetString(fields, headerMap, "Param_String1");
                float paramFloat1 = GetFloat(fields, headerMap, "Param_Float1");
                float paramFloat2 = GetFloat(fields, headerMap, "Param_Float2");

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
            string[] fields = lines[i].Split(',');

            try
            {
                string skillID = GetString(fields, headerMap, "skillID");
                if (string.IsNullOrEmpty(skillID)) continue;

                string assetPath = SKILL_SO_PATH + skillID + ".asset";
                SkillSO skillSO = AssetDatabase.LoadAssetAtPath<SkillSO>(assetPath) ?? CreateAsset<SkillSO>(assetPath);

                skillSO.skillID = int.Parse(skillID);
                skillSO.name = skillID;
                skillSO.skillName = GetString(fields, headerMap, "skillName");
                skillSO.cooldown = GetFloat(fields, headerMap, "cooldown");
                skillSO.skillTargetType = GetEnum<ESkillTargetType>(GetString(fields, headerMap, "skillTargetType"));
                skillSO.targetLogic = GetEnum<ETargetLogic>(GetString(fields, headerMap, "targetLogic"));
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

    private static Dictionary<string, int> CreateHeaderMap(string headerLine)
    {
        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        string[] headers = headerLine.Split(',');
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