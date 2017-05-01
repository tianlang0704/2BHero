using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoInjectable {
    [Header("Resources")]
    public List<string> skyMarkNames = new List<string>() { "E2", "E3" };
    public List<string> groundMarkNames = new List<string>() { "E1" };
    public List<Enemy> enemyPrefabs = new List<Enemy>();
    public string enemyGoalMarkName = "EG1";
    public EnemyGoal goalPrefab = null;
    [Header("Spawn Params")]
    public float intervalConst = 3f;
    public float minInterval = 0.5f;




    private RandomList<EnemyGenerator> generatorList = new RandomList<EnemyGenerator>();
    private List<EnemyGoal> goalList = new List<EnemyGoal>();
    private Coroutine spawningRouting = null;




    [Inject]
    protected DifficultyContoroller difficultyController;

    public void RegisterGenerator(EnemyGenerator enemyGenerator) {
        this.generatorList.Add(enemyGenerator);
    }

    public void UnregisterGenerator(EnemyGenerator enemyGenerator) {
        this.generatorList.Remove(enemyGenerator);
    }

    public void RegisterGoal(EnemyGoal goal) {
        this.goalList.Add(goal);
    }

    public void UnregisterGoal(EnemyGoal goal) {
        this.goalList.Remove(goal);
    }


    /// <summary>
    /// Method for start spawning enemy
    /// </summary>
    public void StartSpawning() {
        if (this.spawningRouting != null) { return; }
        this.spawningRouting = StartCoroutine(EnemySpawningLoop());
    }
    /// <summary>
    /// Method for stop spawning enemy
    /// </summary>
    public void StopSpawning() {
        if(this.spawningRouting == null) { return; }
        StopCoroutine(this.spawningRouting);
        this.spawningRouting = null;
    }
    /// <summary>
    /// Clear all enemies in the scene
    /// </summary>
    public void ClearAllEnemies() {
        this.generatorList.ForEach((EnemyGenerator eg) => {
            eg.ClearAllEnemies();
        });
    }
    /// <summary>
    /// Method for using a random generator to generate a random enemy
    /// </summary>
    public void SpawnEnemyRandomly() {
        int generatorCount = this.generatorList.Count;
        if (generatorCount <= 0) { Debug.Log("There no enemy generator."); return; }

        this.generatorList.GetRandom().SpawnEnemyRandomly();
    }



    /// <summary>
    /// Enemy spawn routine
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnemySpawningLoop() {
        while (true) {
            SpawnEnemyRandomly();
            float interval = this.intervalConst * this.difficultyController.spawnIntervalFactor;
            yield return new WaitForSeconds(interval < this.minInterval ? this.minInterval : interval);
        }
    }


}
