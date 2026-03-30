using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

/// <summary>
/// 遊戲場景管理器
/// 負責場景的異步載入、卸載，以及常駐場景 (MainScene) 的維護。
/// </summary>
public class GameSceneManager : MonoBehaviorSingleton<GameSceneManager>
{
    // 常駐場景名稱，專門放置 UI 與各類管理器 (Managers)
    private const string MAIN_SCENE_NAME = "MainScene";

    // 當前已載入的遊戲關卡名稱
    private string _currentLevelName = string.Empty;

    // 是否正在載入場景中
    public bool IsLoading { get; private set; }

    /// <summary>
    /// 異步載入新的遊戲關卡
    /// </summary>
    /// <param name="levelName">目標關卡名稱</param>
    /// <param name="onProgress">載入進度回調 (0.0f - 1.0f)</param>
    /// <returns>Task</returns>
    public async Task LoadLevelAsync(string levelName, Action<float> onProgress = null)
    {
        if (IsLoading)
        {
            Debug.LogWarning($"[GameSceneManager] 正在載入場景 {levelName}，請稍候。");
            return;
        }

        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("[GameSceneManager] 場景名稱不可為空！");
            return;
        }

        IsLoading = true;

        try
        {
            // 1. 檢查並解除載入舊的遊戲關卡（但保持 MainScene 不動）
            if (!string.IsNullOrEmpty(_currentLevelName) && _currentLevelName != MAIN_SCENE_NAME)
            {
                // 確認場景是否已載入，若已載入則卸載
                Scene sceneToUnload = SceneManager.GetSceneByName(_currentLevelName);
                if (sceneToUnload.isLoaded)
                {
                    var unloadOp = SceneManager.UnloadSceneAsync(_currentLevelName);
                    while (!unloadOp.isDone)
                    {
                        await Task.Yield();
                    }
                }
            }

            // 2. 使用 Additive 模式異步載入新場景
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
            
            // 3. 實作載入進度回傳
            while (!loadOp.isDone)
            {
                // loadOp.progress 範圍是 0 - 0.9，1.0 代表完成
                float progress = Mathf.Clamp01(loadOp.progress / 0.9f);
                onProgress?.Invoke(progress);
                
                await Task.Yield();
            }

            // 確保進度達到 100%
            onProgress?.Invoke(1.0f);

            // 4. 自動將新關卡設為活躍場景 (Active Scene)
            // 這樣新生成的物件會自動歸入此場景中
            Scene newScene = SceneManager.GetSceneByName(levelName);
            if (newScene.IsValid())
            {
                SceneManager.SetActiveScene(newScene);
                _currentLevelName = levelName;
                Debug.Log($"[GameSceneManager] 成功切換至場景: {levelName}");
            }
            else
            {
                Debug.LogError($"[GameSceneManager] 無法取得場景: {levelName}，請確保場景已加入 Build Settings。");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 取得當前活躍場景名稱
    /// </summary>
    public string GetCurrentLevelName()
    {
        return _currentLevelName;
    }
}
