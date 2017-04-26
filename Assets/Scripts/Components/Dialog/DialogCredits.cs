using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogCredits : MonoBehaviour {
    private InputController inputCont;

    private void Update() {
        this.inputCont.VariableDurationShoot((a, b) => {
            Loader.shared.GetSingleton<DelegateCenter>().LoadMenuScene();
        });
    }

    private void Start() {
        this.inputCont = Loader.shared.GetSingleton<InputController>();
    }
}
