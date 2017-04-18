using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SubEvent {
    OnDestroy, OnRecycle, OnFirstFixedUpdate
}

public enum UnsubEvent{
    OnDestroy, OnRecycle, OnFirstFixedUpdate
}

public class SubInfo {
    public Action sub;
    public SubEvent subOn;
    public UnsubEvent unsubOn;

    public SubInfo() { }
    public SubInfo(Action sub, SubEvent subOn, UnsubEvent unsubOn) {
        this.sub = sub;
        this.subOn = subOn;
        this.unsubOn = unsubOn;
    }
}

// This class has two main functionalities
//
// 1. Life Cycle Exposion
// expose some of the gameobject's life cycle events as delegates to
// other components and objects so that they can react to the events
//
// 2. Auto-Unsub
// objects can specify unsub event when subscribing to the event on
// this component so they don't have to check and unsub themselves.
// And at the same time they don't have to have a seperate portion of the
// code to deal with the unsubscription -- both sub and unsub will be in 
// the same context and in the same block.
public class LifeCycleDelegates : MonoBehaviour {
    //TODO: Rewrite using dicitonary
    public Action OnDestroyDelegate;
    public Action OnRecycleDelegate;
    public Action OnFirstFixedUpdateDelegate;
    [HideInInspector] public bool afterFirstFixedUpdate = false;

    private Poolable poolable = null;
    private List<SubInfo> removeTracker = new List<SubInfo>();

// Mark: Sub functions and convenient wrappers
    public SubInfo SubOnRecycle(Action sub) {
        return Sub(sub, SubEvent.OnRecycle, UnsubEvent.OnDestroy);
    }

    public SubInfo OnceOnDestroy(Action sub) {
        return Sub(sub, SubEvent.OnDestroy, UnsubEvent.OnDestroy);
    }

    public SubInfo OnceOnFirstFixedUpdate(Action sub) {
        return Sub(sub, SubEvent.OnFirstFixedUpdate, UnsubEvent.OnFirstFixedUpdate);
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
            default: break;
        }
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
// End: Life Cycle Events

    private IEnumerator WaitForFixedUpateRoutine() {
        yield return new WaitForFixedUpdate();
        this.afterFirstFixedUpdate = true;
        OnFirstFixedUpdate();
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
    }
}
