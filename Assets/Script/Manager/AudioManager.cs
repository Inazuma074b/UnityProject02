using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AudioManager : MonoBehaviorSingleton<AudioManager>
{
    public AudioSource audioSource_bgm;
    public GameObject audioSource_sfx_root;
    //public AudioSource curPlayAudio { get; private set; }
    private List<AudioSource> audioSources_sfx_list;

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


    #region BGM
    public async void PlayBGM(string bgm)
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

    public void StopBGM()
    {
        if (audioSource_bgm.isPlaying) audioSource_bgm.Stop();
    }
    #endregion


    #region SFX
    public async void PlaySFX(string sfx_name, GameObject obj = null)
    {
        if (audioSources_sfx_list == null) audioSources_sfx_list = new List<AudioSource>();
        if (audioSource_sfx_root == null) audioSource_sfx_root = new("audioSource_sfx_root");

        string path = System.IO.Path.Combine(Constants.SFX_Path, sfx_name).Replace("\\", "/");
        AudioClip audioClip = await ResourceManager.Instance.LoadResourceAsync<AudioClip>(path);

        if (audioClip == null)
        {
            Debug.LogWarning("Audio files doesn't exist:" + path);
            return;
        }

        AudioSource audioSource;
        if (obj == null) audioSource = audioSource_sfx_root.AddComponent<AudioSource>();
        else audioSource = obj.AddComponent<AudioSource>();

        audioSources_sfx_list.Add(audioSource);
        audioSource.clip = audioClip;
        audioSource.volume = DataManager.GetSFXVolume();
        audioSource.loop = false;
        audioSource.Play(0);

        StartCoroutine(AudioPlayFinished(audioSource.clip.length, () => {
            StopSFX(audioSource);
        }));
    }

    public void StopSFX(AudioSource a)
    {
        if (a != null && a.isPlaying) a.Stop();
        audioSources_sfx_list.Remove(a);
        Destroy(a);
    }

    public void StopAllSFX()
    {
        while (audioSources_sfx_list.Count > 0)
            Instance.StopSFX(audioSources_sfx_list[0]);
    }

    #endregion
    
    /*
    public bool Play(string name, AudioType audioType, AudioSource audioSource, bool isLoop = false)
    {
        string path = audioType switch
        {
            AudioType.BGM => System.IO.Path.Combine(Constants.Music_Path, name).Replace("\\", "/"),
            AudioType.SFX => System.IO.Path.Combine(Constants.SFX_Path, name).Replace("\\", "/"),
            _ => ""
        };
        AudioClip audioClip = Resources.Load(path) as AudioClip;
        if (audioClip != null)
        {
            Play(audioSource, audioClip, audioType, isLoop);
            return true;
        }
        else
        {
            Debug.LogWarning("Audio files doesn't exist:" + path);
            return false;
        }
    }

    private void Play(AudioSource source, AudioClip clip, AudioType audioType, bool isLoop = false)
    {
        source.volume = audioType switch
        {
            AudioType.BGM => DataManager.GameSettings.BGMVolume,
            AudioType.SFX => DataManager.GameSettings.SFXVolume,
            _ => 1.0f,
        };
        source.clip = clip;
        source.loop = isLoop;
        source.Play(0);

    }

    */
   
    #region Misc
    public void ResetAudioSourceGameObject()
    {
        //BGM
        var find_audioSource_bgm = GameObject.Find(Constants.BgmAudioSource_Path);
        if (find_audioSource_bgm == null)
        {
            Type[] ty = new Type[] { typeof(AudioSource) };
            GameObject o = new("audioSource_bgm", ty);
            audioSource_bgm = o.GetComponent<AudioSource>();
        }
        else
        {
            audioSource_bgm = find_audioSource_bgm.GetComponent<AudioSource>();
            if (audioSource_bgm == null) audioSource_bgm = find_audioSource_bgm.AddComponent<AudioSource>();
        }
    }

    private IEnumerator AudioPlayFinished(float time, UnityAction callback)
    {
        yield return new WaitForSeconds(time);
        callback.Invoke();
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