using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LifeCycleDelegates))]
public class DialogTutorial : PopupDialogBase {

    private void Awake() {
        this.GetComponent<LifeCycleDelegates>().OnceOnFirstdUpdate(() => {
            if (DelegateCenter.shared.GetEnableTutorial()) {
                this.gameObject.SetActive(true);
                DelegateCenter.shared.GamePause();
            }else {
                this.gameObject.SetActive(false);
            }
        });
    }

    private void Update() {
        InputController.shared.VariableDurationShoot((a, b) => {
            this.gameObject.SetActive(false);
            DelegateCenter.shared.SetEnableTutorial(false);
            DelegateCenter.shared.GameResume();
        });
    }
}
