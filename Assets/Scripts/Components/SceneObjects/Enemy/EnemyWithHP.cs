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
    [Header("Animation Settings")]
    public Vector3 scoreOffset = new Vector3(0, 1, 0);
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
    [Inject]
    protected EffectController effectControler;









// Mark: Public interfaces
    virtual public void Hit(BulletWithDamage dmger) {
        this.hp -= dmger.damage;
        OnHit(dmger);
    }
// End: Public interfaces

// Mark: General internal functions
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

    private void AnimatdRecycle() {
        this.GetComponent<Animator>().SetTrigger("dying");
        this.audioSource.PlayOneShot(this.dieSounds.GetRandom(), 0.9f);
        base.Recycle(this.delayDeath);
    }

    private void Score() {
        this.effectControler.ShowScoreAt(this.transform.position + this.scoreOffset, 10);
        this.gameController.Score(this.score);
    }
// End: General internal functions

// Mark: Enemy Events
    virtual protected void OnDie() {
        // 1. Change layer so that bullet won't hit anymore
        this.gameObject.layer = LayerMask.NameToLayer("EnemyDead");
        // 2. Cancel drop if dropping
        this.GetComponent<Droppable>().CancelDrop();
        // 3. Disable rail movement
        this.GetComponent<Railable>().DisableMoveOnRail();
        // 4. Die in air or after drop to the ground
        if (!this.isDropOnDie) {
            AnimatdRecycle();
            Score();
        } else {
            this.GetComponent<Droppable>().Drop(() => {
                AnimatdRecycle();
                Score();
            });
        }
    }

    virtual protected void OnHit(BulletWithDamage dmger) {
        this.GetComponent<Animator>().SetTrigger("hit");
        this.audioSource.PlayOneShot(this.hitSounds.GetRandom(), 0.9f);
        if (!this.callOnDie && this.hp <= 0) {
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
    }

    protected override void OnRecycle() {
        base.OnRecycle();
    }

    protected override void Awake() {
        base.Awake();
        this.audioSource = this.GetComponent<AudioSource>();
    }
// End: Enemy Events
}
