using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SkillDatabase
{
    private static Dictionary<int, SkillSO> _skillDictionary;
    private static bool _isInitialized = false;
    private const string SKILL_SO_PATH = "SkillData"; // 모든 SkillSO 에셋이 위치한 Resources 폴더 하위 경로

    /// <summary>
    /// 데이터베이스를 초기화하고 모든 SkillSO 에셋을 딕셔너리에 로드합니다.
    /// </summary>
    private static void Initialize()
    {
        if (_isInitialized) return;

        _skillDictionary = new Dictionary<int, SkillSO>();
        
        // Resources 폴더에서 모든 SkillSO 에셋을 불러옵니다.
        var allSkills = Resources.LoadAll<SkillSO>(SKILL_SO_PATH);

        foreach (var skill in allSkills)
        {
            if (!_skillDictionary.ContainsKey(skill.skillID))
            {
                _skillDictionary.Add(skill.skillID, skill);
            }
            else
            {
                Debug.LogWarning($"SkillDatabase에 중복된 스킬 ID가 존재합니다: {skill.skillID}");
            }
        }

        _isInitialized = true;
        Debug.Log($"[SkillDatabase] 초기화 완료. {allSkills.Length}개의 스킬을 로드했습니다.");
    }

    /// <summary>
    /// ID를 사용하여 스킬(SkillSO)을 가져옵니다.
    /// </summary>
    /// <param name="id">찾고자 하는 스킬의 고유 ID</param>
    /// <returns>해당 ID의 SkillSO. 찾지 못하면 null을 반환합니다.</returns>
    public static SkillSO GetSkillByID(int id)
    {
        // 데이터베이스가 초기화되지 않았다면 먼저 초기화합니다.
        if (!_isInitialized) Initialize();

        if (_skillDictionary.TryGetValue(id, out SkillSO skill))
        {
            return skill;
        }
        else
        {
            Debug.LogWarning($"ID({id})에 해당하는 스킬을 찾을 수 없습니다.");
            return null;
        }
    }
}
