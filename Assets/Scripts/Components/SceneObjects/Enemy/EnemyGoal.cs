using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateCenter {
    public Action<Enemy> OnNormalGoal;
}

public class EnemyGoal : MonoInjectable {
    public bool isGoalEnabled = true;

    [Inject]
    protected GameController gameController;
    [Inject]
    protected EnemyController enemyController;
    [Inject]
    protected DelegateCenter delegateCenter;










    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Enemy") {
            if(this.delegateCenter.OnNormalGoal != null) {
                this.delegateCenter.OnNormalGoal(collision.gameObject.GetComponent<Enemy>());
            }
        }
    }
}
