using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateCenter {
    public Action<int> OnDifficultyIncrement;
    public Action OnGameOver;
    public Action OnGameStart;
    public Action OnGamePause;
    public Action OnGameResume;
}

/// <summary>
/// Class for managing game stats and pause-start
/// </summary>
public class GameController : MonoInjectable {
    public bool isPaused { get { return Time.timeScale == 0; } set { Time.timeScale = value ? 0 : 1; } }

    // Private variables for internal use, self explanatory
    private bool isGameStarted = false;
    private bool isInitDone = false;
    private bool isFirstFixedUpdateAfterStart = true;

    // Dependencies
    [Inject]
    protected DelegateCenter delegateCenter;
    [Inject]
    protected SceneController sceneController;
    [Inject]
    protected DifficultyContoroller difficultyContoroller;
    [Inject]
    protected EnemyController enemyController;
    [Inject]
    protected SoundController soundController;
    [Inject]
    protected PlayerController playerController;
    [Inject]
    protected WindowController windowController;
    [Inject]
    protected ScoringController scoringController;









// Mark: Game controls
    /// <summary>
    /// Toggle game pause resume
    /// </summary>
    /// <param name="pauseDo">Called when game paused</param>
    /// <param name="resumeDo">Called when game resumed</param>
    public void GamePauseResume(Action pauseDo = null, Action resumeDo = null) {
        if (this.isPaused) {
            GameResume();
            if(resumeDo != null) { resumeDo(); }
        }else {
            GamePause();
            if (pauseDo != null) { pauseDo(); }
        }
    }
    /// <summary>
    /// Method for pausing the gmae
    /// </summary>
    public void GamePause() {
        if (this.isPaused) { return; }
        this.isPaused = true;
        if (this.delegateCenter.OnGamePause != null) { this.delegateCenter.OnGamePause(); }
    }
    /// <summary>
    /// Method for resuming the game
    /// </summary>
    public void GameResume() {
        if (!this.isPaused) { return; }
        this.isPaused = false;
        if (this.delegateCenter.OnGameResume != null) { this.delegateCenter.OnGameResume(); }
    }
    /// <summary>
    /// Method for restarting the game
    /// </summary>
    public void GameRestart() {
        // End the game
        GameOver(false);
        // Reload game scene
        this.sceneController.LoadGameScene();
        // Start the game
        GameStart();
    }
    /// <summary>
    /// Init safe method for starting the game
    /// </summary>
    public void GameStartInitSafe() {
        // If init is not done, set it to start when first update hits
        // if init is done, just start the game
        if (!this.isInitDone) {
            this.GetComponent<LifeCycleDelegates>().OnceOnFirstUpdate(() => {
                GameStart();
            });
        } else {
            GameStart();
        }
    }
    /// <summary>
    /// Method for starting the game
    /// </summary>
    public void GameStart() {
        if (this.isGameStarted) { return; }
        // Reset first fixed update flag
        this.isFirstFixedUpdateAfterStart = true;
        // Reset game stats
        this.scoringController.ResetLife();
        this.scoringController.ResetScore();
        // Reset pause stats
        GameResume();
        // Start difficulty loop to inscrease difficulty gradually
        this.difficultyContoroller.StartDifficultLoop(() => {
            this.isGameStarted = true;
            this.enemyController.StartSpawning();
            if (this.delegateCenter.OnGameStart != null) { this.delegateCenter.OnGameStart(); }
        });
    }
    /// <summary>
    /// Method for ending the game
    /// </summary>
    /// <param name="callOnGameover">True for calling the OnGameOver delegate</param>
    public void GameOver(bool callOnGameover = true) {
        if (!this.isGameStarted) { return; }
        this.isGameStarted = false;
        this.difficultyContoroller.StopDifficultLoop();
        this.enemyController.StopSpawning();
        this.enemyController.ClearAllEnemies();
        if(this.delegateCenter.OnGameOver != null && callOnGameover) { this.delegateCenter.OnGameOver(); }
        
    }
// End: Game controls

// Mark: Game stats

// End: Game stats

// Mark: Game events
    /// <summary>
    /// Method called when game starts
    /// </summary>
    protected virtual void OnGameStart() {

    }
    /// <summary>
    /// Method called when game overs
    /// </summary>
    protected virtual void OnGameOver() {
        this.GameResume();
        this.scoringController.ShowScore();
    }
    /// <summary>
    /// Method called when game pauses
    /// </summary>
    protected virtual void OnGamePause() {
        this.soundController.BlurBGM();
    }
    /// <summary>
    /// Method called when gmae resumes
    /// </summary>
    protected virtual void OnGameResume() {
        this.soundController.NormalizeBGM();
    }
    /// <summary>
    /// Method called when first update after starts
    /// </summary>
    protected virtual void OnFirstUpdateAfterStart() {
        this.scoringController.ResetLife();
        this.scoringController.ResetScore();
    }
    /// <summary>
    /// Method called when normal goal is reached by enemy
    /// </summary>
    /// <param name="e">The enemy reached the goal</param>
    protected virtual void OnNormalGoal(Enemy e) {
        this.scoringController.DeductLife(1);
        e.Recycle();
    }
    /// <summary>
    /// Method called when a correct enemy reached the rps goal
    /// </summary>
    /// <param name="e">The enemy reached the goal</param>
    protected virtual void OnRPSGoalCorrect(Enemy e) {
        this.scoringController.Score(30);
        e.Recycle();
    }
    /// <summary>
    /// Method called when a incorrect enemy reached the rps goal
    /// </summary>
    /// <param name="e">The enemy reached the goal</param>
    protected virtual void OnRPSGoalIncorrect(Enemy e) {
        this.scoringController.DeductLife(1);
        e.Recycle();
    }

// End: Game events
    /// <summary>
    /// Monobehaviour method for FixedUpdate
    /// </summary>
    private void FixedUpdate() {
        if (this.isFirstFixedUpdateAfterStart) {
            this.isFirstFixedUpdateAfterStart = false;
            OnFirstUpdateAfterStart();
        }
    }

// Mark: initialization
    protected override void Start() {
        base.Start();
        // Setup delegates
        DelegateCenter mc = this.delegateCenter;
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        mc.OnGameOver += OnGameOver;
        mc.OnGameStart += OnGameStart;
        mc.OnGamePause += OnGamePause;
        mc.OnGameResume += OnGameResume;
        mc.OnNormalGoal += OnNormalGoal;
        mc.OnRPSGoalCorrect += OnRPSGoalCorrect;
        mc.OnRPSGoalIncorrect += OnRPSGoalIncorrect;

        lc.OnceOnDestroy(() => {
            mc.OnGameOver -= OnGameOver;
            mc.OnGameStart -= OnGameStart;
            mc.OnGamePause -= OnGamePause;
            mc.OnGameResume -= OnGameResume;
            mc.OnNormalGoal -= OnNormalGoal;
            mc.OnRPSGoalCorrect -= OnRPSGoalCorrect;
            mc.OnRPSGoalIncorrect -= OnRPSGoalIncorrect;
        });
        
        lc.OnceOnFirstFixedUpdate(() => {
            this.isInitDone = true;
        });
    }
// End: initialization
}
