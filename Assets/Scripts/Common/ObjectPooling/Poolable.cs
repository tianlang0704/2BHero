using System;
using System.Collections.Generic;
using UnityEngine;

public class Poolable: MonoInjectable {
    /// <summary>
    /// Inspector variable for setting how many to cache in the pool
    /// </summary>
    public int amountToPool = 5;
    /// <summary>
    /// Inspector variable for if it shrinks back to the cache number set
    /// </summary>
    public bool isShrinkBack = true;
    /// <summary>
    /// Flag for check if it has to be shown in any camera for at least once
    /// </summary>
    public bool appearAtLeastOnce = false;




    /// <summary>
    /// Set by PoolObjectController when created by it to track pool info
    /// </summary>
    [HideInInspector] public PoolItem poolItem = null;
    /// <summary>
    /// Delegate called when poolable is recycled by controller
    /// </summary>
    [HideInInspector] public Action recycleDelegate;
    /// <summary>
    /// Flag for checking if this poolable is on delayed recycle
    /// </summary>
    [HideInInspector] public bool onDelayedRecycle = false;




    private Renderer r;
    private bool lastFrameVisible;
    private bool isAppearedOnce;
    private bool firstFrameAfterEnable;





    // Dependencies
    [Inject]
    protected ObjectPoolController opc;

    public void Recycle() {
        this.opc.Recycle(this);
    }

    public void Recycle(float t) {
        this.opc.Recycle(this, t);
    }

    protected override void Awake() {
        base.Awake();
        this.r = this.gameObject.GetComponent<Renderer>();
    }

    private void Update() {
        UpdateActive();
    }

    private void OnEnable() {
        // Make sure active status is updated on the first frame after enable
        this.firstFrameAfterEnable = true;
        // Reset appeared
        this.isAppearedOnce = !this.appearAtLeastOnce;
    }

    private void UpdateActive() {
        // Check for visibility change, and need to set the visibility on first frame
        // And recycle poolable object if it's not visible anymore
        if (this.r.isVisible != lastFrameVisible || this.firstFrameAfterEnable) {
            // 1. Once visible changed or on first frame, set last frame visible
            this.lastFrameVisible = this.r.isVisible;
            // 2. If on first frame after enable, set the flag to false
            if (this.firstFrameAfterEnable) { this.firstFrameAfterEnable = false; }

            // 3. Recycle according to appear once setting and visible status
            if (this.isAppearedOnce && !this.r.isVisible) {
                // 3a. if it's already appeared and is not visible, recycle it
                Recycle();
            }else {
                // 3b. If not appeared, update is appeared flag to visible
                this.isAppearedOnce = this.r.isVisible;
            }
        }
    }
}
