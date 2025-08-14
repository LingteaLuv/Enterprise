using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object _lock = new object();
    private static bool _isShootingDown = false;
    
    [SerializeField]
    private bool _dontDestroyOnLoad = true;
    
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_isShootingDown)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();

                        var singletonComponent = _instance as Singleton<T>;
                        if (singletonComponent != null && singletonComponent._dontDestroyOnLoad)
                        {
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    protected void OnApplicationQuit()
    {
        _isShootingDown = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _isShootingDown = true;
        }
    }
}
