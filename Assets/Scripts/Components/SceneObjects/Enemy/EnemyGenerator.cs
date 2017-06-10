using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyList {
    public List<Enemy> enemePrefabs = new List<Enemy>();

    public EnemyList(List<Enemy> list) {
        this.enemePrefabs = list;
    }

    public static implicit operator EnemyList(List<Enemy> list) {
        return new EnemyList(list);
    }
}

public class EnemyGenerator : MonoInjectable {
    [Header("Resources")]
    public List<EnemyList> enemyPrefabsForWaves = new List<EnemyList>();
    [Header("Spawn Params")]
    public float intervalConst = 3f;
    public float minInterval = 0.5f;




    [Inject]
    protected ObjectPoolController objectPoolController;
    [Inject]
    protected EnemyController enemyController;
    [Inject]
    protected DifficultyContoroller difficultyController;



    /// <summary>
    /// Clear all enemies in the scene
    /// </summary>
    public void ClearAllEnemies() {
        List<Enemy> allPrefabs = new List<Enemy>(
            this.enemyPrefabsForWaves
            .SelectMany(enemyList => enemyList.enemePrefabs)
            .Distinct());

        allPrefabs.ForEach((Enemy e) => {
            this.objectPoolController.RecycleAll(e.GetComponent<Poolable>());
        });
    }
    /// <summary>
    /// Method for spawn a random enemy
    /// </summary>
    public void SpawnEnemyRandomly() {
        // 1. Get enemies to spawn from wave settings
        RandomList<Enemy> enemiesToSpawn = new RandomList<Enemy>(GetEnemiesToSpawn(this.difficultyController.wave));
        // 2. If no prefab, don't spawn
        if (enemiesToSpawn.Count <= 0) { return; }
        // 3. Call SpawnEnemy to spawn at generator
        SpawnEnemy(enemiesToSpawn.GetRandom(), this.gameObject);
    }



    /// <summary>
    /// Return a set of enemy prefabs to use according to wave number
    /// Uses the last one if wave number is larger than wave settings
    /// </summary>
    /// <param name="waveIdx">Wave number</param>
    /// <returns></returns>
    private List<Enemy> GetEnemiesToSpawn(int waveIdx) {
        // 1. Must have at least one wave setting
        int maxWaveCount = this.enemyPrefabsForWaves.Count;
        if(maxWaveCount == 0) { throw new Exception("Generator must have at least one enemy wave setting"); }
        // 2. If wave number is larger than the max wave setting, use the last one.
        if(waveIdx >= maxWaveCount) { waveIdx = maxWaveCount - 1; }
        return this.enemyPrefabsForWaves[waveIdx].enemePrefabs;
    }
    /// <summary>
    /// Method for spawn an enemy at specified location
    /// </summary>
    /// <param name="enemyToSpawn">Enemy prefab used to spawn enemy</param>
    /// <param name="spawnMark">Spawn location mark</param>
    private void SpawnEnemy(Enemy enemyToSpawn, GameObject spawnMark) {
        Poolable enemyPoolable = enemyToSpawn.GetComponent<Poolable>();
        Railable enemyOnRail = this.objectPoolController.InstantiatePoolable(
            enemyPoolable,
            spawnMark.transform.position,
            spawnMark.transform.rotation)
            .GetComponent<Railable>();
        enemyOnRail.dropSpeed *= this.difficultyController.moveSpeedFactor;
        enemyOnRail.moveSpeed *= this.difficultyController.moveSpeedFactor;
    }



    protected override void Start() {
        base.Start();
        this.enemyController.RegisterGenerator(this);
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            this.enemyController.UnregisterGenerator(this);
        });
    }
}
