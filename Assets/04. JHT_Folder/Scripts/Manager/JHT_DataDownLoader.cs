using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

namespace JHT
{
    public class JHT_DataDownLoader
    {
        public const string itemRelicsURL = "https://docs.google.com/spreadsheets/d/1yrgwWQ_XijgUwjyUSkgDH9wji0oHuoYjEcmjZdZVGmg/export?format=csv&gid=1891336971"; //범위 설정 : &range=A2:B, 두번쨰시트 : &gid~~
        public const string lootTableURL = "https://docs.google.com/spreadsheets/d/13MgAe8F47N4GJRDn4vg4wGtn2Y31aOerQEQF096ox_g/export?format=csv";
        public const string monsterDataURL = "https://docs.google.com/spreadsheets/d/1YVvWgG0PJYlX51LqkHn7emFqH58TzSgl7KFRFzwL1aU/export?format=csv&gid=1334569449";
        public const string monsterTableDataURL = "https://docs.google.com/spreadsheets/d/1W5YTINFy0XWnzm549uTDJL6Up4h81jeZhlc0A_9T_j8/export?format=csv&gid=1380546625";
        public const string skillDataURL = "https://docs.google.com/spreadsheets/d/1YVvWgG0PJYlX51LqkHn7emFqH58TzSgl7KFRFzwL1aU/export?format=csv";
        
        public event Action OnDataSetCompleted;
        public event Action OnMonsterDataTableSetCompleted;
        public event Action<List<MonsterSkillSO>> OnSkillDataSetCompleted;

        public bool isMonsterDataReady;
        public bool dataLoadFinish;
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
            dataLoadFinish = false;
            yield return LoadDataCSV(monsterDataURL, SetData, 4);
            yield return LoadDataCSV(monsterTableDataURL, rows => {
                try { SetMonsterTableData(rows); }
                catch (Exception e) { Debug.LogException(e); }
            }, 4);

            if(dataLoadFinish)
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
                    so.itemPowerType = (PowerType)Enum.Parse(typeof(PowerType), row[5]);
                    so.startPower[rarity - 1] = float.Parse(row[8]);
                    so.upPower[rarity - 1] = (float.Parse(row[6]) - so.startPower[rarity - 1])/100;

                    if (so.cost == null)
                    {
                        so.cost = new int[5];
                    }
                  
                    so.cost[rarity - 1] = int.Parse(row[7]);
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
                    so.normalSkill = int.Parse(row[5]);

                    for (int i = 0; i < so.monsterStat.Count; i++)
                    {
                        so.monsterStat[i].stat = (Stat)i;
                        so.monsterStat[i].amount = float.Parse(row[7+i]);
                    }
                    so.monsterStat[0].amount = float.Parse(row[17]);
                    so.normalSkill = int.Parse(row[5]);
                    so.skill1 = int.Parse(row[15]);
                    so.skill2 = -1;
                    //so.skill2
                    so.chaseRange = float.Parse(row[18]);
                    so.moveSpeed = float.Parse(row[19]);
                    
                    
                    so.baseController = MonsterDataManager.Instance.animatorController;
                    //so.projectileSprite = Resources.Load<Sprite>($"{row[19]}");
                }
            }
        }

        private void SetMonsterTableData(string[][] data)
        {
            SetMonsterTable(data).Forget();
        }

        private async UniTask SetMonsterTable(string[][] data)
        {
            JHT_MonsterDataTable[] parse = new JHT_MonsterDataTable[MonsterDataManager.Instance.monsterTableList.Count];
            for (int i = 0; i < MonsterDataManager.Instance.monsterTableList.Count; i++)
            {
                parse[i] = MonsterDataManager.Instance.monsterTableList[i];
            }

            foreach (var row in data)
            {
                int TableID = int.Parse(row[0]);
                int readNum1 = int.Parse(row[1]);
                int readNum2 = int.Parse(row[2]);
                int readNum3 = int.Parse(row[3]);
                int readNum4 = int.Parse(row[4]);
                JHT_MonsterDataTable so = Array.Find(parse, w => w.ID == TableID);
                if (so != null)
                {
                    List<string> numArray1 = new List<string>(readNum1);
                    List<string> numArray2 = new List<string>(readNum2);
                    List<string> numArray3 = new List<string>(readNum3);
                    List<string> numArray4 = new List<string>(readNum4);

                    numArray1 = await TableDataReader(2, readNum1);
                    numArray2 = await TableDataReader(3, readNum2);
                    numArray3 = await TableDataReader(4, readNum3);
                    numArray4 = await TableDataReader(1, readNum4);

                    if (so.monsterData.Count != (readNum1 + readNum2 + readNum3))
                    {
                        so.monsterData.Clear();
                        for (int i = 0; i < numArray1.Count; i++)
                        {
                            if (MonsterDataManager.Instance.monsterDataDic.TryGetValue(numArray1[i], out var soData1))
                                so.monsterData.Add(soData1); 
                            else
                                Debug.LogError($"Missing key: {numArray1[1]}");
                        }
                        for (int i = 0; i < numArray2.Count; i++)
                        {
                            if (MonsterDataManager.Instance.monsterDataDic.TryGetValue(numArray2[i], out var soData2))
                                so.monsterData.Add(soData2);
                            else
                                Debug.LogError($"Missing key: {numArray2[i]}");
                        }
                        for (int i = 0; i < numArray3.Count; i++)
                        {
                            if (MonsterDataManager.Instance.monsterDataDic.TryGetValue(numArray3[i], out var soData3))
                                so.monsterData.Add(soData3);
                            else
                                Debug.LogError($"Missing key: {numArray3[i]}");
                        }
                    }

                    if (so.captinMonsterData.Count != readNum4)
                    {
                        so.captinMonsterData.Clear();
                        for (int i = 0; i < numArray4.Count; i++)
                        {
                            if (!so.captinMonsterData.Contains(MonsterDataManager.Instance.monsterDataDic[numArray4[i]]))
                                so.captinMonsterData.Add(MonsterDataManager.Instance.monsterDataDic[numArray4[i]]);
                        }

                    }

                    so.roundCount = int.Parse(row[7]);
                    so.addStat = float.Parse(row[5]);
                    so.captinAddStat = float.Parse(row[6]);
                    //so.roundCount = int.Parse(row[3]);
                    dataLoadFinish = true;
                }
            }
        }

        private async UniTask<List<string>> TableDataReader(int num,int readNum)
        {
            List<string> list = new List<string>();
            try
            {
                for (int i = readNum; i > 0; i--)
                {
                    if (i >= 10)
                    {

                        list.Add(num.ToString() + "000" + i.ToString());

                    }
                    else
                    {

                        list.Add(num.ToString() + "0000" + i.ToString());

                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(new Exception("[MonsterTable] 비동기 설정 실패", e));
            }
            await UniTask.Yield();
            return list;
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
                    so.skillTargetType = (ESkillTargetType)Enum.Parse(typeof(ESkillTargetType), row[3]);
                    so.skillName = row[6];
                    so.targetLogic = (ETargetLogic)Enum.Parse(typeof(ETargetLogic), row[4]);
                    so.targetRole = (CrewRole)Enum.Parse(typeof(CrewRole), row[5]);
                    so.coolTime = float.Parse(row[7]);
                    so.clip = Resources.Load<AnimationClip>(row[13]);

                    if(so.effects.Count <= 0)
                        so.effects.Add(Resources.Load<SkillEffectSO>($"SkillData/PassiveEffects/{row[8]}"));
                }
            }
        }

    }
}
