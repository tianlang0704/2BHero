using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Loader loads MonoInjectable from settings, and facilitates injections to other
/// MonoInjectable by looking for the Inject attributes in their fields
/// </summary>
public class Loader : MonoInjectable {
    /// <summary>
    /// Injectables to preload
    /// </summary>
    public List<MonoInjectable> prefabs = new List<MonoInjectable>();
    /// <summary>
    /// Preloaded singletons
    /// </summary>
    private Dictionary<Type, MonoInjectable> singletons = new Dictionary<Type, MonoInjectable>();



    /// <summary>
    /// Return a loaded singleton
    /// </summary>
    /// <typeparam name="T">Singleton type</typeparam>
    /// <returns>A loaded singleton of specified type</returns>
    public T GetSingleton<T>() where T : MonoInjectable {
        if (!this.singletons.ContainsKey(typeof(T))) { throw new Exception("No such singleton: " + typeof(T)); }
        return (T)this.singletons[typeof(T)];
    }
    /// <summary>
    /// Load singletons from set prefabs
    /// </summary>
    /// <param name="prefabs">A list of prefabs to instantiate to singletons</param>
    public void LoadSingletons(List<MonoInjectable> prefabs) {
        prefabs.ForEach((prefab) => {
            // Instantiate every prefab and add to DontDestroyOnLoad if its type is not loaded
            if (!this.singletons.ContainsKey(prefab.GetType())) {
                MonoInjectable cb = Instantiate(prefab);
                this.singletons[prefab.GetType()] = cb;
                DontDestroyOnLoad(cb);
            }
        });
    }
    /// <summary>
    /// Inject dependencies for a injectable by reading its Inject attributes on its fields
    /// </summary>
    /// <param name="injectable">injectable to be injected on</param>
    public void InjectDependencies(MonoInjectable injectable) {
        // 1. Get all the fields with Inject attribute defined
        List<MemberInfo> mis = new List<MemberInfo>(
            injectable.GetType().GetMembers(
                BindingFlags.Public|
                BindingFlags.NonPublic|
                BindingFlags.Instance)
                .Where(prop => Attribute.IsDefined(prop, typeof(Inject))));
        // 2. Set field according to Inject settings
        mis.ForEach((member) => {
            // 2a. Check field
            if(member.MemberType != MemberTypes.Field) {
                Debug.Log("Loader can only inject field at this moment: " + injectable + " " + member);
                return;
            }
            FieldInfo attrField = (FieldInfo)member;
            Type injectType = attrField.FieldType;
            // 2b. Get Inject setting and set field according to setting
            foreach(Inject i in member.GetCustomAttributes(typeof(Inject), true)) { 
                if (i.isSingleton) {
                    // 2c. if set to be singleton, use preload singleton(TODO: Add lazy load)
                    if (!this.singletons.ContainsKey(injectType)) {
                        Debug.Log("No sub singleton: " + injectType + " for: " + injectable + " " + member);
                        return;
                    }
                    attrField.SetValue(injectable, this.singletons[injectType]);
                }else {
                    // 2d. if set to be not singleton, instantiate one
                    int injectIdx = this.prefabs.FindIndex((prefab) => { return prefab.GetType() == injectType; });
                    if (injectIdx == -1) {
                        Debug.Log("No class: " + injectType + " for: " + injectable + " " + member);
                        return;
                    }
                    MonoInjectable newInstance = Instantiate(this.prefabs[injectIdx]);
                    // 2e. and set it to don't destroy on load, and destroy it when injectable is destroyed
                    DontDestroyOnLoad(newInstance);
                    injectable.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
                        Destroy(newInstance);
                    });
                    attrField.SetValue(injectable, newInstance);
                }
            }
        });
    }



    /// <summary>
    /// Initialization flag for loader
    /// </summary>
    [HideInInspector]
    public bool isInitialized = false;
    /// <summary>
    /// Init loader as singleton
    /// </summary>
    protected void Init() {
        if (Loader.shared == null) {
            Loader.shared = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(this);
            base.isAutoInject = false;
        }
    }
    /// <summary>
    /// Inject dependencies for preloaded singletons
    /// </summary>
    protected void InjectDependencies() {
        // Load dependencies for all preloaded singletons
        foreach (KeyValuePair<Type, MonoInjectable> kvp in this.singletons) {
            // New dependency injection
            InjectDependencies(kvp.Value);

            // Old delegate initialization
            if (kvp.Value.isDelegatesInitialzed) { return; }
            MethodInfo mi = kvp.Value.GetType().GetMethod("InitializeDelegates");
            if (mi == null) { Debug.Log("Type does not have InitializeDelegates method: " + kvp.Value); return; }
            mi.Invoke(kvp.Value, null);
        }
    }
    /// <summary>
    /// In awake, loader initialize to be a singleton
    /// For later reloads, it loads more settings into the singleton,
    /// and then inject dependencies for them
    /// </summary>
    protected override void Awake() {
        // Putting Init before base.Awake because Init sets isAutoInject used by base.Awake
        Init();
        base.Awake();
        // 1. Load more settings into loader singleton even when its singleton is already loaded
        Loader.shared.LoadSingletons(this.prefabs);
        this.isInitialized = true;

        // 2. Inject for loaded singletons and injectables in lateInjects list
        Loader.shared.InjectDependencies();
        Loader.lateInjects.ForEach((injectable) => {
            Loader.shared.InjectDependencies(injectable);
        });
        Loader.lateInjects.Clear();
    }



    /// <summary>
    /// Static singleton variable for loader
    /// </summary>
    public static Loader shared = null;
    /// <summary>
    /// Static list for holding object being late injected after initialization
    /// </summary>
    private static List<MonoInjectable> lateInjects = new List<MonoInjectable>();
    /// <summary>
    /// Static injection method for loader to handle inject requests safely
    /// </summary>
    /// <param name="injectable">Object to get injected on </param>
    public static void SafeInject(MonoInjectable injectable) {
        // 1. Check if there's any loader is not initialized
        List<Loader> loaders = new List<Loader>(GameObject.FindObjectsOfType<Loader>());
        bool isAllLoadersInitialized = true;
        loaders.ForEach((loader) => { if (!loader.isInitialized) { isAllLoadersInitialized = false; } });

        // 2. If there's any loader not initialzed, add injectable to lateInject list to inject after init.
        if (loaders.Count == 0 || !isAllLoadersInitialized) {
            Loader.lateInjects.Add(injectable);
            return;
        }

        // 3. otherwise, just inject
        Loader.shared.InjectDependencies(injectable);
    }
}
