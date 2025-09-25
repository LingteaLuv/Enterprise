using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JHT
{
    public class JHT_DataDownLoader
    {
        public const string itemRelicsURL = "https://docs.google.com/spreadsheets/d/1yrgwWQ_XijgUwjyUSkgDH9wji0oHuoYjEcmjZdZVGmg/export?format=csv&gid=1891336971"; //범위 설정 : &range=A2:B, 두번쨰시트 : &gid~~
        public const string lootTableURL = "https://docs.google.com/spreadsheets/d/13MgAe8F47N4GJRDn4vg4wGtn2Y31aOerQEQF096ox_g/export?format=csv";
        public const string monsterDataURL = "https://docs.google.com/spreadsheets/d/1PmzSVnMU8XB2xUcDjxwD2VnZMGChZEwb0RiKSioXa8Q/export?format=csv&gid=1883053582";
        public const string monsterTableDataURL = "https://docs.google.com/spreadsheets/d/1W5YTINFy0XWnzm549uTDJL6Up4h81jeZhlc0A_9T_j8/export?format=csv&gid=1059773203";
        public const string skillDataURL = "https://docs.google.com/spreadsheets/d/1PmzSVnMU8XB2xUcDjxwD2VnZMGChZEwb0RiKSioXa8Q/export?format=csv&gid=1671130256";
       
        public event Action OnDataSetCompleted;
        public event Action OnMonsterDataTableSetCompleted;
        public event Action<List<MonsterSkillSO>> OnSkillDataSetCompleted;

        public bool isMonsterDataReady;
        public IEnumerator DownRelicsData()
        {
            yield return null;
            //데이터 두번 받아옴 한번으로 변경 필요해보임
            yield return LoadDataCSV(itemRelicsURL, SetRelics,2);
            OnDataSetCompleted?.Invoke();
        }

        public IEnumerator DownLootTableData()
        {
            yield return null;
            yield return LoadDataCSV(lootTableURL, SetLootTable, 2);
        }

        public IEnumerator DownLoadMonsterData()
        {
            yield return null;
            yield return LoadDataCSV(monsterDataURL, SetData, 4);
            yield return LoadDataCSV(monsterTableDataURL, SetMonsterTableData, 4);

            OnMonsterDataTableSetCompleted?.Invoke();
        }

        public IEnumerator DownLoadSkillTabledata()
        {
            yield return null;
            yield return LoadDataCSV(skillDataURL, SetSKillData, 2);

            OnSkillDataSetCompleted?.Invoke(MonsterDataManager.Instance.monsterSkillList);
        }

        private IEnumerator LoadDataCSV(string url, Action<string[][]> onParsed, int startLine = 1)
        {
            using UnityWebRequest www = UnityWebRequest.Get(url);

            yield return www.SendWebRequest();

            if (!string.IsNullOrEmpty(www.error))
                yield break;

            string raw = www.downloadHandler.text.Trim();
            string[] lines = raw.Split('\n');
            List<string[]> parsed = new();

            for (int i = startLine - 1; i < lines.Length; i++)
            {
                string[] row = lines[i].Trim().Split(',');
                parsed.Add(row);
            }

            onParsed?.Invoke(parsed.ToArray());
        }

        private void SetRelics(string[][] data)
        {
            ItemRelicsSO[] parse = new ItemRelicsSO[ItemDataManager.Instance.relicsList.Count];
            for (int i = 0; i < ItemDataManager.Instance.relicsList.Count; i++)
            {
                parse[i] = ItemDataManager.Instance.relicsList[i];
            }

            foreach (var row in data)
            {
                int RelicsID = int.Parse(row[0]);
                int rarity = int.Parse(row[4]);
                
                ItemRelicsSO so = Array.Find(parse, w => w.itemNum == RelicsID);
                if (so != null)
                {
                    so.itemName = row[1];
                    so.desc = row[2];
                    so.icon = Resources.Load<Sprite>(row[3]);
                    so.itemPowerType = (PowerType)Enum.Parse(typeof(PowerType), row[5]);
                    so.startPower[rarity - 1] = float.Parse(row[8]);
                    so.upPower[rarity - 1] = (float.Parse(row[6]) - so.startPower[rarity - 1])/100;
                    
                    //so.rarityImage[rarity-1] = Resources.Load<Sprite>(row[8]);
                }
            }

        }

        private void SetLootTable(string[][] data)
        {
            RelicsGachaLootTable[] parse = new RelicsGachaLootTable[ItemDataManager.Instance.lootTableList.Count];
            for (int i = 0; i < ItemDataManager.Instance.lootTableList.Count; i++)
            {
                parse[i] = ItemDataManager.Instance.lootTableList[i];
            }

            foreach (var row in data)
            {
                int LootID = int.Parse(row[0]);
                int rarity = int.Parse(row[1]);

                RelicsGachaLootTable so = Array.Find(parse, w => w.tableNum == LootID);
                if (so != null)
                {
                    so._items[rarity-1].weight = int.Parse(row[2]);
                }
            }
        }

        private void SetData(string[][] data)
        {
            JHT_MonsterDataSO[] parse = new JHT_MonsterDataSO[MonsterDataManager.Instance.monsterDataList.Count];
            for (int i = 0; i < MonsterDataManager.Instance.monsterDataList.Count; i++)
            {
                parse[i] = MonsterDataManager.Instance.monsterDataList[i];
            }

            foreach (var row in data)
            {
                int MonsterID = int.Parse(row[0]);
                
                JHT_MonsterDataSO so = Array.Find(parse, w => w.ID == MonsterID);
                if (so != null)
                {
                    so.monsterName = row[1]; 
                    so.monsterCrewRole = (CrewRole)Enum.Parse(typeof(CrewRole), row[2]);
                    so.monsterAttackType = (AtkRangeType)Enum.Parse(typeof(AtkRangeType), row[4]);
                    so.normalSkill = row[6];

                    for (int i = 0; i < so.monsterStat.Count; i++)
                    {
                        so.monsterStat[i].stat = (Stat)i;
                        so.monsterStat[i].amount = float.Parse(row[7+i]);
                    }
                    so.monsterStat[0].amount = float.Parse(row[17]);
                    so.skill1 = row[13];
                    so.chaseRange = float.Parse(row[16]);
                    so.moveSpeed = float.Parse(row[17]);
                    
                    
                    so.baseController = MonsterDataManager.Instance.animatorController;
                    //so.projectileSprite = Resources.Load<Sprite>($"{row[19]}");
                }
            }
        }

        private void SetMonsterTableData(string[][] data)
        {
            JHT_MonsterDataTable[] parse = new JHT_MonsterDataTable[MonsterDataManager.Instance.monsterTableList.Count];
            for (int i = 0; i < MonsterDataManager.Instance.monsterTableList.Count; i++)
            {
                parse[i] = MonsterDataManager.Instance.monsterTableList[i];
            }

            foreach (var row in data)
            {
                int TableID = int.Parse(row[0]);
                int readNum = int.Parse(row[1]);
                JHT_MonsterDataTable so = Array.Find(parse, w => w.ID == TableID);
                if (so != null)
                {
                    List<string> numArray = new List<string>(readNum * 3);

                    if (so.monsterData.Count != numArray.Count)
                    {

                        for (int i = readNum; i > 0; i--)
                        {
                            if (readNum >= 10)
                            {
                                for (int j = 4; j >= 2; j--)
                                {
                                    numArray.Add(j.ToString() + "000" + i.ToString());
                                }
                            }
                            else
                            {
                                for (int j = 4; j >= 2; j--)
                                {
                                    numArray.Add(j.ToString() + "0000" + i.ToString());
                                }
                            }
                        }
                    }

                    if (so.monsterData.Count == numArray.Count)
                    {
                        for (int i = 0; i < numArray.Count; i++)
                        {
                            JHT_MonsterDataSO a = so.monsterData.Find(w => w.ID == MonsterDataManager.Instance.monsterDataDic[numArray[i]].ID);
                            if (numArray[i] != a.ID.ToString())
                                so.monsterData.Add(MonsterDataManager.Instance.monsterDataDic[numArray[i]]);
                        }
                    }

                    so.addStat = float.Parse(row[2]);
                    //so.roundCount = int.Parse(row[3]);
                }
            }
        }

        public void SetSKillData(string[][] data)
        {
            MonsterSkillSO[] parse = new MonsterSkillSO[MonsterDataManager.Instance.monsterSkillList.Count];
            for (int i = 0; i < MonsterDataManager.Instance.monsterSkillList.Count; i++)
            {
                parse[i] = MonsterDataManager.Instance.monsterSkillList[i];
            }

            foreach (var row in data)
            {
                int skillID = int.Parse(row[0]);

                MonsterSkillSO so = Array.Find(parse, w => w.ID == skillID);
                if (so != null)
                {
                    so.skillName = row[1];
                    so.monsterSkillAttackType = (MonsterSkillAttackType)Enum.Parse(typeof(MonsterSkillAttackType), row[1]);
                    so.coolTime = float.Parse(row[5]);
                }
            }
        }

    }
}
