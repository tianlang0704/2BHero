using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogMain : MonoBehaviour {
    public DialogOption optionMenuPrefab;

    public void HandleStartGame() {
        Loader.shared.GetSingleton<DelegateCenter>().LoadGameScene();
        Loader.shared.GetSingleton<DelegateCenter>().GameStart();
    }

    public void HandleOption() {
        this.optionMenuPrefab.ClonePrefabAndShow(() => {
            this.gameObject.SetActive(true);
        });
        this.gameObject.SetActive(false);
    }

    public void HandleCredit() {
        Loader.shared.GetSingleton<DelegateCenter>().LoadCreditsScene();
    }

    public void HandleExit() {
        Application.Quit();
    }
}
