using JHT;
using UnityEngine;

[CreateAssetMenu(fileName = "EncyclopediaSO")]
public class EncyclopediaSO : ScriptableObject
{
    public int ID;
    public Rarity rarity;

    public string desc;
    public int goalCount;

    public RewardType1 reward1;
    public int rewardAmount1;

    public CurrencyType reward2;
    public int rewardAmount2;


    public Faction faction;

    public void GetReward1(RewardType1 type)
    {
        switch (type)
        {
            case RewardType1.Diamond:
                //해당 부분을 찾아서 연결
                break;
        }
    }

    public void GetReward2(CurrencyType type)
    {
        switch (type)
        {
            case CurrencyType.Gem:
                break;
            case CurrencyType.Gold:
                break;
            case CurrencyType.EnhancementStone:
                break;
            case CurrencyType.RelicsCoupon:
                InventoryManager.Instance.RelicsCoupon += rewardAmount2;
                break;
            case CurrencyType.RelicsPoint:
                InventoryManager.Instance.RelicsPoints += rewardAmount2;
                break;
        }
    }
}

public enum RewardType1
{
    Diamond
}
