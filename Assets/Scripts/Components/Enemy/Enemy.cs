using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
[RequireComponent(typeof(Railable))]
[RequireComponent(typeof(LifeCycleDelegates))]
[RequireComponent(typeof(GroundDetector))]
public class Enemy : BComponentBase {
    public virtual void Recycle(float delay) {
        this.GetComponent<Poolable>().Recycle(delay);
    }

    virtual public void Recycle() {
        this.GetComponent<Poolable>().Recycle();
    }

    protected virtual void OnEnable() {
        OnSpawn();
    }

    protected virtual void OnSpawn() { }

    protected virtual void OnRecycle() { }

    protected virtual void OnCollisionEnter2D(Collision2D collision) { }

    protected override void Awake() {
        base.Awake();
        this.gameObject.SetActive(false);
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        lc.SubOnRecycle(OnRecycle);
    }
}
