using JHT;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Linq;

namespace JHT
{
    public class ItemDataManager : Singleton<ItemDataManager>
    {
        public List<ItemWeaponSO> weaponList;
        public Dictionary<int, ItemWeaponSO> weaponDataDic;
        public EncyclopediaPanel encyclopediaPanel;
        public bool IsDataLoaded { get; private set; } = false;
        //public List<RelicsObject> relicsList;
        //public Dictionary<int, List<RelicsObject>> relicsDataDic;

        // 수정필요 : csv에서 데이터 받아온 후 초기화 할 수 있도록 설정 해야함
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            weaponList = new();
            weaponDataDic = new();

            ItemWeaponSO[] loadedWeapons = Resources.LoadAll<ItemWeaponSO>("EquipData");

            LoadWeaponList(loadedWeapons);
        }

        private void LoadWeaponList(ItemWeaponSO[] objs)
        {
            List<ItemWeaponSO> list = new();
            foreach (var w in objs)
            {
                list.Add(w);
            }

            //List<ItemWeaponSO> sortList = list.OrderBy(w => w.itemNum).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                weaponList.Add(list[i]);
            }

            LoadWeaponFinish(weaponList);
        }


        private void LoadWeaponFinish(List<ItemWeaponSO> list)
        {
            weaponDataDic.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                if (!weaponDataDic.ContainsKey(list[i].itemNum))
                    weaponDataDic.Add(list[i].itemNum, list[i]);
            }

            IsDataLoaded = true; // 데이터 로딩 완료!
            //if (encyclopediaPanel == null)
            //    encyclopediaPanel = FindObjectOfType<EncyclopediaPanel>();
            StartCoroutine(EndInit());
        }

        private IEnumerator EndInit()
        {
            yield return new WaitForEndOfFrame();
            encyclopediaPanel.Init();
        }

        public Dictionary<int, ItemWeaponSO> GetAllWeaponData()
        {
            return weaponDataDic;
        }
    }
}
