using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMain : MonoBehaviorSingleton<GameMain>
{


    // Start is called before the first frame update
    async void Start()
    {
        InitGame();
        //UIManager.Instance.NavigateTo(UIStart.UIName);
        //AudioManager.Instance.PlayBGM(Constants.bgm01);

        await UIManager.Instance.NavigateTo(Constants.UIStart);

    }

    private void InitGame()
    {
        DataManager.LoadGameSettings();
    }

    // Update is called once per frame
    public void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


}
