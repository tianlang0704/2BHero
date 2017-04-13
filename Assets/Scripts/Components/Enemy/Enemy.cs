using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
[RequireComponent(typeof(Railable))]
[RequireComponent(typeof(LifeCycleDelegates))]
[RequireComponent(typeof(GroundDetector))]
public class Enemy : MonoBehaviour {
    public bool exhaustGase = false;
    public bool exhaustInAir = false;
    public bool exhaustOnGround = false;
    public GameObject exaustObj;

    private GroundDetector groundDetector;

    virtual public void Recycle(float delay) {
        this.GetComponent<Poolable>().Recycle(delay);
    }

    virtual public void Recycle() {
        this.GetComponent<Poolable>().Recycle();
    }

    virtual protected void OnEnable() {
        OnSpawn();
    }

    virtual protected void OnSpawn() {
        turnExhaust();
    }

    virtual protected void OnRecycle() { }

    virtual protected void OnLift() {
        turnExhaust();
    }

    virtual protected void OnLand() {
        turnExhaust();
    }

    virtual protected void OnCollisionEnter2D(Collision2D collision) { }

    virtual protected void Awake() {
        this.gameObject.SetActive(false);
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        lc.SubOnRecycle(OnRecycle);
        this.groundDetector = this.GetComponent<GroundDetector>();
        this.groundDetector.OnLift += OnLift;
        this.groundDetector.OnLand += OnLand;
        lc.OnceOnDestroy(() => {
            this.groundDetector.OnLift -= OnLift;
            this.groundDetector.OnLand -= OnLand;
        });
    }

    private void turnExhaust() {
        if (this.exhaustGase) { 
            if (
                (this.exhaustInAir && this.groundDetector.isInAir) || 
                (this.exhaustOnGround && this.groundDetector.isOnGround)
            ) {
                this.exaustObj.SetActive(true);
            }else if (
                (!this.exhaustInAir && this.groundDetector.isInAir) ||
                (!this.exhaustOnGround && this.groundDetector.isOnGround)
            ) {
                this.exaustObj.SetActive(false);
            }
        }
        
    }
}
