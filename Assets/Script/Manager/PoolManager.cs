using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// PoolManager: 物件池管理系統
/// 整合 ResourceManager，透過 Addressables Key 實現高效的物件複用。
/// </summary>
public class PoolManager : MonoBehaviorSingleton<PoolManager>
{
    // 物件池容器：Key 為 Addressables Key，Value 為閒置物件佇列
    private readonly Dictionary<string, Queue<GameObject>> _pools = new();
    
    // 物件根節點，保持 Hierarchy 乾淨
    private Transform _poolRoot;

    public override void Awake()
    {
        base.Awake();
        
        // 建立一個根物件來管理閒置物件
        _poolRoot = new GameObject("PoolRoot").transform;
        DontDestroyOnLoad(_poolRoot);
    }

    /// <summary>
    /// 從池中獲取物件 (異步)
    /// </summary>
    /// <param name="key">Addressables Key</param>
    /// <param name="parent">父節點</param>
    /// <returns>實例化的物件</returns>
    public async Task<GameObject> SpawnAsync(string key, Transform parent = null)
    {
        // 1. 檢查池中是否有閒置物件
        if (_pools.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            obj.transform.SetParent(parent);
            obj.SetActive(true);
            return obj;
        }

        // 2. 池中無物件，透過 ResourceManager 加載 Prefab
        GameObject prefab = await ResourceManager.Instance.LoadResourceAsync<GameObject>(key);

        if (prefab == null)
        {
            Debug.LogError($"[PoolManager] 無法生成物件，因為 Prefab 加載失敗: {key}");
            return null;
        }

        // 3. 實例化新物件
        GameObject newObj = Instantiate(prefab, parent);
        
        // 將 key 記錄在名稱中，以便回收 (Despawn) 時知道該放回哪個池子
        newObj.name = key; 
        
        return newObj;
    }

    /// <summary>
    /// 回收物件 (Despawn)
    /// 將物件隱藏並放回池中，而非毀壞
    /// </summary>
    /// <param name="obj">要回收的物件</param>
    public void Despawn(GameObject obj)
    {
        if (obj == null) return;

        // 從物件名稱獲取當初生成的 Key
        string key = obj.name; 

        obj.SetActive(false);
        obj.transform.SetParent(_poolRoot);

        // 如果該類別的池子尚未建立，則建立之
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new Queue<GameObject>();
        }

        _pools[key].Enqueue(obj);
    }

    /// <summary>
    /// 清除特定類別的物件池
    /// </summary>
    /// <param name="key">Addressables Key</param>
    public void ClearPool(string key)
    {
        if (_pools.TryGetValue(key, out var queue))
        {
            while (queue.Count > 0)
            {
                Destroy(queue.Dequeue());
            }
            _pools.Remove(key);
        }
    }

    /// <summary>
    /// 清除所有物件池
    /// </summary>
    public void ClearAll()
    {
        foreach (var key in _pools.Keys)
        {
            ClearPool(key);
        }
        _pools.Clear();
    }
}
