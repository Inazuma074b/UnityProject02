using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AudioManager : MonoBehaviorSingleton<AudioManager>
{
    public AudioSource audioSource_bgm;

    private List<AudioSource> audioSources_bgs_pool;
    private List<AudioSource> audioSources_sfx_pool;
    private List<AudioSource> audioSources_me_pool;

    #region MonoBehaviour
    private void OnEnable()
    {
        //EventManager.AddListener(GameEventType.Error_NoInternet, OnNoInternet);
        //EventManager.AddListener(GameEventType.DrivingStateChanged, OnDrivingStateChanged);
        Instance.ResetAudioSourceGameObject();
    }

    private void OnDisable()
    {
        //EventManager.RemoveListener(GameEventType.Error_NoInternet, OnNoInternet);
        //EventManager.RemoveListener(GameEventType.DrivingStateChanged, OnDrivingStateChanged);
    }
    #endregion

    public void Play(AudioType type, string name, bool isLoop = false, Transform obj = null, Action<AudioSource> callback = null)
    {
        switch (type) 
        {
            case AudioType.bgm:
                PlayBGM(name);
                break;
            case AudioType.bgs:
                PlayBGS(name, isLoop, obj, callback);
                break;
            case AudioType.me:
                PlayME(name, obj);
                break;
            case AudioType.sfx:
                PlaySFX(name, obj);
                break;
        }
    }

    public void StopAllByType(AudioType type)
    {
        switch (type)
        {
            case AudioType.bgm:
                StopBGM();
                break;
            case AudioType.bgs:
                StopAllBGS();
                break;
            case AudioType.me:
                StopAllME();
                break;
            case AudioType.sfx:
                StopAllSFX();
                break;
        }
    }


    #region BGM
    private async void PlayBGM(string bgm)
    {
        if (audioSource_bgm == null)
        {
            return;
        }
        else
        {
            if (audioSource_bgm.clip != null)
            {
                if (audioSource_bgm.clip.name.Equals(bgm)) return;
                else audioSource_bgm.Stop();
            }
        }

        string path = System.IO.Path.Combine(Constants.Music_Path, bgm).Replace("\\", "/");
        AudioClip audioClip = await ResourceManager.Instance.LoadResourceAsync<AudioClip>(path);
        if (audioClip == null)
        {
            Debug.LogWarning("Audio files doesn't exist:" + path);
            return;
        }
        audioSource_bgm.volume = DataManager.GetBGMVolume();
        audioSource_bgm.clip = audioClip;
        audioSource_bgm.loop = true;
        audioSource_bgm.Play(0);
    }

    private void StopBGM()
    {
        if (audioSource_bgm != null && audioSource_bgm.isPlaying) audioSource_bgm.Stop();
    }
    #endregion


    #region BGS
    private async void PlayBGS(string bgs_name, bool isLoop, Transform obj, Action<AudioSource> callback)
    {
        if (audioSources_bgs_pool == null) audioSources_bgs_pool = new List<AudioSource>();

        string path = System.IO.Path.Combine(Constants.BGS_Path, bgs_name).Replace("\\", "/");
        AudioClip audioClip = await ResourceManager.Instance.LoadResourceAsync<AudioClip>(path);

        if (audioClip == null)
        {
            Debug.LogWarning("Audio files doesn't exist:" + path);
            return;
        }

        AudioSource audioSource = GetAvailableAudioGameObject(AudioType.bgs);
        
        // 確保位置與層級正確
        if (obj != null)
        {
            audioSource.transform.SetParent(obj);
            audioSource.transform.localPosition = Vector3.zero; // 確保在目標物中心播放
            audioSource.spatialBlend = 1.0f; // 開啟 3D 音效感
        }
        else
        {
            audioSource.transform.SetParent(this.transform);
            audioSource.spatialBlend = 0.0f; // 回歸 2D 背景音感
        }

        audioSource.clip = audioClip;
        audioSource.volume = DataManager.GetBGSVolume();
        audioSource.loop = isLoop;
        audioSource.Play(0);

        if (!isLoop)
        {
            StartCoroutine(AudioPlayFinished(audioSource, () => {
                if (audioSource != null) StopAudio(audioSource);
            }));
        }

        callback(audioSource);
    }

    private void StopAllBGS()
    {
        if (audioSources_bgs_pool == null) return;
        foreach (AudioSource au in audioSources_bgs_pool)
            StopAudio(au);
    }
    #endregion


    #region ME
    private async void PlayME(string me_name, Transform obj = null)
    {
        if (audioSources_me_pool == null) audioSources_me_pool = new List<AudioSource>();

        string path = System.IO.Path.Combine(Constants.ME_Path, me_name).Replace("\\", "/");
        AudioClip audioClip = await ResourceManager.Instance.LoadResourceAsync<AudioClip>(path);

        if (audioClip == null)
        {
            Debug.LogWarning("Audio files doesn't exist:" + path);
            return;
        }

        AudioSource audioSource = GetAvailableAudioGameObject(AudioType.me);
        // 確保位置與層級正確
        if (obj != null)
        {
            audioSource.transform.SetParent(obj);
            audioSource.transform.localPosition = Vector3.zero; // 確保在目標物中心播放
            audioSource.spatialBlend = 1.0f; // 開啟 3D 音效感
        }
        else
        {
            audioSource.transform.SetParent(this.transform);
            audioSource.spatialBlend = 0.0f; // 回歸 2D 背景音感
        }

        audioSource.clip = audioClip;
        audioSource.volume = DataManager.GetMEVolume();
        audioSource.loop = false;
        audioSource.Play(0);

        StartCoroutine(AudioPlayFinished(audioSource, () => {
            if (audioSource != null) StopAudio(audioSource);
        }));
    }

    private void StopAllME()
    {
        if (audioSources_me_pool == null) return;
        foreach (AudioSource au in audioSources_me_pool)
            StopAudio(au);
    }
    #endregion


    #region SFX
    private async void PlaySFX(string sfx_name, Transform obj = null)
    {
        if (audioSources_sfx_pool == null) audioSources_sfx_pool = new List<AudioSource>();

        string path = System.IO.Path.Combine(Constants.SFX_Path, sfx_name).Replace("\\", "/");
        AudioClip audioClip = await ResourceManager.Instance.LoadResourceAsync<AudioClip>(path);

        if (audioClip == null)
        {
            Debug.LogWarning("Audio files doesn't exist:" + path);
            return;
        }

        AudioSource audioSource = GetAvailableAudioGameObject(AudioType.sfx);
        // 確保位置與層級正確
        if (obj != null)
        {
            audioSource.transform.SetParent(obj);
            audioSource.transform.localPosition = Vector3.zero; // 確保在目標物中心播放
            audioSource.spatialBlend = 1.0f; // 開啟 3D 音效感
        }
        else
        {
            audioSource.transform.SetParent(this.transform);
            audioSource.spatialBlend = 0.0f; // 回歸 2D 背景音感
        }

        audioSource.clip = audioClip;
        audioSource.volume = DataManager.GetSFXVolume();
        audioSource.loop = false;
        audioSource.Play(0);

        StartCoroutine(AudioPlayFinished(audioSource, () => {
            if (audioSource != null) StopAudio(audioSource);
        }));
    }

    private void StopAllSFX()
    {
        if (audioSources_sfx_pool == null) return;
        foreach (AudioSource au in audioSources_sfx_pool)
            StopAudio(au);
    }

    #endregion
   
    #region Misc
    private void ResetAudioSourceGameObject()
    {
        //BGM
        if (audioSource_bgm == null)
        {
            var find_audioSource_bgm = GameObject.Find(Constants.BgmAudioSource_Path);
            if (find_audioSource_bgm == null)
            {
                Type[] ty = new Type[] { typeof(AudioSource) };
                GameObject o = new(Constants.BgmAudioSource_Path, ty);
                audioSource_bgm = o.GetComponent<AudioSource>();
            }
            else
            {
                if(!find_audioSource_bgm.TryGetComponent<AudioSource>(out audioSource_bgm))
                    audioSource_bgm = find_audioSource_bgm.AddComponent<AudioSource>();
            }
        }
    }

    private AudioSource GetAvailableAudioGameObject(AudioType type)
    {
        // 根據 type 決定要跑哪一個 list
        List<AudioSource> targetList = type switch
        {
            AudioType.bgs => audioSources_bgs_pool,
            AudioType.sfx => audioSources_sfx_pool,
            AudioType.me => audioSources_me_pool,
            _ => null
        };
        if (targetList == null) return null;
        // 尋找閒置中的物件
        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            // 清理掉被 Destroy 的殘值
            if (targetList[i] == null)
            {
                targetList.RemoveAt(i);
                continue;
            }
            if (!targetList[i].isPlaying)
            {
                return targetList[i];
            }

        }

        // 找不到則新建一個 GameObject 並掛上 AudioSource
        GameObject go = new GameObject($"AudioSource_{type}_{targetList.Count}");
        go.transform.SetParent(this.transform);
        AudioSource newAu = go.AddComponent<AudioSource>();
        targetList.Add(newAu);
        return newAu;
    }

    private void StopAudio(AudioSource a)
    {
        if (a != null)
        {
            if (a.isPlaying) a.Stop();
            if (a.transform.parent != this.transform) a.transform.SetParent(this.transform);
        }
    }



    private IEnumerator AudioPlayFinished(AudioSource au, UnityAction callback)
    {
        // 先等一幀，確保 isPlaying 狀態已更新
        yield return null;
        // 等到播放結束
        while (au != null && au.isPlaying) yield return null;
        // 確保物件還在才執行回調
        if (au != null) callback?.Invoke();
    }


    #endregion






    #region
    public void test1()
    {

        Instance.PlaySFX(Constants.sfx01);
    }

    public void test2()
    {
        Instance.PlaySFX(Constants.sfx02);
    }
    #endregion



}

#region Enum
public enum AudioType
{
    bgm,
    bgs,
    sfx,
    me
}
#endregion