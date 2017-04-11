using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnimateParent : Shooter {
    protected override Animator animator {
        get {
            return this.transform.parent.GetComponent<Animator>();
        }
    }
}
