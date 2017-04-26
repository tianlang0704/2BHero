using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnder : BComponentBase {
    [Header("Resources")]
    public List<Enemy> enemiesToSpawn = new List<Enemy>();
    [Header("Difficulty Factors")]
    public float dropSpeed;
    public float moveSpeed;

    public void SpawnEnemyRandomly() {
        int spawnCount = this.enemiesToSpawn.Count;
        if (spawnCount <= 0) { Debug.Log("There no enemy to spawn."); return; }

        System.Random rng = new System.Random();
        int enemyIdx = rng.Next(0, spawnCount);
        SpawnEnemy(this.enemiesToSpawn[enemyIdx], this.gameObject);
    }

    public void SpawnEnemy(Enemy enemyToSpawn, GameObject spawnMark) {
        Poolable enemyPoolable = enemyToSpawn.GetComponent<Poolable>();
        Railable enemyOnRail = base.GetDep<ObjectPoolController>().GetPoolable(
            enemyPoolable,
            spawnMark.transform.position,
            spawnMark.transform.rotation)
            .GetComponent<Railable>();
        enemyOnRail.dropSpeed *= this.dropSpeed;
        enemyOnRail.moveSpeed *= this.moveSpeed;
    }


    protected override void Awake() {
        base.Awake();
        GameObject.FindObjectOfType<Loader>().DynamicInjection(this);
    }

    public void InjectDependencies(
        ObjectPoolController opc
    ) {
        base.ClearDependencies();
        base.AddDep<ObjectPoolController>(opc);
        base.isInjected = true;
    }
}
