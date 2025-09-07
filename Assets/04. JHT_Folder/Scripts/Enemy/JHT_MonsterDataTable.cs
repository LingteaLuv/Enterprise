using JHT;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterTable", menuName = "MonsterTable/Dungeon")]
public class JHT_MonsterDataTable : ScriptableObject
{
    //추후에 데이터 테이블은 string형식으로 구성되며 data가 저장된 곳에서 로드해야됨
    public List<JHT_BaseMonsterFSM> monsterData;  
}
