using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateCenter {
    public Action<Enemy> OnRPSGoalCorrect;
    public Action<Enemy> OnRPSGoalIncorrect;
}

public class EnemyGoalRPS : EnemyGoal {
    /// <summary>
    /// Enemy prefabs to RPS on
    /// </summary>
    public List<Enemy> rpsEnemyPrefabs = new List<Enemy>();
    /// <summary>
    /// Current rps result prefab
    /// </summary>
    [HideInInspector] public Enemy currentEnemy = null;
    /// <summary>
    /// RPS hit event
    /// </summary>
    [HideInInspector] public Action<EnemyGoalRPS> OnRPSHit;
    /// <summary>
    /// Internal RNG
    /// </summary>
    private System.Random random = new System.Random();








    /// <summary>
    /// The method to randomize the rps target by choosing one from the rps enemy prefabs
    /// </summary>
    /// <returns>The choosen rps target</returns>
    public Enemy RPS() {
        this.currentEnemy = this.rpsEnemyPrefabs[random.Next(this.rpsEnemyPrefabs.Count)];
        return currentEnemy;
    }
    /// <summary>
    /// Collision method used to check which enemy hit the goal and if it's the correct one.
    /// </summary>
    /// <param name="collision"></param>
    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Enemy") {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if(enemy.enemyName == this.currentEnemy.enemyName) {
                if (this.delegateCenter.OnRPSGoalCorrect != null) {
                    this.delegateCenter.OnRPSGoalCorrect(collision.gameObject.GetComponent<Enemy>());
                }
            } else {
                if (this.delegateCenter.OnRPSGoalIncorrect != null) {
                    this.delegateCenter.OnRPSGoalIncorrect(collision.gameObject.GetComponent<Enemy>());
                }
            }
            this.OnRPSHit(this);
        }
    }
}
