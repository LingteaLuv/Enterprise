using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace JHT
{
    public class JHT_ObjectPool
    {
        private JHT_PooledObject poolPrefab;
        private GameObject poolParent;

        public List<JHT_PooledObject> list;

        public JHT_ObjectPool(JHT_PooledObject _poolPrefab, int _poolSize, Transform _parent = null, bool isInGame = false)
        {
            SetupPool(_poolPrefab, _poolSize, _parent, isInGame);
        }

        private void SetupPool(JHT_PooledObject _poolPrefab, int _poolSize, Transform _parent,bool isInGame)
        {
            list = new List<JHT_PooledObject>(_poolSize);
            poolPrefab = _poolPrefab;

            if (!isInGame)
            {
                poolParent = _parent.gameObject;
            }
            else
            {
                poolParent = new GameObject($"{_poolPrefab.name} Pool");
                poolParent.transform.parent = _parent;
            }

            for (int i = 0; i < _poolSize; i++)
            {
                JHT_PooledObject instance = MonoBehaviour.Instantiate(poolPrefab, poolParent.transform);
                instance.gameObject.SetActive(false);
                instance.PooledInit(this);
                list.Add(instance);
            }
        }


        public JHT_PooledObject GetPooled()
        {
            if (list.Count == 0)
            {
                JHT_PooledObject newInst = MonoBehaviour.Instantiate(poolPrefab, poolParent.transform);
                newInst.gameObject.SetActive(true);
                newInst.PooledInit(this);
                return newInst;
            }

            int lastIndex = list.Count - 1;
            JHT_PooledObject nextInst = list[lastIndex];
            list.RemoveAt(lastIndex);
            nextInst.gameObject.SetActive(true);
            nextInst.PooledInit(this);
            return nextInst;
        }

        public void ReturnToPool(JHT_PooledObject pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
            pooledObject.PooledInit(this);
            //pooledObject.transform.parent = poolObject.transform;
            list.Add(pooledObject);
        }

        public void ReturAllPool(List<JHT_PooledObject> pools)
        {
            foreach (var p in pools)
            {
                p.gameObject.SetActive(false);
                p.PooledInit(this);
                list.Add(p);
            }
        }
    }
}
