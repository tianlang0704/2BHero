using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogMain : BComponentBase {
    public DialogOption optionMenuPrefab;

    public void HandleStartGame() {
        base.GetDep<SceneController>().LoadGameScene();
        base.GetDep<GameController>().GameStart();
    }

    public void HandleOption() {
        this.optionMenuPrefab.ClonePrefabAndShow(() => {
            this.gameObject.SetActive(true);
        });
        this.gameObject.SetActive(false);
    }

    public void HandleCredit() {
        base.GetDep<SceneController>().LoadCreditsScene();
    }

    public void HandleExit() {
        Application.Quit();
    }

// Mark: Initializeation
    protected override void Awake() {
        base.Awake();
        GameObject.FindObjectOfType<Loader>().DynamicInjection(this);
    }

    public void InjectDependencies(
        SceneController sc,
        GameController gc
    ) {
        base.ClearDependencies();
        base.AddDep<SceneController>(sc);
        base.AddDep<GameController>(gc);
        base.isInjected = true;
    }
// End: Initializeation

}
