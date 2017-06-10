using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LifeCycleDelegates))]
[RequireComponent(typeof(Sticker))]
public class Bullet: MonoBehaviour {
    private Poolable poolable = null;
    private Sticker sticker = null;
    private LifeCycleDelegates lifeCycle = null;

    virtual protected void OnEnableStick() {
        this.poolable.Recycle(1);
        this.GetComponent<Animator>().SetBool("flying", false);
    }

    virtual protected void OnDisableStick() {
        this.GetComponent<Collider2D>().enabled = false;
        this.GetComponent<Rigidbody2D>().isKinematic = true;
    }

    virtual protected void OnRecycle() {
        this.GetComponent<Collider2D>().enabled = true;
        this.GetComponent<Rigidbody2D>().isKinematic = false;
    }

    virtual protected void OnCollisionEnter2D(Collision2D collision) {
        
    }

    virtual protected void Awake() {
        this.poolable = this.GetComponent<Poolable>();
        this.sticker = this.GetComponent<Sticker>();
        this.lifeCycle = this.GetComponent<LifeCycleDelegates>();
        // Subscribe to stickable events until destroy
        this.sticker.OnEnableStick += OnEnableStick;
        this.lifeCycle.OnceOnDestroy(() => { this.sticker.OnEnableStick -= OnEnableStick; });
        this.sticker.OnDisableStick += OnDisableStick;
        this.lifeCycle.OnceOnDestroy(() => { this.sticker.OnDisableStick -= OnDisableStick; });
        // Subscribe to pullable's unrecycle events
        this.lifeCycle.Sub(OnRecycle, SubEvent.OnRecycle, UnsubEvent.OnDestroy);
    }
}