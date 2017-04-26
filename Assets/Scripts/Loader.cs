using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Loader : ControllerBase {
    // TODO: Create a loadable base and put them all into a list
    // and remodel this class to facilitate more functions
    public List<ControllerBase> prefabs = new List<ControllerBase>();
    private Dictionary<Type, ControllerBase> singletons = new Dictionary<Type, ControllerBase>();

    public T GetSingleton<T>() where T : ControllerBase {
        if (!this.singletons.ContainsKey(typeof(T))) { throw new Exception("No such singleton: " + typeof(T)); }
        return (T)this.singletons[typeof(T)];
    }

    public void LoadSingletons(List<ControllerBase> prefabs) {
        prefabs.ForEach((prefab) => {
            if (!this.singletons.ContainsKey(prefab.GetType())) {
                ControllerBase cb = Instantiate(prefab);
                this.singletons[prefab.GetType()] = cb;
                DontDestroyOnLoad(cb);
            }
        });
    }

    public void InjectDependencies() {
        foreach(KeyValuePair<Type, ControllerBase> kvp in this.singletons) {
            if (kvp.Value.isDelegatesInitialzed) { return; }
            MethodInfo mi = kvp.Value.GetType().GetMethod("InitializeDelegates");
            if (mi == null) { Debug.Log("Type does not have InitializeDelegates method: " + kvp.Value); return; }
            mi.Invoke(kvp.Value, null);
        }
    }

    public static Loader shared;
    void Init() {
        if (Loader.shared == null) {
            Loader.shared = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(this);
        }
    }

    protected override void Awake() {
        base.Awake();
        Init();
        Loader.shared.LoadSingletons(this.prefabs);
        Loader.shared.InjectDependencies();
    }
}
