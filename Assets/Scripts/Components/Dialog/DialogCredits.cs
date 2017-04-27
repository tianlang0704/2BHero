using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogCredits : MonoInjectable {
    [Inject]
    protected InputController inputController;
    [Inject]
    protected SceneController sceneController;

    private void Update() {
        this.inputController.VariableDurationShoot((a, b) => {
            this.sceneController.LoadMenuScene();
        });
    }
}
