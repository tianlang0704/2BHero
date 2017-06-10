using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundDetector))]
public class Railable : MonoBehaviour {
    [Header("Pysics Settings")]
    public Vector3 defaultDropSpeed = new Vector3(0, -3);
    public Vector3 defaultMoveSpeed = new Vector3(4, 0);
    public bool isFowardInAir = false;
    
    [HideInInspector] public Vector3 dropSpeed;
    [HideInInspector] public Vector3 moveSpeed;

    private bool isMoveOnRailEnabled = false;
    private bool isMoveOnRailPaused = false;
    private bool isRigidBodyHooked = false;
    private bool isKinematicOriginal = false;
    private Rigidbody2D rb2d;
    private GroundDetector groundDetector;










    public void PauseMoveOnRail() {
        if (this.isMoveOnRailPaused) { return; }
        this.UnhookRigidBody();
        this.isMoveOnRailPaused = true;
    }

    public void ResumeMoveOnRail() {
        if (!this.isMoveOnRailPaused) { return; }
        this.HookRigidBody();
        this.isMoveOnRailPaused = false;
        this.MoveOnRail();
    }

    public void EnableMoveOnRail() {
        if (this.isMoveOnRailEnabled) { return; }
        this.HookRigidBody();
        this.isMoveOnRailEnabled = true;
        this.MoveOnRail();
    }

    public void DisableMoveOnRail() {
        if (!this.isMoveOnRailEnabled) { return; }
        this.UnhookRigidBody();
        this.isMoveOnRailEnabled = false;
    }

    private void HookRigidBody() {
        if (this.isRigidBodyHooked) { return; }
        if (this.rb2d) {
            this.isKinematicOriginal = rb2d.isKinematic;
            this.rb2d.isKinematic = true;
        }
        this.isRigidBodyHooked = true;
    }

    private void UnhookRigidBody() {
        if (!this.isRigidBodyHooked) { return; }
        if (this.rb2d) {
            this.rb2d.isKinematic = this.isKinematicOriginal;
            this.rb2d.velocity = Vector3.zero;
        }
        this.isRigidBodyHooked = false;
    }

    virtual protected void OnEnable() {
        this.dropSpeed = this.defaultDropSpeed;
        this.moveSpeed = this.defaultMoveSpeed;
    }

    virtual protected void FixedUpdate() {
        MoveOnRail();
    }

    private void MoveOnRail() {
        // Check switches
        if (!this.isMoveOnRailEnabled || this.isMoveOnRailPaused) { return; }

        // Check if move forward in air
        if (this.isFowardInAir && this.groundDetector.isInAir) {
            this.rb2d.velocity = this.moveSpeed;
            return;
        }

        // Otherwise do drop to ground and move forward on ground
        if (this.groundDetector.isInAir) {
            this.rb2d.velocity = this.dropSpeed;
        } else if (this.groundDetector.isOnGround) {
            this.rb2d.velocity = this.moveSpeed;
        }
    }

    private void Awake() {
        this.rb2d = this.GetComponent<Rigidbody2D>();
        this.groundDetector = this.GetComponent<GroundDetector>();
    }
}
