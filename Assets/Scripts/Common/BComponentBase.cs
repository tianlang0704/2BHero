using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(LifeCycleDelegates))]
public abstract class BComponentBase : MonoBehaviour {
    public bool isInjected = false;
    public LifeCycleDelegates lifeCycle;
    protected Dictionary<Type, BComponentBase> dependencies = new Dictionary<Type, BComponentBase>();


// Mark:Dependency Management
    protected T GetDep<T>() where T : BComponentBase {
        if (!isInjected) { throw new Exception("Dependency not injected"); }
        if (!this.dependencies.ContainsKey(typeof(T))) { throw new Exception("No valid dependency"); }
        return (T)this.dependencies[typeof(T)];
    }

    protected void AddDep<T>(T value) where T : BComponentBase {
        if (this.dependencies.ContainsKey(typeof(T))) { return; }
        this.dependencies[typeof(T)] = value;
    }

    protected void ClearDependencies() {
        this.dependencies.Clear();
    }
// End:Dependency Management

    protected virtual void Awake() {
        this.lifeCycle = this.GetComponent<LifeCycleDelegates>();
    }

    virtual protected void Start() {
        InitializeDelegates();
    }

    virtual protected void InitializeDelegates() { }
}
