using JHT;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RelicsGachaLootTable", menuName = "RelicsGachaLootTable/RlicsGachaTable")]
public class RelicsGachaLootTable : ScriptableObject
{
    [SerializeField] private List<RelicsGacha> _items;
    [System.NonSerialized] private bool isInitialized = false;
    [System.NonSerialized] private float _totalWeight;

    private void Initialize()
    {
        if (!isInitialized)
        {
            _totalWeight = _items.Sum(item => item.weight);
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
