using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GroundDetector))]
public class Railable : MonoBehaviour {
    [Header("Pysics Settings")]
    public Vector2 defaultDropSpeed = new Vector2(0, -3);
    public Vector2 defaultMoveSpeed = new Vector2(4, 0);
    public bool isFowardInAir = false;
    
    [HideInInspector] public Vector2 dropSpeed;
    [HideInInspector] public Vector2 moveSpeed;

    private bool originalIsKinematic = false;
    private bool isMoveOnRail = false;

    public void EnableMoveOnRail() {
        if (this.isMoveOnRail) { return; }
        Rigidbody2D rb2d = this.GetComponent<Rigidbody2D>();
        if (rb2d) {
            rb2d.isKinematic = this.originalIsKinematic;
        }
        this.isMoveOnRail = true;
    }

    public void DisableMoveOnRail() {
        if (!this.isMoveOnRail) { return; }
        Rigidbody2D rb2d = this.GetComponent<Rigidbody2D>();
        if (rb2d) {
            this.originalIsKinematic = rb2d.isKinematic;
            rb2d.isKinematic = false;
            rb2d.velocity = Vector2.zero;
        }
        this.isMoveOnRail = false;
    }

    virtual protected void OnEnable() {
        this.dropSpeed = this.defaultDropSpeed;
        this.moveSpeed = this.defaultMoveSpeed;
    }

    virtual protected void Update() {
        MoveOnRail();
    }

    private void MoveOnRail() {
        // Check master switch
        if (!this.isMoveOnRail) { return; }

        // Check if move forward in air
        if (this.isFowardInAir) {
            this.GetComponent<Rigidbody2D>().velocity = this.moveSpeed;
            return;
        }

        // Otherwise do drop to ground and move forward on ground
        if (this.GetComponent<GroundDetector>().isInAir) {
            this.GetComponent<Rigidbody2D>().velocity = this.dropSpeed;
        } else if (this.GetComponent<GroundDetector>().isOnGround) {
            this.GetComponent<Rigidbody2D>().velocity = this.moveSpeed;
        }
    }
}
