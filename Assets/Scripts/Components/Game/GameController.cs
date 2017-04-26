using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateCenter {
    public Action<int> Score;
    public Action<int> OnScoreChange;
    public Action<int> DeductLife;
    public Action<int> OnLifeChange;
    public Action<int> OnDifficultyIncrement;
    public Action OnGameOver;
    public Action OnGameStart;
    public Action OnGamePause;
    public Action OnGameResume;
    public Action<Action, Action> GamePauseResume;
    public Action GamePause;
    public Action GameResume;
    public Action GameRestart;
    public Action GameStart;
    public Action GameOver;
    public Func<bool> IsGamePaused;
}

public class GameController : ControllerBase {
    public int defaultScore = 0;
    public int defaultLife = 3;
    public DialogScore scoreMenuPrefab;
    public bool isPaused { get { return Time.timeScale == 0; } set { Time.timeScale = value ? 0 : 1; } }

    private int score;
    private int life;
    private bool isGameStarted = false;
    private bool isInitDone = false;
    private bool isFirstFixedUpdateAfterStart = true;

// Mark: Game controls
    public void GamePauseResume(Action pauseDo = null, Action resumeDo = null) {
        if (this.isPaused) {
            GameResume();
            if(resumeDo != null) { resumeDo(); }
        }else {
            GamePause();
            if (pauseDo != null) { pauseDo(); }
        }
    }

    public void GamePause() {
        if (this.isPaused) { return; }
        this.isPaused = true;
        if (DelegateCenter.shared.OnGamePause != null) { DelegateCenter.shared.OnGamePause(); }
    }

    public void GameResume() {
        if (!this.isPaused) { return; }
        this.isPaused = false;
        if (DelegateCenter.shared.OnGameResume != null) { DelegateCenter.shared.OnGameResume(); }
    }

    public void GameRestart() {
        GameOver(false);
        DelegateCenter.shared.LoadGameScene();
        GameStart();
    }

    public void GameStartInitSafe() {
        if (!this.isInitDone) {
            this.GetComponent<LifeCycleDelegates>().OnceOnFirstFixedUpdate(() => {
                GameStart();
            });
        } else {
            GameStart();
        }
    }

    public void GameStart() {
        if (this.isGameStarted) { return; }
        this.isFirstFixedUpdateAfterStart = true;
        ResetLife();
        ResetScore();
        GameResume();

        DelegateCenter.shared.StartDifficultLoop(() => {
            this.isGameStarted = true;
            DelegateCenter.shared.StartSpawningEnemy();
            if (DelegateCenter.shared.OnGameStart != null) { DelegateCenter.shared.OnGameStart(); }
        });
    }

    // Explicit overload for delegate
    public void GameOver() {
        GameOver(true);
    }

    public void GameOver(bool callOnGameover) {
        if (!this.isGameStarted) { return; }
        this.isGameStarted = false;
        DelegateCenter.shared.StopDifficultLoop();
        DelegateCenter.shared.StopSpawningEnemy();
        DelegateCenter.shared.ClearAllEnemies();
        if(DelegateCenter.shared.OnGameOver != null && callOnGameover) { DelegateCenter.shared.OnGameOver(); }
        
    }
// End: Game controls

// Mark: Game stats
    virtual protected void Score(int score) {
        this.score += score;
        if (DelegateCenter.shared.OnScoreChange != null) { DelegateCenter.shared.OnScoreChange(this.score); }
    }

    virtual protected void DeductLife(int amount) {
        this.life -= amount;
        if (DelegateCenter.shared.OnLifeChange != null) { DelegateCenter.shared.OnLifeChange(this.life); }
        if (this.life <= 0) {
            GameOver();
        }
    }

    public void ResetScore() {
        this.score = 0;
        if (DelegateCenter.shared.OnScoreChange != null) { DelegateCenter.shared.OnScoreChange(this.score); }
    }

    public void ResetLife() {
        this.life = this.defaultLife;
        if (DelegateCenter.shared.OnLifeChange != null) { DelegateCenter.shared.OnLifeChange(this.life); }
        
    }
// End: Game stats

// Mark: Game events
    virtual protected void OnGameStart() {

    }

    virtual protected void OnGameOver() {
        GameResume();
        DelegateCenter.shared.SetStatsBarActive(false);
        this.scoreMenuPrefab.ClonePrefabAndShow("Score", this.score, false, ()=> {
            DelegateCenter.shared.SetStatsBarActive(true);
        });
    }

    virtual protected void OnGamePause() {
        DelegateCenter.shared.BlurBGM();
    }

    virtual protected void OnGameResume() {
        DelegateCenter.shared.NormalizeBGM();
    }

    virtual protected void OnFirstUpdateAfterStart() {
        if (DelegateCenter.shared.OnScoreChange != null) { DelegateCenter.shared.OnScoreChange(this.score); }
        if (DelegateCenter.shared.OnLifeChange != null) { DelegateCenter.shared.OnLifeChange(this.life); }
    }
// End: Game events

    private void FixedUpdate() {
        if (this.isFirstFixedUpdateAfterStart) {
            this.isFirstFixedUpdateAfterStart = false;
            OnFirstUpdateAfterStart();
        }
    }

// Mark: Singleton initialization
    public static GameController shared = null;
    override protected void Awake() {
        base.Awake();
        if (GameController.shared == null) {
            GameController.shared = this;
        } else if (GameController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public override void InitializeDelegates() {
        base.InitializeDelegates();
        // Setup delegates
        DelegateCenter mc = DelegateCenter.shared;
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        mc.Score += Score;
        mc.DeductLife += DeductLife;
        mc.OnGameOver += OnGameOver;
        mc.OnGameStart += OnGameStart;
        mc.OnGamePause += OnGamePause;
        mc.OnGameResume += OnGameResume;
        mc.GamePauseResume += GamePauseResume;
        mc.GameRestart += GameRestart;
        mc.GamePause += GamePause;
        mc.GameResume += GameResume;
        mc.GameOver += GameOver;
        mc.GameStart += GameStartInitSafe;
        Func<bool> IsGamePaused = () => { return this.isPaused; };
        mc.IsGamePaused += IsGamePaused;

        lc.OnceOnDestroy(() => {
            mc.Score -= Score;
            mc.DeductLife -= DeductLife;
            mc.OnGameOver -= OnGameOver;
            mc.OnGameStart -= OnGameStart;
            mc.OnGamePause -= OnGamePause;
            mc.OnGameResume -= OnGameResume;
            mc.GamePauseResume -= GamePauseResume;
            mc.GameRestart -= GameRestart;
            mc.GamePause -= GamePause;
            mc.GameResume -= GameResume;
            mc.GameOver -= GameOver;
            mc.GameStart -= GameStartInitSafe;
            mc.IsGamePaused -= IsGamePaused;
        });
        
        lc.OnceOnFirstFixedUpdate(() => {
            this.isInitDone = true;
        });
    }
// End: Singleton initialization
}
