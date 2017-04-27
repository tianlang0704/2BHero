using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Script component for dropping with set gravity until hitting the ground
/// </summary>
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

    /// <summary>
    /// Called when drop starts
    /// </summary>
    public Action OnDropStart;
    /// <summary>
    /// Called when drop ends
    /// </summary>
    public Action OnDropEnd;



    /// <summary>
    /// Flag for checking if drop is in action
    /// </summary>
    private bool isDropping = false;
    /// <summary>
    /// Variable for saving drop routine when dropping
    /// </summary>
    private Coroutine dropRoutine = null;
    /// <summary>
    /// Original IsKinematic setting for Rigidbody2D before dropping
    /// </summary>
    private bool originalIsKinematic = false;
    /// <summary>
    /// Original gravidy setting for Rigidbody2D before dropping
    /// </summary>
    private float originalDropGravity = 0;



    /// <summary>
    /// Method for activating drop
    /// </summary>
    /// <param name="dropCallback"></param>
    public void Drop(Action dropCallback = null) {
        if (this.isDropping) { return; }
        this.dropRoutine = StartCoroutine(DropWaitLoop(dropCallback));
    }
    /// <summary>
    /// Method for cancelling dropping when drop in progress
    /// </summary>
    public void CancelDrop() {
        if (!this.isDropping) { return; }
        if (this.dropRoutine != null) {
            StopCoroutine(this.dropRoutine);
            this.dropRoutine = null;
        }
        ResetDrop();
    }



    /// <summary>
    /// The routine for actual drop function
    /// </summary>
    /// <param name="dropCallback">Called when drop ends</param>
    /// <param name="timeout">Force end timeout for the drop if it does not hit the ground</param>
    /// <returns></returns>
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
    /// <summary>
    /// Method to enable dropping used by the drop routine
    /// </summary>
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
    /// <summary>
    /// Method to disable dropping used by the drop routine
    /// </summary>
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
    /// <summary>
    /// Cancel dropping when recycling
    /// </summary>
    private void Awake() {
        this.GetComponent<LifeCycleDelegates>().SubOnRecycle(() => {
            CancelDrop();
        });
    }
    // End: Drop functions
}
