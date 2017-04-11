﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class DelegateCenter {
    public Action StartSpawningEnemy;
    public Action StopSpawningEnemy;
    public Action ClearAllEnemies;
    public Action<float> SetEnemyDropSpeed;
    public Action<float> SetEnemyMoveSpeed;
    public Action<float> SetEnemySpawnInterval;
    public Action<int> SetEnemyPositionIndexMax;
    public Action<List<int>> ParseEnemyIndexes;
}

public class EnemyController : ControllerBase {
    [Header("Resources")]
    public List<string> skyMarkNames = new List<string>() { "E2", "E3" };
    public List<string> groundMarkNames = new List<string>() { "E1" };
    public List<Enemy> enemyPrefabs = new List<Enemy>();
    public string enemyGoalMarkName = "EG1";
    public EnemyGoal goalPrefab = null;
    [Header("Spawn Params")]
    public float intervalConst = 3f;
    public float minInterval = 0.5f;
    [Header("Difficulty Factors(Set by difficulty controller)")]
    public float dropSpeed;
    public float moveSpeed;
    public float spawnInterval;
    public int rngPosIdxMax;

    private List<GameObject> skyPosMarks = new List<GameObject>();
    private List<GameObject> groundPosMarks = new List<GameObject>();
    private List<GameObject> allPosMarks = new List<GameObject>();
    private List<Enemy> enemiesToSpawn = new List<Enemy>();
    private Coroutine spawningRouting = null;

    public void StartSpawning() {
        if (this.spawningRouting != null) { return; }
        this.spawningRouting = StartCoroutine(EnemySpawningLoop());
    }

    public void StopSpawning() {
        if(this.spawningRouting == null) { return; }
        StopCoroutine(this.spawningRouting);
        this.spawningRouting = null;
    }

    public void ClearAllEnemies() {
        this.enemyPrefabs.ForEach((Enemy e) => {
            DelegateCenter.shared.RecycleAll(e.GetComponent<Poolable>());
        });
    }

    private IEnumerator EnemySpawningLoop() {
        while (true) {
            SpawnEnemyRandomly();
            float interval = this.intervalConst * this.spawnInterval;
            yield return new WaitForSeconds(interval < this.minInterval ? this.minInterval : interval);
        }
    }

    public void SpawnEnemyRandomly() {
        int spawnCount = this.enemiesToSpawn.Count;
        if(spawnCount  <= 0) { Debug.Log("There no enemy to spawn."); return; }

        System.Random rng = new System.Random();
        int enemyIdx = rng.Next(0, spawnCount);
        int spotMax = this.allPosMarks.Count;
        int spotIdx = rng.Next(0, this.rngPosIdxMax > spotMax ? spotMax : this.rngPosIdxMax);
        SpawnEnemy(this.enemiesToSpawn[enemyIdx], this.allPosMarks[spotIdx]);
    }

    public void SpawnEnemy(Enemy enemyToSpawn, GameObject spawnMark) {
        Poolable enemyPoolable = enemyToSpawn.GetComponent<Poolable>();
        Railable enemyOnRail = DelegateCenter.shared.GetPoolable(
            enemyPoolable,
            spawnMark.transform.position,
            spawnMark.transform.rotation)
            .GetComponent<Railable>();
        enemyOnRail.dropSpeed *= this.dropSpeed;
        enemyOnRail.moveSpeed *= this.moveSpeed;
    }

    public void ParseEnemyIndexes(List<int> indexes) {
        if(indexes == null || indexes.Count == 0) {
            this.enemiesToSpawn = new List<Enemy>(this.enemyPrefabs);
            return;
        }

        int idxMax = this.enemyPrefabs.Count - 1;
        this.enemiesToSpawn.Clear();
        indexes.ForEach((int idx) => {
            if (idx > idxMax || idx < 0) { return; }
            this.enemiesToSpawn.Add(this.enemyPrefabs[idx]);
        });
    }

// Mark: Scene initialization
    public void SetupEnemyControllerForScene() {
        // Initialize enmey generating positions
        ParsePositionsMarks();
        // Initialize enemy goal
        GenerateGoalsFromMarks();
    }

    private void ParsePositionsMarks() {
        this.skyPosMarks.Clear();
        this.groundPosMarks.Clear();
        this.allPosMarks.Clear();

        // Parse enemy generation position marks
        foreach (string s in this.skyMarkNames) {
            GameObject go = GameObject.Find(s);
            if (!go) { Debug.Log("No mark object found for sky position: " + s); }
            this.skyPosMarks.Add(go);
        }
        foreach (string s in this.groundMarkNames) {
            GameObject go = GameObject.Find(s);
            if (!go) { Debug.Log("No mark object found for ground position: " + s); }
            this.groundPosMarks.Add(go);
        }
        this.allPosMarks.AddRange(this.groundPosMarks);
        this.allPosMarks.AddRange(this.skyPosMarks);
    }

    private void GenerateGoalsFromMarks() {
        List<GameObject> gos = new List<GameObject>(GameObject.FindObjectsOfType<GameObject>());
        gos.ForEach((GameObject go) => {
            if (go.name != this.enemyGoalMarkName) { return; }
            GameObject scaleKeeper = new GameObject("ScaleKeeper");
            scaleKeeper.transform.parent = go.transform.parent;
            Instantiate(
                this.goalPrefab,
                go.transform.position,
                go.transform.rotation,
                scaleKeeper.transform);
        });
    }
// End: Scene initialization

// Mark: Singleton initialization
    public static EnemyController shared = null;
    override protected void Awake() {
        base.Awake();
        if (EnemyController.shared == null) {
            EnemyController.shared = this;
        } else if (EnemyController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    override protected void InitializeDelegates() {
        base.InitializeDelegates();
        DelegateCenter mc = DelegateCenter.shared;
        // Enemy parameter settings
        Action<float> SetEnemyDropSpeed = (value)=> { this.dropSpeed = value; };
        mc.SetEnemyDropSpeed += SetEnemyDropSpeed;
        Action<float> SetEnemyMoveSpeed = (value) => { this.moveSpeed = value; };
        mc.SetEnemyMoveSpeed += SetEnemyMoveSpeed;
        Action<float> SetEnemySpawnInterval = (value) => { this.spawnInterval = value; };
        mc.SetEnemySpawnInterval += SetEnemySpawnInterval;
        Action<int> SetEnemyPositionIndexMax = (value) => { this.rngPosIdxMax = value; };
        mc.SetEnemyPositionIndexMax += SetEnemyPositionIndexMax;
        mc.ParseEnemyIndexes += ParseEnemyIndexes;

        // Spawning functions
        mc.StartSpawningEnemy += StartSpawning;
        mc.StopSpawningEnemy += StopSpawning;
        mc.ClearAllEnemies += ClearAllEnemies;
        
        this.lifeCycle.OnceOnDestroy(() => {
            mc.SetEnemyDropSpeed -= SetEnemyDropSpeed;
            mc.SetEnemyMoveSpeed -= SetEnemyMoveSpeed;
            mc.SetEnemySpawnInterval -= SetEnemySpawnInterval;
            mc.SetEnemyPositionIndexMax -= SetEnemyPositionIndexMax;
            mc.ParseEnemyIndexes -= ParseEnemyIndexes;

            mc.StartSpawningEnemy -= StartSpawning;
            mc.StopSpawningEnemy -= StopSpawning;
            mc.ClearAllEnemies -= ClearAllEnemies;
        });
    }
// End: Singleton initialization
}
