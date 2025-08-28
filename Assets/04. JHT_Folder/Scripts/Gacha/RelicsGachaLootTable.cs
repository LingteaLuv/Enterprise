using JHT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RelicsGachaLootTable", menuName = "RelicsGachaLootTable/RlicsGachaTable")]
public class RelicsGachaLootTable : ScriptableObject
{
    [SerializeField] private List<RelicsGacha> _items;
    [System.NonSerialized] private bool isInitialized = false;

    private float _totalWeight;

    private void Initialize()
    {
        if (isInitialized)

            if (_items == null || _items.Count == 0)
            {
                _totalWeight = 0f;
                isInitialized = true;
                return;
            }
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].weight < 0f)
            {
                Debug.LogWarning($"[Initialize] 음수 weight: {_items[i].rarity.ToString()}");
                _items[i].weight = 0f;
            }
        }

        _totalWeight = _items.Sum(item => item.weight);
        isInitialized = true;
    }

    private void AltInitalize()
    {
        if (!isInitialized)
        {
            _totalWeight = 0;
            foreach (var item in _items)
            {
                _totalWeight += item.weight;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].weight < 0f)
                {
                    Debug.LogWarning($"[LootTable] 음수 weight: {_items[i].rarity.ToString()}");
                    _items[i].weight = 0f;
                }
            }

            isInitialized = true;
        }
    }

    public RelicsGacha GetRandomRange()
    {
        Initialize();

        float diceRoll = UnityEngine.Random.Range(0f, _totalWeight);

        foreach (var item in _items)
        {
            if (item.weight >= diceRoll)
            {
                return item;
            }

            diceRoll -= item.weight;
        }

        throw new System.Exception("GachaItem get failed");
    }
}

[System.Serializable]
public class RelicsGacha
{
    public ItemRarity rarity;
    public float weight;
}
