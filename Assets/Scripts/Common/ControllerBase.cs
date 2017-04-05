using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LifeCycleDelegates))]
public class ControllerBase : MonoBehaviour {
    protected LifeCycleDelegates lifeCycle;

    virtual protected void Awake() {
        this.lifeCycle = this.GetComponent<LifeCycleDelegates>();
    }

    virtual protected void Start() {
        InitializeDelegates();
    }

    virtual protected void InitializeDelegates() {

    }
}
