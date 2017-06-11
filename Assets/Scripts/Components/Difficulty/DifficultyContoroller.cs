using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapper class for inspector index settings
/// </summary>
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

/// <summary>
/// Class for automatic difficulty increasing by chancing some of the parameters
/// over time for other object in the game to use
/// </summary>
public class DifficultyContoroller : MonoInjectable {
    // Settings for different waves, see descripotion below for FactorGenerator for more details
    public float waveDuration = 5f;
    public List<float> enemySpeedFactors = new List<float>() { 0.4f };
    public float enemySpeedInfStep = 0.04f;
    public List<float> enemyIntervalFactors = new List<float>() { 1f };
    public float enemyIntervalInfStep = -0.03f;
    public List<float> enemyHPFactors = new List<float>() { 1 };
    public float enemyHPInfStep = 0;
    public List<int> magazineSizes = new List<int>();
    public int magazineSizeInfStep = 0;
    public float scoreMagazineRatio = 0.1f;




    [HideInInspector]
    public float moveSpeedFactor = 1;
    [HideInInspector]
    public float spawnIntervalFactor = 1;
    [HideInInspector]
    public float hpFactor = 1;
    [HideInInspector]
    public int magazineSize = 1;
    [HideInInspector]
    public int wave = 0;




    [Inject]
    protected ScoringController scoringController;




    /// <summary>
    /// Get the multiply factor for movement speed according to wave setting
    /// </summary>
    /// <param name="waveIdx">the number of wave is at right now</param>
    /// <returns></returns>
    public float GetMoveSpeedFactor(int waveIdx) {
        return FactorGenerator(this.enemySpeedFactors, this.enemySpeedInfStep, waveIdx);
    }
    /// <summary>
    /// Get the multiply factor for spawn interval according to wave setting
    /// </summary>
    /// <param name="waveIdx">the number of wave is at right now</param>
    /// <returns></returns>
    public float GetSpawnIntervalFactor(int waveIdx) {
        return FactorGenerator(this.enemyIntervalFactors, this.enemyIntervalInfStep, waveIdx);
    }
    /// <summary>
    /// Get the multiply factor for HP according to wave setting
    /// </summary>
    /// <param name="waveIdx">the number of wave is at right now</param>
    /// <returns></returns>
    public float GetHPFactor(int waveIdx) {
        return FactorGenerator(this.enemyHPFactors, this.enemyHPInfStep, waveIdx);
    }
    /// <summary>
    /// Get the size of the magazine for the wave index
    /// </summary>
    /// <param name="waveIdx">the number of wave is at right now</param>
    /// <returns></returns>
    public int GetMagazineSize(int waveIdx) {
        int maxWaveIdx = this.magazineSizes.Count - 1;
        if (maxWaveIdx < 0) { throw new Exception("Must have at least one magazine size setting"); }
        if (waveIdx > maxWaveIdx) { waveIdx = maxWaveIdx; }
        return this.magazineSizes[waveIdx] + (int)(this.scoringController.score * this.scoreMagazineRatio);
    }




    /// <summary>
    /// Difficulty Factor Generator Description
    /// 
    /// Result factor = Const Factor + Infinit Step Factor
    /// Const factor - Set value for each wave
    /// Infinit Step - The amount of linear increment for each wave over the const
    /// factor setting
    /// 
    /// 1. When Const Factors Count == 0
    /// Const index will be the maxIdx = -1 when the count is 0,
    /// which will make the const factor 0 which will effectively
    /// make the result factor wanted index (0) - max index (-1) = 1 step
    /// into the inf step setting. (const 0 + 1 step of inf setting)
    ///
    /// 2. When Const Factors Count > 0 && Wanted Index < Max Index (Count - 1)
    /// On the other hand if there is any const factor settings,
    /// the const index will be positive and act as the index in
    /// selecting the const factor in the const factors array. When 
    /// wanted index is < max index, the const index == wanted index 
    /// which makes the inf step 0 (wanted index - wanted index) thus 
    /// making the value of the inf steps 0, which makes the result 
    /// factor the result of the const settings only.
    ///
    /// 3. When Const Factors Count > 0 && Wanted Index > Max Index
    /// And when wanted index > max index, the const index will be the max index
    /// thus the last const factor in the const factors list will be selected,
    /// then combined with the amount of index steps larger than the const factor
    /// list's max index * the step amount. That makes the difficulty setting grow
    /// infinitely after the set amount of const factors.
    /// </summary>
    /// <param name="factors">Const factor settings for each wave</param>
    /// <param name="infStep">Linear increment amount</param>
    /// <param name="wantedIdx">Wave index</param>
    /// <returns></returns>
    private float FactorGenerator(List<float> factors, float infStep, int wantedIdx) {
        int maxConstIdx = factors.Count - 1;
        int constIdx = wantedIdx >= maxConstIdx ? maxConstIdx : wantedIdx;
        float constFactor = constIdx < 0 ? 0 : factors[constIdx];
        float infStepFactor = (wantedIdx - constIdx) * infStep;
        return constFactor + infStepFactor;
    }




// Mark: Difficulty loop functions
    /// <summary>
    /// Holds the difficulty loop when it's running
    /// </summary>
    private Coroutine difficultyLoop = null;
    /// <summary>
    /// Method to start updating the factors according to settings
    /// </summary>
    /// <param name="gameStartCallback"></param>
    public void StartDifficultLoop(Action gameStartCallback = null) {
        // Stop the previous loop if there is one
        if (this.difficultyLoop != null) {
            StopCoroutine(this.difficultyLoop);
            this.difficultyLoop = null;
        }
        this.difficultyLoop = StartCoroutine(DifficultyLoop(gameStartCallback));
    }
    /// <summary>
    /// Method to stop updating the factors
    /// </summary>
    public void StopDifficultLoop() {
        // Return is there is no loop to stop
        if (this.difficultyLoop == null) { return; }
        StopCoroutine(this.difficultyLoop);
        this.difficultyLoop = null;
    }
    /// <summary>
    /// The execution loop itself
    /// </summary>
    /// <param name="gameStartCallback">Called when loop actually starts(after the first fixed update)</param>
    /// <returns></returns>
    private IEnumerator DifficultyLoop(Action gameStartCallback) {
        // Wait for the first fixedUpdate so it's essentially after
        // Start event and all the delegates are initialized
        yield return new WaitForFixedUpdate();
        wave = 0;
        UpdateFactors();
        if(gameStartCallback != null) { gameStartCallback(); }
        while (true) {
            yield return new WaitForSeconds(this.waveDuration);
            UpdateFactors();
            wave += 1;
        }
    }
    /// <summary>
    /// Called in difficulty loop to update factors
    /// </summary>
    /// <param name="wave">Wave Index</param>
    private void UpdateFactors() {
        this.moveSpeedFactor = GetMoveSpeedFactor(this.wave);
        this.spawnIntervalFactor = GetSpawnIntervalFactor(this.wave);
        this.hpFactor = GetHPFactor(this.wave);
        this.magazineSize = GetMagazineSize(this.wave);
    }
// Mark: Difficulty loop functions
}
