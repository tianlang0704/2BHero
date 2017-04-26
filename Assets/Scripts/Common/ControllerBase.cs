using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(LifeCycleDelegates))]
public class ControllerBase : MonoBehaviour {
    [HideInInspector] public bool isDelegatesInitialzed = false;

    protected virtual void Awake() {
    }

    protected virtual void Start() {
    }

    public virtual void InitializeDelegates() {
        this.isDelegatesInitialzed = true;
    }
}
