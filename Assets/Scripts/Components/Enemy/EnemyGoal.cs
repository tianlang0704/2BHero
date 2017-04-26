using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoal : BComponentBase {
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Enemy") {
            base.GetDep<GameController>().DeductLife(1);
            collision.GetComponent<Enemy>().Recycle();
        }
    }


    protected override void Awake() {
        base.Awake();
        GameObject.FindObjectOfType<Loader>().DynamicInjection(this);
    }

    public void InjectDependencies(
        GameController gc
    ) {
        base.ClearDependencies();
        base.AddDep<GameController>(gc);
        base.isInjected = true;
    }
}
