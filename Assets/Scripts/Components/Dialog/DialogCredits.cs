using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogCredits : MonoBehaviour {
    private void Update() {
        InputController.shared.VariableDurationShoot((a, b) => {
            DelegateCenter.shared.LoadMenuScene();
        });
    }
}
