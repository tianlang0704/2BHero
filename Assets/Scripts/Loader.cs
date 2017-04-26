using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LoaderSetting {
    public bool isSingleton = true;
    public BComponentBase component;
    LoaderSetting(BComponentBase component) {
        this.component = component;
    }
}

public class Loader : BComponentBase {
    public List<LoaderSetting> settings = new List<LoaderSetting>();
    // Components loaded and unloaded between scenes
    private Dictionary<Type, BComponentBase> componentList = new Dictionary<Type, BComponentBase>();
    // Components lives acroess different scenes
    private Dictionary<Type, BComponentBase> singletonList = new Dictionary<Type, BComponentBase>();
    // Injection queue for injections requested before components are initialized(Awake call)
    private List<BComponentBase> lateInjections = new List<BComponentBase>();

    private bool componentsInitialized = false;

    public void UnloadAllComponents() {
        foreach (KeyValuePair<Type, BComponentBase> kvp in this.componentList) {
            UnloadComponent(kvp.Key);
        }
    }

    public void UnloadComponent(Type t) {
        if (!this.componentList.ContainsKey(t)) { return; }
        Destroy(this.componentList[t]);
        this.componentList.Remove(t);
    }

    public T GetBComponent<T>() where T: BComponentBase {
        // Check non singleton first, if not check the other component list
        if (this.componentList.ContainsKey(typeof(T))) {
            return (T)this.componentList[typeof(T)];
        }

        return GetSingleton<T>();
    }

    public T GetSingleton<T>() where T: BComponentBase {
        if (this.singletonList.ContainsKey(typeof(T))) {
            return (T)this.singletonList[typeof(T)];
        }

        return null;
    }

    private void LoadComponents(List<LoaderSetting> settings) {
        settings.ForEach((LoaderSetting s) => {
            Type t = s.component.GetType();
            if (s.isSingleton) {
                // If this component is set to be singleton
                if (!this.singletonList.ContainsKey(t)) {
                    // And if this component wasn't load yet, load it and don't destroy it on load
                    BComponentBase newComp = Instantiate(s.component);
                    DontDestroyOnLoad(newComp);
                    this.singletonList[t] = newComp;
                }
            } else {
                // If this component is not set to be singleton
                if (this.componentList.ContainsKey(t)) {
                    // And if it aliready exists in the list, destroy it frist
                    UnloadComponent(t);
                }
                // Add it to component list
                this.componentList[t] = Instantiate(s.component);
            }
        });
    }

    public void DynamicInjection(BComponentBase obj) {
        // If components are not initialized (when called before loaders Awake function)
        // add it to the lateInjection list
        if (!this.componentsInitialized) {
            this.lateInjections.Add(obj);
            return;
        }

        //Otherwise inject directly
        InjectDependencies(obj);
    }

    private void InjectDependencies(BComponentBase obj) {
        if (obj.isInjected) { return; }
        // Find InjectDependencies and call it with what it needs
        // If it does not have this function, skip it
        MethodInfo mi = obj.GetType().GetMethod("InjectDependencies");
        if (mi == null) { Debug.Log("No InjectDependencies method found for " + obj.GetType()); return; }
        // 1. Get it's parameter list and make a list of what it needs
        ParameterInfo[] pis = mi.GetParameters();
        List<object> paraList = new List<object>();
        foreach (ParameterInfo pi in pis) {
            if (this.componentList.ContainsKey(pi.ParameterType)) {
                paraList.Add(this.componentList[pi.ParameterType]);
            } else if (this.singletonList.ContainsKey(pi.ParameterType)) {
                paraList.Add(this.singletonList[pi.ParameterType]);
            } else {
                throw new Exception("Can't find dependencies for " + obj.GetType().ToString());
            }
        }
        // 2. call InjectDependencies with the param list
        mi.Invoke(obj, paraList.ToArray());
    }

    private void InjectDependenciesForAll() {
        foreach(KeyValuePair<Type, BComponentBase> kvp in this.componentList) {
            if(kvp.Value.isInjected)
            InjectDependencies(kvp.Value);
        }
        foreach (KeyValuePair<Type, BComponentBase> kvp in this.singletonList) {
            InjectDependencies(kvp.Value);
        }
    }

    // Loader is the only singleton
    public static Loader shared = null;
    void InitSingleton() {
        if (Loader.shared == null) {
            Loader.shared = this;
            DontDestroyOnLoad(this);
        }else {

            Destroy(this);
        }
    }
    protected override void Awake() {
        base.Awake();

        // Load all preset components
        LoadComponents(this.settings);
        // Inject dependencies for all loaded components
        InjectDependenciesForAll();
        // Set complete flag
        this.componentsInitialized = true;
        
        // Inject if there's any late injections
        if(this.lateInjections.Count > 0) {
            this.lateInjections.ForEach((obj) => {
                InjectDependencies(obj);
            });
        }

        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            UnloadAllComponents();
        });
    }
}
