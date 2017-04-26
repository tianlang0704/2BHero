using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoal : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Enemy") {
            Loader.shared.GetSingleton<DelegateCenter>().DeductLife(1);
            collision.GetComponent<Enemy>().Recycle();
        }
    }
}
