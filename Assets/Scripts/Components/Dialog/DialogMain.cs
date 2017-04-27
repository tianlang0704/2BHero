using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogMain : MonoInjectable {
    public DialogOption optionMenuPrefab;

    [Inject]
    protected GameController gameController;
    [Inject]
    protected SceneController sceneController;

    public void HandleStartGame() {
        this.sceneController.LoadGameScene();
        this.gameController.GameStart();
    }

    public void HandleOption() {
        this.optionMenuPrefab.ClonePrefabAndShow(() => {
            this.gameObject.SetActive(true);
        });
        this.gameObject.SetActive(false);
    }

    public void HandleCredit() {
        this.sceneController.LoadCreditsScene();
    }

    public void HandleExit() {
        Application.Quit();
    }
}
