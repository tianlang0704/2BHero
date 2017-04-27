using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoal : MonoInjectable {
    [Inject]
    protected GameController gameController;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Enemy") {
            this.gameController.DeductLife(1);
            collision.GetComponent<Enemy>().Recycle();
        }
    }
}
