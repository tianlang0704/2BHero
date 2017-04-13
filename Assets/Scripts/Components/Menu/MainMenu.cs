using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {
    public OptionMenu optionMenuPrefab;

    public void HandleStartGame() {
        Debug.Log("GameStart pressed");
        SceneController.shared.LoadGameScene();
        GameController.shared.GameStart();
    }

    public void HandleOption() {
        Debug.Log("Option pressed");
        this.optionMenuPrefab.ClonePrefabAndShow(() => {
            this.gameObject.SetActive(true);
        });
        this.gameObject.SetActive(false);
    }

    public void HandleCredit() {
        Debug.Log("Credit pressed");
    }

    public void HandleExit() {
        Application.Quit();
    }
}
