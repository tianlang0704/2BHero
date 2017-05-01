using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SubEvent {
    OnDestroy, OnRecycle, OnFirstFixedUpdate, OnFirstUpdate, OnSceneLoaded
}

public enum UnsubEvent{
    OnDestroy, OnRecycle, OnFirstFixedUpdate, OnFirstUpdate, OnSceneLoaded
}

public class SubInfo {
    public Action sub;
    public Action<Scene, LoadSceneMode> sceneSub;
    public SubEvent subOn;
    public UnsubEvent unsubOn;

    public SubInfo() { }
    public SubInfo(Action sub, SubEvent subOn, UnsubEvent unsubOn) {
        this.sub = sub;
        this.subOn = subOn;
        this.unsubOn = unsubOn;
    }
    public SubInfo(Action<Scene, LoadSceneMode> sceneSub, SubEvent subOn, UnsubEvent unsubOn) {
        this.sceneSub = sceneSub;
        this.subOn = subOn;
        this.unsubOn = unsubOn;
    }
}


/// <summary>
/// This class has two main functionalities
///
/// 1. Life Cycle Exposion
/// expose some of the gameobject's life cycle events as delegates to
/// other components and objects so that they can react to the events
///
/// 2. Auto-Unsub
/// objects can specify unsub event when subscribing to the event on
/// this component so they don't have to check and unsub themselves.
/// And at the same time they don't have to have a seperate portion of the
/// code to deal with the unsubscription -- both sub and unsub will be in 
/// the same context and in the same block.
/// </summary>
public class LifeCycleDelegates : MonoBehaviour {
    //TODO: Rewrite using dicitonary
    public Action OnDestroyDelegate;
    public Action OnRecycleDelegate;
    public Action OnFirstFixedUpdateDelegate;
    public Action OnFirstUpdateDelegate;
    public Action<Scene, LoadSceneMode> OnSceneLoadDelegate;
    [HideInInspector] public bool isAfterFirstFixedUpdate = false;
    [HideInInspector] public bool isAfterFirstUpdate = false;

    private Poolable poolable = null;
    private List<SubInfo> removeTracker = new List<SubInfo>();

// Mark: Sub functions and convenient wrappers
    public SubInfo SubOnRecycle(Action sub) {
        return Sub(sub, SubEvent.OnRecycle, UnsubEvent.OnDestroy);
    }

    public SubInfo SubOnSceneLoaded(Action<Scene, LoadSceneMode> sub) {
        return Sub(sub, SubEvent.OnSceneLoaded, UnsubEvent.OnDestroy);
    }

    public SubInfo OnceOnDestroy(Action sub) {
        return Sub(sub, SubEvent.OnDestroy, UnsubEvent.OnDestroy);
    }

    public SubInfo OnceOnFirstFixedUpdate(Action sub) {
        return Sub(sub, SubEvent.OnFirstFixedUpdate, UnsubEvent.OnFirstFixedUpdate);
    }
     
    public SubInfo OnceOnFirstUpdate(Action sub) {
        return Sub(sub, SubEvent.OnFirstUpdate, UnsubEvent.OnFirstUpdate);
    }

    public SubInfo Sub(Action sub, SubEvent subOn, UnsubEvent unsubOn) {
        SubInfo newSubInfo = new SubInfo(sub, subOn, unsubOn);
        switch (subOn) {
            case SubEvent.OnRecycle:
                this.OnRecycleDelegate += sub;
                break;
            case SubEvent.OnDestroy:
                this.OnDestroyDelegate += sub;
                break;
            case SubEvent.OnFirstFixedUpdate:
                this.OnFirstFixedUpdateDelegate += sub;
                break;
            case SubEvent.OnFirstUpdate:
                this.OnFirstUpdateDelegate += sub;
                break;
            default: break;
        }
        this.removeTracker.Add(newSubInfo);
        return newSubInfo;
    }

    /// <summary>
    /// Special case for SceneLoaded
    /// </summary>
    /// <param name="sceneSub">Action to execute when subed event is reached</param>
    /// <param name="subOn">Sub event</param>
    /// <param name="unsubOn">Unsub event</param>
    /// <returns></returns>
    public SubInfo Sub(Action<Scene, LoadSceneMode> sceneSub, SubEvent subOn, UnsubEvent unsubOn) {
        SubInfo newSubInfo = new SubInfo(sceneSub, subOn, unsubOn);
        this.OnSceneLoadDelegate += sceneSub;
        this.removeTracker.Add(newSubInfo);
        return newSubInfo;
    }
// End: Sub functions and convenient wrappers

// Mark: Unsub functions
    public void Unsub(Action sub, UnsubEvent unsubOn) {
        int idx = this.removeTracker.FindIndex((SubInfo s) => { return (s.unsubOn == unsubOn && s.sub == sub); });
        if(idx == -1) { return; }
        SubInfo subinfo = this.removeTracker[idx];
        Unsub(subinfo);
    }

    public void Unsub(SubInfo subinfo) {
        UnsubAll(new List<SubInfo>() { subinfo });
    }

    private void UnsubAll(List<SubInfo> subinfos) {
        foreach (SubInfo s in subinfos) {
            switch (s.subOn) {
                case SubEvent.OnRecycle:
                    this.OnRecycleDelegate -= s.sub;
                    break;
                case SubEvent.OnDestroy:
                    this.OnDestroyDelegate -= s.sub;
                    break;
                case SubEvent.OnFirstFixedUpdate:
                    this.OnFirstFixedUpdateDelegate -= s.sub;
                    break;
                case SubEvent.OnFirstUpdate:
                    this.OnFirstUpdateDelegate -= s.sub;
                    break;
                case SubEvent.OnSceneLoaded:
                    this.OnSceneLoadDelegate -= s.sceneSub;
                    break;
                default: break;
            }
            this.removeTracker.Remove(s);
        }
    }
// End: Unsub functions

// Mark: Life Cycle Events
    private void OnRecycle() {
        if (this.OnRecycleDelegate != null) { this.OnRecycleDelegate(); }
        List<SubInfo> result = this.removeTracker.FindAll((SubInfo s) => { return s.unsubOn == UnsubEvent.OnRecycle; });
        UnsubAll(result);
    }

    private void OnDestroy() {
        if (this.OnDestroyDelegate != null) { this.OnDestroyDelegate(); }
        List<SubInfo> result = this.removeTracker.FindAll((SubInfo s) => { return s.unsubOn == UnsubEvent.OnDestroy; });
        UnsubAll(result);
    }

    private void OnFirstFixedUpdate() {
        if (this.OnFirstFixedUpdateDelegate != null) { this.OnFirstFixedUpdateDelegate(); }
        List<SubInfo> result = this.removeTracker.FindAll((SubInfo s) => { return s.unsubOn == UnsubEvent.OnFirstFixedUpdate; });
        UnsubAll(result);
    }

    private void OnFirstUpdate() {
        if (this.OnFirstUpdateDelegate != null) { this.OnFirstUpdateDelegate(); }
        List<SubInfo> result = this.removeTracker.FindAll((SubInfo s) => { return s.unsubOn == UnsubEvent.OnFirstUpdate; });
        UnsubAll(result);
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        if (this.OnSceneLoadDelegate != null) { this.OnSceneLoadDelegate(scene, mode); }
        List<SubInfo> result = this.removeTracker.FindAll((SubInfo s) => { return s.unsubOn == UnsubEvent.OnSceneLoaded; });
        UnsubAll(result);
    }
// End: Life Cycle Events

    private IEnumerator WaitForFixedUpateRoutine() {
        yield return new WaitForFixedUpdate();
        this.isAfterFirstFixedUpdate = true;
        OnFirstFixedUpdate();
    }

    private void Update() {
        if (!this.isAfterFirstUpdate) {
            this.isAfterFirstUpdate = true;
            OnFirstUpdate();
        }
    }

    private void Awake() {
        // Setup for OnRecycle event
        this.poolable = this.GetComponent<Poolable>();
        if(this.poolable){
            this.poolable.recycleDelegate += this.OnRecycle;
            this.OnceOnDestroy(() => { this.poolable.recycleDelegate -= this.OnRecycle; });
        }
        // Setup for OnFirstFixedUpdate event
        StartCoroutine(WaitForFixedUpateRoutine());
        // Setup for OnSceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoad;
        OnceOnDestroy(() => { SceneManager.sceneLoaded -= OnSceneLoad; });
    }
}
