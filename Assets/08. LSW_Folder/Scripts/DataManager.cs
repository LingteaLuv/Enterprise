using System;
using System.Collections.Generic;
using JHT;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public List<ItemSO> AllRelics = new List<ItemSO>();
    public List<CharacterData> AllCharacters = new List<CharacterData>();
    public List<ItemSO> AllWeapons = new List<ItemSO>();

    public Action OnWeaponReady;
    public Action OnCrewReady;
}
