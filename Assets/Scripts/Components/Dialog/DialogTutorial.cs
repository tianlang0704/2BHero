using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LifeCycleDelegates))]
public class DialogTutorial : PopupDialogBase {
    private InputController inputCont;

    private void Awake() {
        this.GetComponent<LifeCycleDelegates>().OnceOnFirstdUpdate(() => {
            if (Loader.shared.GetSingleton<DelegateCenter>().GetEnableTutorial()) {
                this.gameObject.SetActive(true);
                Loader.shared.GetSingleton<DelegateCenter>().GamePause();
            }else {
                this.gameObject.SetActive(false);
            }
        });
    }

    private void Update() {
        this.inputCont.VariableDurationShoot((a, b) => {
            this.gameObject.SetActive(false);
            Loader.shared.GetSingleton<DelegateCenter>().SetEnableTutorial(false);
            Loader.shared.GetSingleton<DelegateCenter>().GameResume();
        });
    }

    private void Start() {
        this.inputCont = Loader.shared.GetSingleton<InputController>();
    }
}
