using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LifeCycleDelegates))]
public class DialogTutorial : PopupDialogBase {
    [Inject]
    protected InputController inputController;
    [Inject]
    protected SettingsController settingsController;
    [Inject]
    protected GameController gameController;

    protected override void Awake() {
        base.Awake();
        this.GetComponent<LifeCycleDelegates>().OnceOnFirstUpdate(() => {
            if (this.settingsController.isTutorialEnabled) {
                this.gameObject.SetActive(true);
                this.gameController.GamePause();
            }else {
                this.gameObject.SetActive(false);
            }
        });
    }

    private void Update() {
        this.inputController.VariableDurationShoot((a, b) => {
            this.gameObject.SetActive(false);
            this.settingsController.isTutorialEnabled = false;
            this.gameController.GameResume();
        });
    }
}
