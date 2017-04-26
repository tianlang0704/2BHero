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
        if (Loader.shared.GetSingleton<DelegateCenter>().OnGamePause != null) { Loader.shared.GetSingleton<DelegateCenter>().OnGamePause(); }
    }

    public void GameResume() {
        if (!this.isPaused) { return; }
        this.isPaused = false;
        if (Loader.shared.GetSingleton<DelegateCenter>().OnGameResume != null) { Loader.shared.GetSingleton<DelegateCenter>().OnGameResume(); }
    }

    public void GameRestart() {
        GameOver(false);
        Loader.shared.GetSingleton<DelegateCenter>().LoadGameScene();
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

        Loader.shared.GetSingleton<DelegateCenter>().StartDifficultLoop(() => {
            this.isGameStarted = true;
            Loader.shared.GetSingleton<DelegateCenter>().StartSpawningEnemy();
            if (Loader.shared.GetSingleton<DelegateCenter>().OnGameStart != null) { Loader.shared.GetSingleton<DelegateCenter>().OnGameStart(); }
        });
    }

    // Explicit overload for delegate
    public void GameOver() {
        GameOver(true);
    }

    public void GameOver(bool callOnGameover) {
        if (!this.isGameStarted) { return; }
        this.isGameStarted = false;
        Loader.shared.GetSingleton<DelegateCenter>().StopDifficultLoop();
        Loader.shared.GetSingleton<DelegateCenter>().StopSpawningEnemy();
        Loader.shared.GetSingleton<DelegateCenter>().ClearAllEnemies();
        if(Loader.shared.GetSingleton<DelegateCenter>().OnGameOver != null && callOnGameover) { Loader.shared.GetSingleton<DelegateCenter>().OnGameOver(); }
        
    }
// End: Game controls

// Mark: Game stats
    virtual protected void Score(int score) {
        this.score += score;
        if (Loader.shared.GetSingleton<DelegateCenter>().OnScoreChange != null) { Loader.shared.GetSingleton<DelegateCenter>().OnScoreChange(this.score); }
    }

    virtual protected void DeductLife(int amount) {
        this.life -= amount;
        if (Loader.shared.GetSingleton<DelegateCenter>().OnLifeChange != null) { Loader.shared.GetSingleton<DelegateCenter>().OnLifeChange(this.life); }
        if (this.life <= 0) {
            GameOver();
        }
    }

    public void ResetScore() {
        this.score = 0;
        if (Loader.shared.GetSingleton<DelegateCenter>().OnScoreChange != null) { Loader.shared.GetSingleton<DelegateCenter>().OnScoreChange(this.score); }
    }

    public void ResetLife() {
        this.life = this.defaultLife;
        if (Loader.shared.GetSingleton<DelegateCenter>().OnLifeChange != null) { Loader.shared.GetSingleton<DelegateCenter>().OnLifeChange(this.life); }
        
    }
// End: Game stats

// Mark: Game events
    virtual protected void OnGameStart() {

    }

    virtual protected void OnGameOver() {
        GameResume();
        Loader.shared.GetSingleton<DelegateCenter>().SetStatsBarActive(false);
        this.scoreMenuPrefab.ClonePrefabAndShow("Score", this.score, false, ()=> {
            Loader.shared.GetSingleton<DelegateCenter>().SetStatsBarActive(true);
        });
    }

    virtual protected void OnGamePause() {
        Loader.shared.GetSingleton<DelegateCenter>().BlurBGM();
    }

    virtual protected void OnGameResume() {
        Loader.shared.GetSingleton<DelegateCenter>().NormalizeBGM();
    }

    virtual protected void OnFirstUpdateAfterStart() {
        if (Loader.shared.GetSingleton<DelegateCenter>().OnScoreChange != null) { Loader.shared.GetSingleton<DelegateCenter>().OnScoreChange(this.score); }
        if (Loader.shared.GetSingleton<DelegateCenter>().OnLifeChange != null) { Loader.shared.GetSingleton<DelegateCenter>().OnLifeChange(this.life); }
    }
// End: Game events

    private void FixedUpdate() {
        if (this.isFirstFixedUpdateAfterStart) {
            this.isFirstFixedUpdateAfterStart = false;
            OnFirstUpdateAfterStart();
        }
    }

// Mark: Singleton initialization
    public override void InitializeDelegates() {
        base.InitializeDelegates();
        // Setup delegates
        DelegateCenter mc = Loader.shared.GetSingleton<DelegateCenter>();
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
