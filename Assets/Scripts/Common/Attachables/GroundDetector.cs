using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyPositionStatus {
    InAir, OnGround
}

public class GroundDetector : MonoBehaviour {
    public EnemyPositionStatus defaultPosStat = EnemyPositionStatus.InAir;
    public string groundTag = "Ground";
    [HideInInspector] public EnemyPositionStatus posStat;
    [HideInInspector] public Action OnLift;
    [HideInInspector] public Action OnLand;

    public bool isInAir { get { return this.posStat == EnemyPositionStatus.InAir; } }
    public bool isOnGround { get { return this.posStat == EnemyPositionStatus.OnGround; } }

    virtual protected void OnEnable() {
        this.posStat = this.defaultPosStat;
    }

    virtual protected void OnTriggerExit2D(Collider2D collision) {
        // Simple ground check for now
        if (collision.gameObject.tag == "Ground") {
            this.posStat = EnemyPositionStatus.InAir;
            if (OnLift != null) { OnLift(); }
        }
    }

    virtual protected void OnTriggerEnter2D(Collider2D collision) {
        // Simple ground check for now
        if (collision.gameObject.tag == "Ground") {
            this.posStat = EnemyPositionStatus.OnGround;
            if (OnLand != null) { OnLand(); }
        }
    }
}
