using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogCredits : BComponentBase {
    private void Update() {
        InputController.shared.VariableDurationShoot((a, b) => {
            base.GetDep<SceneController>().LoadMenuScene();
        });
    }

    protected override void Awake() {
        base.Awake();
        GameObject.FindObjectOfType<Loader>().DynamicInjection(this);
    }

    public void InjectDependencies(
        SceneController sc
    ) {
        base.ClearDependencies();
        base.AddDep<SceneController>(sc);
        base.isInjected = true;
    }
}
