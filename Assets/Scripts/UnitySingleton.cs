using UnityEngine;

public abstract class UnitySingleton<T> : MonoBehaviour where T : UnitySingleton<T>
{
    private static T _instance = null;
    private static object _lock = new object();
    private static bool _isQuitApp = false;

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(T)) as T;
                    if (_instance == null)
                    {
                        _instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                        DontDestroyOnLoad(_instance.gameObject);
                    }
#if UNITY_EDITOR
                    else if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogWarning(typeof(T).ToString() + ": Already instance.");
                        return _instance;
                    }
#endif
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (_instance != this.GetComponent<T>())
            {
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (!_isQuitApp)
        {
            _instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _isQuitApp = true;
    }
}
