using System;
using System.Collections.Generic;
using UnityEngine;

public class Poolable: MonoBehaviour {
    //Used to check if the object is appeared at least once
    public bool appearAtLeastOnce = false;
    [HideInInspector] public PoolItem poolItem = null;
    [HideInInspector] public Action recycleDelegate;
    [HideInInspector] public bool onDelayedRecycle = false;

    private Renderer r;
    private bool lastFrameVisible;
    private bool appeared;
    private bool firstFrameAfterEnable;

    public void Recycle() {
        MessengerController.shared.Recycle(this);
    }

    public void Recycle(float t) {
        MessengerController.shared.RecycleWithDelay(this, t);
    }

    private void Awake() {
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
