using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIStart : UIBase
{
    public Button BtnStart;
    protected override void OnShow(params object[] Objects)
    {
        BtnStart.onClick.AddListener(OnBtnStart);
    }

    // Update is called once per frame
    protected override void OnClose()
    {
        BtnStart.onClick.RemoveAllListeners();
    }
    
    private void OnBtnStart()
    {
        AudioManager.Instance.Play(AudioType.sfx, Constants.sfx01);
        LoadGameScene();
    }

    private async void LoadGameScene()
    {
        Debug.Log("Load GameScene.");
        await GameSceneManager.Instance.LoadLevelAsync(Constants.GameScene);
    }
}
