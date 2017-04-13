using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnimateParent : Shooter {
    protected override Animator targetAnimator {
        get {
            return this.transform.parent.GetComponent<Animator>();
        }
    }
}
