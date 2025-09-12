using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace JHT
{
    public class JHT_DataDownLoader
    {
        public const string itemRelicsURL = "https://docs.google.com/spreadsheets/d/1yrgwWQ_XijgUwjyUSkgDH9wji0oHuoYjEcmjZdZVGmg/export?format=csv"; //범위 설정 : &range=A2:B, 두번쨰시트 : &gid~~
        public const string lootTableURL = "https://docs.google.com/spreadsheets/d/13MgAe8F47N4GJRDn4vg4wGtn2Y31aOerQEQF096ox_g/export?format=csv";

        public event Action OnDataSetCompleted;
        public event Action OnMonsterDataSetCompleted;

        public IEnumerator DownloadData()
        {
            yield return null;
            yield return LoadDataCSV(itemRelicsURL, SetRelics,2);
            yield return LoadDataCSV(lootTableURL, SetLootTable, 2);
            OnDataSetCompleted?.Invoke();
        }

        public IEnumerator DownLoadMonsterData()
        {
            yield return null;
            //yield return LoadMonsterDataCSV();
            //yield return LoadMonsterTableCSV();
            OnMonsterDataSetCompleted?.Invoke();
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
                    so.upPower[rarity - 1] = float.Parse(row[6]);
                    so.startPower[rarity - 1] = float.Parse(row[8]);
                    so.cost[rarity - 1] = float.Parse(row[7]);
                    
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

    }
}
