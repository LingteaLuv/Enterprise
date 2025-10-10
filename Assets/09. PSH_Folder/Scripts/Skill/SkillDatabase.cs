using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class SkillDatabase
{
    private static Dictionary<int, SkillSO> _skillDictionary;
    private static bool _isInitialized = false;
    private const string SKILL_SO_LABEL = "SkillData";

    public static bool IsInitialized => _isInitialized;

    /// <summary>
    /// 데이터베이스를 비동기적으로 초기화하고 모든 SkillSO 에셋을 딕셔너리에 로드합니다.
    /// </summary>
    public static async Task InitializeAsync()
    {
        if (_isInitialized) return;

        _skillDictionary = new Dictionary<int, SkillSO>();

        // 어드레서블을 사용하여 라벨이 지정된 모든 SkillSO 에셋을 비동기적으로 불러옵니다.
        AsyncOperationHandle<IList<SkillSO>> handle = Addressables.LoadAssetsAsync<SkillSO>(SKILL_SO_LABEL, null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<SkillSO> allSkills = handle.Result;
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
            Debug.Log($"[SkillDatabase] 초기화 완료! {allSkills.Count}개의 스킬을 로드");
        }
        else
        {
            Debug.LogError("[SkillDatabase] 스킬 데이터 로딩에 실패");
        }

        // 로드가 완료되면 핸들을 해제할 수 있어요.
        // Addressables.Release(handle); 
        // 주의: 만약 SkillSO 에셋들이 다른 곳에서 계속 사용된다면 핸들을 해제하면 안돼요!
        // 이 경우, 데이터베이스가 더 이상 필요 없을 때 (예: 게임 종료 시) 한 번에 해제하는 게 좋답니다.
    }

    /// <summary>
    /// ID를 사용해서 스킬(SkillSO)을 가져와요!
    /// </summary>
    /// <param name="id">찾고 싶은 스킬의 고유 ID</param>
    /// <returns>해당 ID의 SkillSO. 없으면 null을 반환해요.</returns>
    public static SkillSO GetSkillByID(int id)
    {
        // 데이터베이스가 초기화되지 않았다면 경고를 표시하고 null을 반환해요.
        // 게임 시작할 때 InitializeAsync()를 호출해서 미리 초기화를 끝내주세요!
        if (!_isInitialized)
        {
            Debug.LogError("[SkillDatabase] 데이터베이스가 아직 준비되지 않았어요! GetSkillByID를 부르기 전에 InitializeAsync()를 먼저 불러주세요!");
            return null;
        }

        if (_skillDictionary.TryGetValue(id, out SkillSO skill))
        {
            return skill;
        }
        else
        {
            Debug.LogWarning($"ID({id})에 해당하는 스킬을 찾을 수 없어요");
            return null;
        }
    }

    /// <summary>
    /// 데이터베이스를 언로드하고 리소스를 해제해요.
    /// </summary>
    public static void Unload()
    {
        if (!_isInitialized) return;

        // 여기서 핸들을 저장해두었다가 해제하거나, 라벨을 기반으로 에셋을 해제할 수 있어요.
        _skillDictionary.Clear();
        _isInitialized = false;
        Debug.Log("[SkillDatabase] 언로드 완료!");
    }
}
