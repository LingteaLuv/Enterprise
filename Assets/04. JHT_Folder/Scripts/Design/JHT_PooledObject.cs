using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace JHT
{
    public class JHT_PooledObject : MonoBehaviour
    {
        private JHT_ObjectPool pool;
        public JHT_ObjectPool Pool
        {
            get => pool;
            set => pool = value;
        }

        // objPool => 돌아갈곳 , 지정해줘서 돌아갈곳 지정해주기
        public void PooledInit(JHT_ObjectPool objPool)
        {
            pool = objPool;
        }

        public void Release(float delay = 0f)
        {
            if (delay > 0)
                StartCoroutine(Wait(delay));
            else
                pool.ReturnToPool(this);
        }

        IEnumerator Wait(float delay)
        {
            yield return new WaitForSeconds(delay);
            pool.ReturnToPool(this);
        }
    }
}
