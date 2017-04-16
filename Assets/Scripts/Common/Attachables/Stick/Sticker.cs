using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LifeCycleDelegates))]
public class Sticker : MonoBehaviour {
    [HideInInspector] public Action OnEnableStick;
    [HideInInspector] public Action OnDisableStick;

    [HideInInspector] public Stickable stuckTo = null;

    private Transform originalParent = null;
    private SubInfo stickTargetSubInfo;
    private Poolable poolable = null;
    private LifeCycleDelegates lifeCycle = null;

    virtual protected void OnCollisionEnter2D(Collision2D c) {
        Stickable stickable = c.gameObject.GetComponent<Stickable>();
        if (!this.stuckTo && stickable) {
            EnableStick(stickable);
        }
    }

    private void EnableStick(Stickable t) {
        if (this.stuckTo) { return; }
        // Stop collider and movements
        this.GetComponent<Collider2D>().enabled = false;
        Rigidbody2D r = this.GetComponent<Rigidbody2D>();
        if(r) {
            r.velocity = Vector2.zero;
            r.angularVelocity = 0f;
            r.isKinematic = true;
        }
        // Set parent under a empty gameObject with unit scale so that
        // the local scale of the sticker does not change.
        Transform sk = t.GetScaleKeeper();
        sk.parent = t.transform;
        this.transform.parent = sk;
        // Update stuckTo
        this.stuckTo = t;
        // Setup handler for when stick target recycle
        LifeCycleDelegates targetLCD = this.stuckTo.GetComponent<LifeCycleDelegates>();
        if (targetLCD) { this.stickTargetSubInfo = targetLCD.Sub(OnStickTargetDie, SubEvent.OnRecycle, UnsubEvent.OnRecycle); }
        // Call delegate
        if (this.OnEnableStick != null) { this.OnEnableStick(); }
    }

    private void OnStickTargetDie() {
        DisableStick();
    }

    private void DisableStick() {
        if (!this.stuckTo) { return; }
        // Reset physics
        this.GetComponent<Collider2D>().enabled = true;
        this.GetComponent<Rigidbody2D>().isKinematic = false;
        // Reset parents
        this.transform.parent = this.originalParent;
        // Unsub from listening to target recycle
        LifeCycleDelegates targetLCD = this.stuckTo.GetComponent<LifeCycleDelegates>();
        if (targetLCD) { targetLCD.Unsub(this.stickTargetSubInfo); }
        // Call event
        if (this.OnDisableStick != null) { this.OnDisableStick(); }
        this.stuckTo = null;
    }

    virtual protected void OnRecycle() {
        this.transform.parent = this.originalParent;
        DisableStick();
    }

    virtual protected void Awake() {
        this.lifeCycle = this.GetComponent<LifeCycleDelegates>();
        this.poolable = this.GetComponent<Poolable>();
        if(this.poolable) {
            this.lifeCycle.Sub(OnRecycle, SubEvent.OnRecycle, UnsubEvent.OnDestroy);
        }
        // Save original parent
        this.originalParent = this.transform.parent;
    }
}
