using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// ResourceManager: 核心資源管理系統
/// 整合 Addressables，支援泛型加載與自動快取 Handle。
/// </summary>
public class ResourceManager : MonoBehaviorSingleton<ResourceManager>
{
    // 儲存已加載資源的 Handle，避免重複加載消耗效能
    private readonly Dictionary<string, AsyncOperationHandle> _handleCache = new();

    /// <summary>
    /// 異步加載資源 (泛型)
    /// </summary>
    /// <typeparam name="T">資源類型 (如 GameObject, AudioClip, Sprite 等)</typeparam>
    /// <param name="key">Addressables Key</param>
    /// <returns>加載完成的資源</returns>
    public async Task<T> LoadResourceAsync<T>(string key) where T : UnityEngine.Object
    {
        // 檢查是否已有快取的 Handle
        if (_handleCache.TryGetValue(key, out var cachedHandle))
        {
            if (cachedHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return cachedHandle.Convert<T>().Result;
            }
            
            // 如果還在加載中，則等待該 Handle 完成
            await cachedHandle.Task;
            return cachedHandle.Convert<T>().Result;
        }

        // 開始新的異步加載
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        _handleCache[key] = handle; // 存入快取 (轉為非泛型 Handle)

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }

        // 加載失敗時移除快取並噴錯
        Debug.LogError($"[ResourceManager] 加載資源失敗! Key: {key}");
        _handleCache.Remove(key);
        return null;
    }

    /// <summary>
    /// 釋放不再使用的資源
    /// </summary>
    /// <param name="key">Addressables Key</param>
    public void ReleaseResource(string key)
    {
        if (_handleCache.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _handleCache.Remove(key);
            Debug.Log($"[ResourceManager] 資源已釋放: {key}");
        }
    }

    /// <summary>
    /// 清除所有快取的資源 (切換場景時使用)
    /// </summary>
    public void ClearAll()
    {
        foreach (var handle in _handleCache.Values)
        {
            Addressables.Release(handle);
        }
        _handleCache.Clear();
    }
}
