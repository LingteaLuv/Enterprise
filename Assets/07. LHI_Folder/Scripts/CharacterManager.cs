using UnityEngine;

namespace LHI
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            Charactertest.OnClicked += test;
        }

        private void OnDisable()
        {
            Charactertest.OnClicked -= test;
        }

        void test()
        {

        }
    }
}