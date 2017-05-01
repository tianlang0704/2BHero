using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoInjectable {
    public string menuSceneName = "menu";
    public string gameSceneName = "game";
    public string creditsSceneName = "credits";

    public void LoadGameScene() {
        SceneManager.LoadScene(this.gameSceneName);
    }

    public void LoadMenuScene() {
        SceneManager.LoadScene(this.menuSceneName);
    }

    public void LoadCreditsScene() {
        SceneManager.LoadScene(this.creditsSceneName);
    }
}
