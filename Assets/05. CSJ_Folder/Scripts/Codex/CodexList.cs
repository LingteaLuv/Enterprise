using UnityEngine;

namespace _05._CSJ_Folder.Scripts.Codex
{
    [CreateAssetMenu(menuName = "Codex/List")]
    public class CodexList : ScriptableObject
    {
        [Header("도감 기준")] 
        [SerializeField] internal Faction faction;
        [SerializeField] internal CodexStd_Enum _codexStd;
        
        [Header("도감 클리어 간격")]
        [SerializeField] internal int _codexDistance;
        [Header("특별 퀘스트 나오는 간격")]
        [SerializeField] internal int _specialQuestDistance;
        [Header("마지막 도감 기준")]
        [SerializeField] internal int _lastCodexIndex;
        
        [Header("클리어시 증가하는 스탯")]
        [SerializeField] internal CodexStat _codexStat;
        [Header("클리어당 증가하는 수치")]
        [SerializeField] internal int _codexIncrease;
        
        [Header("일반 도감 정의")]
        [SerializeField] internal CodexRewardSO _generalCodexReward;
        [SerializeField] internal CodexRewardSO[] _specialCodexRewards;
        
        
    }
    public enum CodexStat
    {
        CritChance,
        CritDamage,
    }
}