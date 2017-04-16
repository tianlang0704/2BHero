using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GroundDetector))]
public class Exhaustable : MonoBehaviour {
    public bool exhaustGas = false;
    public bool exhaustInAir = false;
    public bool exhaustOnGround = false;
    public GameObject exaustObj;

    private GroundDetector groundDetector;

    virtual protected void Awake() {
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        this.groundDetector = this.GetComponent<GroundDetector>();
        this.groundDetector.OnLift += OnLift;
        this.groundDetector.OnLand += OnLand;
        lc.OnceOnDestroy(() => {
            this.groundDetector.OnLift -= OnLift;
            this.groundDetector.OnLand -= OnLand;
        });
    }

    virtual protected void OnEnable() {
        turnExhaust();
    }

    virtual protected void OnLift() {
        turnExhaust();
    }

    virtual protected void OnLand() {
        turnExhaust();
    }

    private void turnExhaust() {
        if (this.exhaustGas) {
            if (
                (this.exhaustInAir && this.groundDetector.isInAir) ||
                (this.exhaustOnGround && this.groundDetector.isOnGround)
            ) {
                this.exaustObj.SetActive(true);
            } else if (
                 (!this.exhaustInAir && this.groundDetector.isInAir) ||
                 (!this.exhaustOnGround && this.groundDetector.isOnGround)
             ) {
                this.exaustObj.SetActive(false);
            }
        }

    }
}
