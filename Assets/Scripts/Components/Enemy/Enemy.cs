using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyPositionStatus {
    InAir, OnGround
}

[RequireComponent(typeof(Poolable))]
[RequireComponent(typeof(Railable))]
[RequireComponent(typeof(LifeCycleDelegates))]
[RequireComponent(typeof(GroundDetector))]
public class Enemy : MonoBehaviour {
    virtual public void Recycle(float delay) {
        this.GetComponent<Poolable>().Recycle(delay);
    }

    virtual public void Recycle() {
        this.GetComponent<Poolable>().Recycle();
    }

    virtual protected void OnEnable() {
        OnSpawn();
    }

    virtual protected void OnSpawn() { }

    virtual protected void OnRecycle() { }

    virtual protected void OnCollisionEnter2D(Collision2D collision) { }

    virtual protected void Awake() {
        this.GetComponent<LifeCycleDelegates>().SubOnRecycle(OnRecycle);
    }
}
