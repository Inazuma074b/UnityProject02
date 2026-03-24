using UnityEngine;

public class MonoBehaviorSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Very carefully: Don't call Instance when OnDestroy(), it will auto create singleton object automatically, replaced method by GetCurrentInstance()
    protected static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                //_instance = UnityEngine.Object.FindObjectOfType<T>() as T;
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("(singleton)" + typeof(T).Name.ToString());
                    _instance = singletonObject.AddComponent<T>();
                    DontDestroyOnLoad(singletonObject);
                }
                else
                {
                    DontDestroyOnLoad(_instance.gameObject);
                }
            }
            return _instance;
        }
    }
    virtual public void Awake()
    {
        if (null == _instance)
        {
            _instance = this as T;
            DontDestroyOnLoad(this);
        }
    }
    /*
    public static T GetCurrentInstance()
    {
        return _instance;
    }*/
}
