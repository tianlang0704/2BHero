using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
[RequireComponent(typeof(Railable))]
[RequireComponent(typeof(LifeCycleDelegates))]
[RequireComponent(typeof(GroundDetector))]
public class Enemy : MonoInjectable {
    public string enemyName = "defaultName";
    public bool isDropOnSpawn = false;
    public bool isDropOnDie = false;
    public bool isRailOnSpawn = true;










    public void Iconize(bool isEnabled) {
        this.EnableAllColliders(false);
        this.EnableRailing(false);
        this.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
        this.transform.localPosition = Vector2.zero;
        this.transform.localScale = new Vector2(1, 1);
    }

    public void EnableAllColliders(bool isEnabled) {
        foreach(Collider c in this.GetComponents<Collider>()) {
            c.enabled = isEnabled;
        }
    }

    public void EnableRailing(bool isEnabled) {
        if (isEnabled) {
            this.GetComponent<Railable>().EnableMoveOnRail();
            this.GetComponent<Railable>().ResumeMoveOnRail();
        }else {
            this.GetComponent<Railable>().DisableMoveOnRail();
        }
    }

    public virtual void Recycle(float delay) {
        this.GetComponent<Poolable>().Recycle(delay);
    }

    public virtual void Recycle() {
        this.GetComponent<Poolable>().Recycle();
    }

    protected virtual void OnEnable() {
        OnSpawn();
    }

    protected virtual void OnSpawn() {
        // Reset physics
        Collider2D c2d = this.GetComponent<Collider2D>();
        if (c2d) { c2d.enabled = true; }
        this.gameObject.layer = LayerMask.NameToLayer("Enemy");

        // Check if railing on spawn
        if (this.isRailOnSpawn) {
            this.EnableRailing(true);
        }

        // Check if drop on spawn
        if (this.isDropOnSpawn) {
            this.GetComponent<Railable>().PauseMoveOnRail();
            this.GetComponent<Droppable>().Drop(() => {
                this.GetComponent<Railable>().ResumeMoveOnRail();
            });
        }
    }

    protected virtual void OnRecycle() { }

    protected virtual void OnCollisionEnter2D(Collision2D collision) { }

    protected override void Awake() {
        base.Awake();
        this.gameObject.SetActive(false);
        this.GetComponent<LifeCycleDelegates>().SubOnRecycle(OnRecycle); ;
    }
}
