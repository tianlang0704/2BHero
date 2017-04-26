using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LifeCycleDelegates))]
public class DialogTutorial : PopupDialogBase {

    protected override void Start() {
        this.GetComponent<LifeCycleDelegates>().OnceOnFirstdUpdate(() => {
            if (base.GetDep<SettingsController>().isTutorialEnabled) {
                this.gameObject.SetActive(true);
                base.GetDep<GameController>().GamePause();
            }else {
                this.gameObject.SetActive(false);
            }
        });
    }

    private void Update() {
        InputController.shared.VariableDurationShoot((a, b) => {
            this.gameObject.SetActive(false);
            base.GetDep<SettingsController>().isTutorialEnabled = false;
            base.GetDep<GameController>().GameResume();
        });
    }

    protected override void Awake() {
        base.Awake();
        GameObject.FindObjectOfType<Loader>().DynamicInjection(this);
    }

    public void InjectDependencies(
        SoundController sc,
        GameController gc,
        SettingsController settingsC
    ) {
        base.ClearDependencies();
        base.AddDep<SoundController>(sc);
        base.AddDep<GameController>(gc);
        base.AddDep<SettingsController>(settingsC);
        base.isInjected = true;
    }
}
