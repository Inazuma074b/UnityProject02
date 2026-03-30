using UnityEngine;

public class GameSceneController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayBgm();
        LoadUI();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public async void LoadUI()
    {
        await UIManager.Instance.NavigateTo(Constants.UIStart);
    }

    public void PlayBgm()
    {
        AudioManager.Instance.Play(AudioType.bgm, Constants.bgm02);
    }
}
