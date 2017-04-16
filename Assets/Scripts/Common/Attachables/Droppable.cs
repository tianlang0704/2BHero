using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LifeCycleDelegates))]
[RequireComponent(typeof(GroundDetector))]
public class Droppable : MonoBehaviour {
// This block of code facilitates drop function for 
// this game object. When Drop is called this game
// object is going to fall following the set physics
// attributes.
// TODO: Can be moved to an independent component
// Mark: Drop functions
    [Header("Drop Settings")]
    public float dropGravity = 1;
    public bool resetInertiaAfterDrop = true;
    public Action OnDropStart;
    public Action OnDropEnd;

    private bool isDropping = false;
    private Coroutine dropRoutine = null;
    private bool originalIsKinematic = false;
    private float originalDropGravity = 0;

    public void Drop(Action dropCallback = null) {
        if (this.isDropping) { return; }
        this.dropRoutine = StartCoroutine(DropWaitLoop(dropCallback));
    }

    public void CancelDrop() {
        if (!this.isDropping) { return; }
        if (this.dropRoutine != null) {
            StopCoroutine(this.dropRoutine);
            this.dropRoutine = null;
        }
        ResetDrop();
    }

    private IEnumerator DropWaitLoop(Action dropCallback = null, float timeout = 10) {
        SetDrop();
        // Call on event
        if (this.OnDropStart != null) { OnDropStart(); }
        // Record time for timeout and start ground check loop
        float timeDropBegin = Time.realtimeSinceStartup;
        float timeSinceDrop = 0;
        while (this.GetComponent<GroundDetector>().isInAir && timeSinceDrop < timeout) {
            timeSinceDrop = Time.realtimeSinceStartup - timeDropBegin;
            yield return null;
        }
        ResetDrop();
        // Call on event
        if (this.OnDropEnd != null) { OnDropEnd(); }
        if (dropCallback != null) { dropCallback(); }
        // Reset drop routine
        this.dropRoutine = null;
    }

    private void SetDrop() {
        // Set drop flag
        this.isDropping = true;
        // Set kinematic to false to drop with physics
        Rigidbody2D rb2d = this.GetComponent<Rigidbody2D>();
        if (rb2d) {
            this.originalIsKinematic = rb2d.isKinematic;
            rb2d.isKinematic = false;
            this.originalDropGravity = rb2d.gravityScale;
            rb2d.gravityScale = this.dropGravity;
        }
    }

    private void ResetDrop() {
        // Reset back kinematic settings
        Rigidbody2D rb2d = this.GetComponent<Rigidbody2D>();
        if (rb2d) {
            if (this.resetInertiaAfterDrop) { rb2d.velocity = Vector2.zero; }
            rb2d.isKinematic = this.originalIsKinematic;
            rb2d.gravityScale = this.originalDropGravity;
        }
        // Reset drop flag
        this.isDropping = false;
    }

    private void Awake() {
        this.GetComponent<LifeCycleDelegates>().SubOnRecycle(() => {
            CancelDrop();
        });
    }
    // End: Drop functions
}
