using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class MonsterObjectPool
{
    public GameObject[] pools;

    public MonsterObjectPool(GameObject target,int size , GameObject parent)
    {
        CreatePool(target,size,parent);
    }

    private void CreatePool(GameObject target, int size, GameObject parent)
    {
        pools = new GameObject[size];

        for (int i = 0; i < size; i++)
        {
            GameObject obj = MonoBehaviour.Instantiate(target,parent.transform);
            pools[i] = obj;
            pools[i].SetActive(false);
        }
    }

    public void GetPool(bool select)
    {
        foreach (var p in pools)
        {
            if (p.activeSelf != select)
            {
                p.SetActive(select);
                return;
            }
        }
    }

    public void DestroyAll()
    {
        foreach (var p in pools)
        {
            MonoBehaviour.Destroy(p);
        }
        pools = null;
    }

    //public GameObject prefabs;
    //Stack<GameObject> pools;
    //public int poolSize;
    //
    //private void Awake()
    //{
    //    pools = new(poolSize);
    //
    //    for (int i = 0; i < poolSize; i++)
    //    {
    //        GameObject obj = Instantiate(prefabs,transform);
    //        obj.SetActive(false);
    //    }
    //}
    //
    //public GameObject Get()
    //{
    //    GameObject obj = null;
    //    if (pools.Count <= 0)
    //    {
    //        obj = Instantiate(prefabs, transform);
    //        obj.SetActive(true);
    //    }
    //
    //    obj = pools.Pop();
    //    obj.SetActive(true);
    //
    //    return obj;
    //}
    //
    //public void Release(GameObject obj)
    //{
    //    pools.Push(obj);
    //    obj.SetActive(false);
    //}
}
