using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class GameSceneManager : MonoBehaviorSingleton<GameSceneManager>
{
    public static string nowSence { get { return SceneManager.GetActiveScene().name; } }

    /// <summary>
    /// 立即切換場景
    /// </summary>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 非同步切換場景（可加入讀取畫面）
    /// </summary>
    public void LoadSceneAsync(string sceneName, Action callback)
    {
        StartCoroutine(LoadAsync(sceneName, callback));
    }

    private IEnumerator LoadAsync(string sceneName, Action callback)
    {
        // 開始非同步讀取
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // 在載入過程中可顯示 Loading UI
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log("Loading scene: " + (progress * 100) + "%");
            yield return null;
        }
        callback();
    }
}

public enum SceneName
{
    Sample,
    Main,
    Game
}
