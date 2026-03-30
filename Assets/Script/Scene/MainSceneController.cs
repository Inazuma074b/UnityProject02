using UnityEngine;

public class MainSceneController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayBgm();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayBgm()
    {
        AudioManager.Instance.Play(AudioType.bgm, Constants.bgm01);
    }
}
