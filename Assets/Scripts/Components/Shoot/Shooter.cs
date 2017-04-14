using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateCenter {
    public Action<Shooter> OnBulletCountChange;
}

[RequireComponent(typeof(LifeCycleDelegates))]
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
    [Header("Unit Settings")]
    public int magazineSize = 10;
    public float reloadDuration = 1f;
    public int bulletCount {
        get { return _bulletCount; }
        set {
            _bulletCount = value;
            if (DelegateCenter.shared.OnBulletCountChange != null) {
                DelegateCenter.shared.OnBulletCountChange(this);
            }
        }
    }

    private Transform spawnMark;
    private int _bulletCount;
    private bool shootingEnabled = true;
    private bool isReloading = false;

    protected virtual Animator targetAnimator { get { return this.GetComponent<Animator>(); } }

// Mark: Shoot functions
    public void Shoot(float factor) {
        if (!this.shootingEnabled) { return; }
        this.targetAnimator.SetTrigger("attack");
        DelayShoot(this.ShootDelay, factor);
        this.bulletCount -= 1;
        if (this.bulletCount <= 0) { Reload(); }
    }

    public void DelayShoot(float delay, float factor) {
        StartCoroutine(DelayShootRoutine(delay, factor));
    }

    public void EnableShoot() {
        this.shootingEnabled = true;
    }

    public void DisableShoot() {
        this.shootingEnabled = false;
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

// Mark: Assistant functions
    public void Aim() {
        if (!this.shootingEnabled) { return; }
        this.targetAnimator.SetBool("aim", true);
    }

    public void AimStop() {
        this.targetAnimator.SetBool("aim", false);
    }

    public void Reload() {
        if (this.isReloading) { return; }
        this.isReloading = true;
        this.targetAnimator.SetTrigger("reload");
        DisableShoot();
        StartCoroutine(ReloadRoutine(this.reloadDuration, () => {
            this.isReloading = false;
            this.bulletCount = this.magazineSize;
            EnableShoot();
        }));
    }

    private IEnumerator ReloadRoutine(float duration, Action complete) {
        yield return new WaitForSeconds(duration);
        complete();
    }
// End: Assistant functions

    private void Awake() {
        this.spawnMark = this.transform.Find(this.spawnMarkName);
    }

    private void Start() {
        // Initialize delegates
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        DelegateCenter dc = DelegateCenter.shared;
        dc.OnGameResume += EnableShoot;
        dc.OnGamePause += DisableShoot;
        lc.OnceOnDestroy(() => {
            dc.OnGameResume -= EnableShoot;
            dc.OnGamePause -= DisableShoot;
        });
        lc.OnceOnFirstFixedUpdate(() => { this.bulletCount = this.magazineSize; });
    }
}
