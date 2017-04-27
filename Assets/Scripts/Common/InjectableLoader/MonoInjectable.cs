using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(LifeCycleDelegates))]
public class MonoInjectable : MonoBehaviour {
    [HideInInspector] public bool isDelegatesInitialzed = false;
    [HideInInspector] public bool isAutoInject = true;

    protected virtual void Awake() {
        if (this.isAutoInject) {
            Loader.SafeInject(this);
        }
    }

    protected virtual void Start() { }

    public virtual void InitializeDelegates() {
        this.isDelegatesInitialzed = true;
    }
}
