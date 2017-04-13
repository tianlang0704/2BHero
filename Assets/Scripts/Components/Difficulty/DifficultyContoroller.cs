using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateCenter {
    public Action<Action> StartDifficultLoop;
    public Action StopDifficultLoop;
}

public struct DifficultyFactors {
    public float moveSpeed;
    public float spawnInterval;
    public float hp;
}

[System.Serializable]
public class IndexWrapper {
    public List<int> enemyIndexes = new List<int>();

    public IndexWrapper(List<int> list) {
        this.enemyIndexes = list;
    }
    
    public static implicit operator IndexWrapper(List<int> list) {
        return new IndexWrapper(list);
    }
}

public class DifficultyContoroller : ControllerBase {
    public float waveDuration = 7f;
    public List<float> enemySpeedFactors = new List<float>() { 0.4f };
    public float enemySpeedInfStep = 0.04f;
    public List<float> enemyIntervalFactors = new List<float>() { 1f };
    public float enemyIntervalInfStep = -0.03f;
    public List<float> enemyHPFactors = new List<float>() { 1 };
    public float enemyHPInfStep = 0;
    public List<int> rngPosMaxes = new List<int>() { 1, 1, 2, 2, 2, 2, 2, 3 };
    public List<IndexWrapper> enemyIndexes = new List<IndexWrapper>() {
        new List<int>() { 0 },
        new List<int>() { 0, 1, 2 },
    };

    public DifficultyFactors GetFactors(int index) {
        DifficultyFactors newFactors = new DifficultyFactors();
        newFactors.moveSpeed = GetMoveSpeedFactor(index);
        newFactors.spawnInterval = GetSpawnIntervalFactor(index);
        newFactors.hp = GetHPFactor(index);
        return newFactors;
    }

    public float GetMoveSpeedFactor(int index) {
        return FactorGenerator(this.enemySpeedFactors, this.enemySpeedInfStep, index);
    }

    public float GetSpawnIntervalFactor(int index) {
        return FactorGenerator(this.enemyIntervalFactors, this.enemyIntervalInfStep, index);
    }

    public float GetHPFactor(int index) {
        return FactorGenerator(this.enemyHPFactors, this.enemyHPInfStep, index);
    }

    public int GetRNGPosMaxFactor(int index) {
        int posMaxIdx = this.rngPosMaxes.Count - 1;
        return this.rngPosMaxes[index > posMaxIdx ? posMaxIdx : index];
    }

    private List<int> GetEnemyIndexesForWave(int index) {
        int idxMax = this.enemyIndexes.Count - 1;
        if(index > idxMax || idxMax < 0) { return null; }
        return this.enemyIndexes[index].enemyIndexes;
    }

    private float FactorGenerator(List<float> factors, float infStep, int wantedIdx) {
        // Difficulty Factor Generator Description
        // 
        // Result factor = Const Factor + Infinit Step Factor
        //
        // 1. When Const Factors Count == 0
        // Const index will be the maxIdx = -1 when the count is 0,
        // which will make the const factor 0 which will effectively
        // make the result factor wanted index (0) - max index (-1) = 1 step
        // into the inf step setting. (const 0 + 1 step of inf setting)
        //
        // 2. When Const Factors Count > 0 && Wanted Index < Max Index (Count - 1)
        // On the other hand if there is any const factor settings,
        // the const index will be positive and act as the index in
        // selecting the const factor in the const factors array. When 
        // wanted index is < max index, the const index == wanted index 
        // which makes the inf step 0 (wanted index - wanted index) thus 
        // making the value of the inf steps 0, which makes the result 
        // factor the result of the const settings only.
        //
        // 3. When Const Factors Count > 0 && Wanted Index > Max Index
        // And when wanted index > max index, the const index will be the max index
        // thus the last const factor in the const factors list will be selected,
        // then combined with the amount of index steps larger than the const factor
        // list's max index * the step amount. That makes the difficulty setting grow
        // infinitely after the set amount of const factors.
        int maxIdx = factors.Count - 1;
        int constIdx = wantedIdx >= maxIdx ? maxIdx : wantedIdx;
        float constFactor = constIdx < 0 ? 0 : factors[constIdx];
        float infStepFactor = (wantedIdx - constIdx) * infStep;
        return constFactor + infStepFactor;
    }


// Mark: Difficulty loop functions
    private Coroutine difficultyLoop = null;
    private void StartDifficultLoop(Action gameStartCallback = null) {
        // Stop the previous loop if there is one
        if (this.difficultyLoop != null) {
            StopCoroutine(this.difficultyLoop);
            this.difficultyLoop = null;
        }
        this.difficultyLoop = StartCoroutine(DifficultyLoop(gameStartCallback));
    }

    private void StopDifficultLoop() {
        // Return is there is no loop to stop
        if (this.difficultyLoop == null) { return; }
        StopCoroutine(this.difficultyLoop);
        this.difficultyLoop = null;
    }

    private IEnumerator DifficultyLoop(Action gameStartCallback) {
        // Wait for the first fixedUpdate so it's essentially after
        // Start event and all the delegates are initialized
        yield return new WaitForFixedUpdate();
        int wave = 0;
        UpdateEnemyDifficulty(wave);
        if(gameStartCallback != null) { gameStartCallback(); }
        while (true) {
            UpdateEnemyDifficulty(wave);
            yield return new WaitForSeconds(this.waveDuration);
            wave += 1;
        }
    }

    private void UpdateEnemyDifficulty(int wave) {
        DelegateCenter mc = DelegateCenter.shared;
        mc.SetEnemyDropSpeed(GetMoveSpeedFactor(wave));
        mc.SetEnemyMoveSpeed(GetMoveSpeedFactor(wave));
        mc.SetEnemySpawnInterval(GetSpawnIntervalFactor(wave));
        mc.SetEnemyPositionIndexMax(GetRNGPosMaxFactor(wave));
        mc.ParseEnemyIndexes(GetEnemyIndexesForWave(wave));
    }
// Mark: Difficulty loop functions

// Mark: Singleton initialization
    public static DifficultyContoroller shared = null;
    override protected void Awake() {
        base.Awake();
        if (DifficultyContoroller.shared == null) {
            DifficultyContoroller.shared = this;
        } else if (DifficultyContoroller.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    protected override void InitializeDelegates() {
        base.InitializeDelegates();
        DelegateCenter mc = DelegateCenter.shared;
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        mc.StartDifficultLoop += StartDifficultLoop;
        mc.StopDifficultLoop += StopDifficultLoop;
        lc.OnceOnDestroy(() => {
            mc.StartDifficultLoop -= StartDifficultLoop;
            mc.StopDifficultLoop -= StopDifficultLoop;
        });
    }
// End: Singleton initialization
}
