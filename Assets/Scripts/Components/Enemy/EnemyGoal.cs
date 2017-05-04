using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoal : MonoInjectable {
    [Inject]
    protected GameController gameController;
    [Inject]
    protected EnemyController enemyController;

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Enemy") {
            this.gameController.DeductLife(1);
            collision.GetComponent<Enemy>().Recycle();
        }
    }

    protected override void Start() {
        base.Start();
        this.enemyController.RegisterGoal(this);
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            this.enemyController.UnregisterGoal(this);
        });
    }
}
