using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LifeCycleDelegates))]
public class Stickable : MonoBehaviour {
    public string scaleKeeperName = "ScaleKeeper";

    public Transform GetScaleKeeper() {
        Transform sk = this.transform.Find(this.scaleKeeperName);
        if (!sk) {
            sk = (new GameObject(this.scaleKeeperName)).transform;
        }
        return sk;
    }
}
