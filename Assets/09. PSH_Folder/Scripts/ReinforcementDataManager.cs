using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.IO;

public class ReinforcementDataManager : Singleton<ReinforcementDataManager>
{
    // Key: 단계(Stage), Value: [Key: 레벨, Value: 해당 레벨의 강화 정보]
    private Dictionary<int, Dictionary<int, ReinforcementData>> _dataByStage = new Dictionary<int, Dictionary<int, ReinforcementData>>();
    private Dictionary<int, int> _maxLevelPerStage = new Dictionary<int, int>();
    private Dictionary<int, ReinforcementData> _maxLevelDataPerStage = new Dictionary<int, ReinforcementData>();
    public bool IsInitialized { get; private set; } = false;

    private const string REINFORCEMENT_CSV_ADDRESS = "ShipReinforcementTable";

    protected override void Awake()
    {
        base.Awake();
        LoadAndParseDataAsync();
    }

    private async void LoadAndParseDataAsync()
    {
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(REINFORCEMENT_CSV_ADDRESS);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Parse(handle.Result.text);
        }
        else
        {
            Debug.LogError($"[ReinforcementDataManager] Failed to load CSV from Addressables: {REINFORCEMENT_CSV_ADDRESS}");
        }
        Addressables.Release(handle);
    }

    private void Parse(string csvText)
    {
        _dataByStage.Clear();
        _maxLevelPerStage.Clear();
        _maxLevelDataPerStage.Clear();

        using (StringReader reader = new StringReader(csvText))
        {
            reader.ReadLine(); reader.ReadLine(); reader.ReadLine(); // Skip headers

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(',');
                ReinforcementData data = new ReinforcementData
                {
                    Stage = int.Parse(values[0]),
                    Level = int.Parse(values[1]),
                    TotalAttackBonus = float.Parse(values[2]),
                    TotalDefenseBonus = float.Parse(values[3]),
                    TotalHpBonus = float.Parse(values[4]),
                    NextAttackUpgradeCost = int.Parse(values[5]),
                    NextDefenseUpgradeCost = int.Parse(values[6]),
                    NextHpUpgradeCost = int.Parse(values[7])
                };

                if (!_dataByStage.ContainsKey(data.Stage))
                {
                    _dataByStage[data.Stage] = new Dictionary<int, ReinforcementData>();
                    _maxLevelPerStage[data.Stage] = 0;
                }
                _dataByStage[data.Stage][data.Level] = data;

                if (data.Level >= _maxLevelPerStage[data.Stage])
                {
                    _maxLevelPerStage[data.Stage] = data.Level;
                    _maxLevelDataPerStage[data.Stage] = data;
                }
            }
        }
        IsInitialized = true;
        Debug.Log($"[ReinforcementDataManager] Parse complete. Loaded {_dataByStage.Count} stages.");
    }

    public ReinforcementData GetDataForLevel(int stage, int level)
    {
        if (IsInitialized && _dataByStage.TryGetValue(stage, out var stageData) && stageData.TryGetValue(level, out var levelData))
        {
            return levelData;
        }
        return null;
    }

    public int GetMaxLevelForStage(int stage)
    {
        if (IsInitialized && _maxLevelPerStage.TryGetValue(stage, out int maxLevel))
        {
            return maxLevel;
        }
        return 0;
    }

    public ReinforcementData GetDataForMaxLevel(int stage)
    {
        if (IsInitialized && _maxLevelDataPerStage.TryGetValue(stage, out var data))
        {
            return data;
        }
        return null;
    }

    public bool DoesStageExist(int stage)
    {
        return IsInitialized && _dataByStage.ContainsKey(stage);
    }
}