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
        
        public event Action OnDataSetCompleted;

        public IEnumerator DownloadData()
        {
            yield return null;
            yield return LoadCSV(itemRelicsURL, SetRelics,2);
            OnDataSetCompleted?.Invoke();
        }

        private IEnumerator LoadCSV(string url, Action<string[][]> onParsed, int startLine = 1)
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
                    so.cost[rarity - 1] = float.Parse(row[7]);
                    //so.rarityImage[rarity-1] = Resources.Load<Sprite>(row[8]);
                }
            }

        }

    }
}
