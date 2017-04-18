using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogMain : MonoBehaviour {
    public DialogOption optionMenuPrefab;

    public void HandleStartGame() {
        DelegateCenter.shared.LoadGameScene();
        DelegateCenter.shared.GameStart();
    }

    public void HandleOption() {
        this.optionMenuPrefab.ClonePrefabAndShow(() => {
            this.gameObject.SetActive(true);
        });
        this.gameObject.SetActive(false);
    }

    public void HandleCredit() {
        DelegateCenter.shared.LoadCreditsScene();
    }

    public void HandleExit() {
        Application.Quit();
    }
}
