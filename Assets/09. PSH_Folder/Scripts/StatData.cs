using System.Numerics;
using UnityEngine;

[System.Serializable]
public class StatData
{
    public string statName = "new stat";
    [Min(1)]
    public int level = 1;

    public BigInteger baseStat = 10;
    public BigInteger statIncreasePerLevel = 5;

    public BigInteger baseCost = 100;
    public double costIncreaseRatio = 1.07;
}