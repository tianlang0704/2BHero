using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour {
    [Header("Bullet Resource")]
    public string spawnMarkName = "SpawnMark";
    public Poolable bullet = null;
    [Header("Bullet Physics")]
    public float forceMin = 0f;
    public float forceDelta = -0.20f;
    public float riseFactor = 0.1f;
    public float torque = 0.001f;
    [Header("Animation")]
    public float ShootDelay = 0f;

    private Transform spawnMark;

    protected virtual Animator targetAnimator { get { return this.GetComponent<Animator>(); } }

// Mark: Shoot functions
    public void Aim() {
        this.targetAnimator.SetBool("aim", true);
    }

    public void AimStop() {
        this.targetAnimator.SetBool("aim", false);
    }

    public void Shoot(float factor) {
        this.targetAnimator.SetTrigger("attack");
        DelayShoot(this.ShootDelay, factor);
    }

    public void DelayShoot(float delay, float factor) {
        StartCoroutine(DelayShootRoutine(delay, factor));
    }

    private IEnumerator DelayShootRoutine(float delay, float factor) {
        yield return new WaitForSeconds(delay);
        DoShoot(factor);
    }

    public void DoShoot(float factor) {
        float horizontalForce = this.forceMin + this.forceDelta * factor;
        Rigidbody2D newBulletRb2d = DelegateCenter.shared.GetPoolable(
            this.bullet,
            this.spawnMark.position,
            this.gameObject.transform.rotation)
            .GetComponent<Rigidbody2D>();
        if (newBulletRb2d) {
            newBulletRb2d.AddForce(
                new Vector2(horizontalForce, Math.Abs(horizontalForce * this.riseFactor)), 
                ForceMode2D.Impulse);
            newBulletRb2d.AddTorque(this.torque * (1.5f - factor), ForceMode2D.Impulse);
        }
    }
// End: Shoot functions

    private void Awake() {
        this.spawnMark = this.transform.Find(this.spawnMarkName);
    }
}
