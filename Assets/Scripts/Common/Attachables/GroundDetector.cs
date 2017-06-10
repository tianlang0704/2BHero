using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyPositionStatus {
    InAir, OnGround
}

public class GroundDetector : MonoBehaviour {
    public EnemyPositionStatus defaultPosStat = EnemyPositionStatus.InAir;
    public string groundTag = "Ground";
    public float colliderBorderOffset = 0.0707f;
    [HideInInspector] public EnemyPositionStatus posStat;
    [HideInInspector] public Action OnLift;
    [HideInInspector] public Action OnLand;

    public bool isInAir { get { return this.posStat == EnemyPositionStatus.InAir; } }
    public bool isOnGround { get { return this.posStat == EnemyPositionStatus.OnGround; } }

    virtual protected void OnEnable() {
        this.posStat = this.defaultPosStat;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            Land();
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground") {
            Lift();
        }
    }

    private void FixedUpdate() {
        // If dynamic, use collider
        if(this.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Dynamic) { return; }

        // If not dynamic, use raycasting
        int layerMask = LayerMask.GetMask(new string[] { "Ground" });
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, Vector2.down, Mathf.Infinity, layerMask);
        if(hit && hit.collider.gameObject.tag == "Ground") {
            CapsuleCollider2D c2d = this.GetComponent<CapsuleCollider2D>();
            float bottomToCenter = -c2d.offset.y + (c2d.size.y / 2);
            if(hit.distance < bottomToCenter + this.colliderBorderOffset + Mathf.Epsilon) {
                Land();
            }else {
                Lift();
            }
        }
    }

    private void Land() {
        this.posStat = EnemyPositionStatus.OnGround;
        if (OnLand != null) { OnLand(); }
    }

    private void Lift() {
        this.posStat = EnemyPositionStatus.InAir;
        if (OnLift != null) { OnLift(); }
    }
}
