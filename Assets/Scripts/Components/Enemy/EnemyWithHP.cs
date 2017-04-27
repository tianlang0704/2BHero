using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Droppable))]
[RequireComponent(typeof(AudioSource))]
public class EnemyWithHP : Enemy {
    [Header("Unit Settings")]
    public int defaultHP = 10;
    public int score = 10;
    public bool isDropOnSpawn = false;
    public bool isDropOnDie = false;
    [Header("Animation Settings")]
    public float delayDeath = 1f;
    [Header("Sound Settings")]
    [SerializeField] private List<AudioClip> _hitSounds = new RandomList<AudioClip>();
    public RandomList<AudioClip> hitSounds { get { return (RandomList<AudioClip>)_hitSounds; } }
    [SerializeField] private List<AudioClip> _dieSounds = new RandomList<AudioClip>();
    public RandomList<AudioClip> dieSounds { get { return (RandomList<AudioClip>)_dieSounds; } }

    private int hp = 10;
    private BulletWithDamage killer = null;
    private bool callOnDie = false;
    private AudioSource audioSource;

    [Inject]
    protected GameController gameController;

    virtual public void Hit(BulletWithDamage dmger) {
        this.hp -= dmger.damage;
        OnHit(dmger);
    }

    virtual protected void LateUpdate() {
        // Call OnDie when hp <= 0 after everything is updated
        if (this.callOnDie) {
            this.callOnDie = false;
            OnDie();
        }
    }

    override protected void OnCollisionEnter2D(Collision2D collision) {
        base.OnCollisionEnter2D(collision);
        if (collision.gameObject.tag == "Bullet") {
            Hit(collision.gameObject.GetComponent<BulletWithDamage>());
        }
    }


// Mark: Enemy Events
    virtual protected void OnDie() {
        // Change layer so that bullet won't hit anymore
        this.gameObject.layer = LayerMask.NameToLayer("EnemyDead");
        // Cancel drop if dropping
        this.GetComponent<Droppable>().CancelDrop();
        // Disable rail movement
        this.GetComponent<Railable>().DisableMoveOnRail();
        // Die in air or after drop to the ground
        if (!this.isDropOnDie) {
            AnimatdRecycle();
        } else {
            this.GetComponent<Droppable>().Drop(() => {
                AnimatdRecycle();
            });
        }
    }

    private void AnimatdRecycle() {
        this.GetComponent<Animator>().SetTrigger("dying");
        this.audioSource.PlayOneShot(this.dieSounds.GetRandom(), 0.9f);
        base.Recycle(this.delayDeath);
    }

    virtual protected void OnHit(BulletWithDamage dmger) {
        this.GetComponent<Animator>().SetTrigger("hit");
        this.audioSource.PlayOneShot(this.hitSounds.GetRandom(), 0.9f);
        if (!this.callOnDie && this.hp <= 0) {
            this.gameController.Score(this.score);
            this.callOnDie = true;
            this.killer = dmger;
        }
    }

    override protected void OnSpawn() {
        base.OnSpawn();
        // Reset unit settings
        this.hp = this.defaultHP;
        this.killer = null;
        this.callOnDie = false;

        // Reset physics
        Collider2D c2d = this.GetComponent<Collider2D>();
        if (c2d) { c2d.enabled = true; }
        this.gameObject.layer = LayerMask.NameToLayer("Enemy");

        // Check if drop on spawn
        if (this.isDropOnSpawn) {
            this.GetComponent<Railable>().DisableMoveOnRail();
            this.GetComponent<Droppable>().Drop(() => {
                this.GetComponent<Railable>().EnableMoveOnRail();
            });
        } else {
            this.GetComponent<Railable>().EnableMoveOnRail();
        }
    }

    protected override void OnRecycle() {
        base.OnRecycle();
    }

    protected override void Awake() {
        base.Awake();
        this.audioSource = this.GetComponent<AudioSource>();
    }
    // Mark: Enemy Events
}
