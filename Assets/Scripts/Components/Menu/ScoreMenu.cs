using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreMenu : PopupMenuBase {
    public Text titleText;
    public Text scoreText;
    public Button playBtn;
    public Button menuBtn;

    public void ClonePrefabAndShow(string title, int score) {
        ScoreMenu s = Instantiate(this);
        s.titleText.text = title;
        s.scoreText.text = score.ToString();
        s.Show();
    }

    public void HandleMenu() {
        CloseMenu();
        DelegateCenter.shared.LoadMenuScene();
    }

    public void HandlePlay() {
        CloseMenu();
        DelegateCenter.shared.GameRestart();
    }
}
