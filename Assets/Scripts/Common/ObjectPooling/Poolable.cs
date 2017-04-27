using System;
using System.Collections.Generic;
using UnityEngine;

public class Poolable: MonoInjectable {
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
    private bool appeared;
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
        this.appeared = !this.appearAtLeastOnce;
    }

    private void UpdateActive() {
        // Check for visible change
        if (this.r.isVisible != lastFrameVisible || this.firstFrameAfterEnable) {
            // Reset first frame flag after first frame
            if(this.firstFrameAfterEnable) { this.firstFrameAfterEnable = !this.firstFrameAfterEnable; }
            // Once visible changed, set last frame visible
            this.lastFrameVisible = this.r.isVisible;
            // Recycle according to appear once setting and visible status
            if (this.appeared && !this.r.isVisible) {
                Recycle();
            }else {
                this.appeared = this.r.isVisible;
            }
        }
    }
}
