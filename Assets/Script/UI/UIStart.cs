using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIStart : UIBase
{
    public Button TestButton;
    protected override void OnShow(params object[] Objects)
    {
        AudioManager.Instance.PlayBGM(Constants.bgm01);

        TestButton.onClick.AddListener(OnTestButton);

        Test();
    }

    // Update is called once per frame
    protected override void OnClose()
    {
        TestButton.onClick.RemoveAllListeners();
    }
    
    public void Test()
    {
    }

    private void OnTestButton()
    {
        AudioManager.Instance.PlaySFX(Constants.sfx01);

    }


}
