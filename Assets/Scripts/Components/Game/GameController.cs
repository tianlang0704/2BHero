using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateCenter {
    public Action<int> OnScoreChange;
    public Action<int> OnLifeChange;
    public Action<int> OnDifficultyIncrement;
    public Action OnGameOver;
    public Action OnGameStart;
    public Action OnGamePause;
    public Action OnGameResume;
}

public class GameController : BComponentBase {
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
        if (base.GetDep<DelegateCenter>().OnGamePause != null) { base.GetDep<DelegateCenter>().OnGamePause(); }
    }

    public void GameResume() {
        if (!this.isPaused) { return; }
        this.isPaused = false;
        if (base.GetDep<DelegateCenter>().OnGameResume != null) { base.GetDep<DelegateCenter>().OnGameResume(); }
    }

    public void GameRestart() {
        GameOver(false);
        base.GetDep<SceneController>().LoadGameScene();
        GameStart();
    }

    public void GameStartInitSafe() {
        if (!this.isInitDone) {
            this.lifeCycle.OnceOnFirstFixedUpdate(() => {
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

        base.GetDep<DifficultyContoroller>().StartDifficultLoop(() => {
            this.isGameStarted = true;
            base.GetDep<EnemyController>().StartSpawning();
            if (base.GetDep<DelegateCenter>().OnGameStart != null) { base.GetDep<DelegateCenter>().OnGameStart(); }
        });
    }

    // Explicit overload for delegate
    public void GameOver() {
        GameOver(true);
    }

    public void GameOver(bool callOnGameover) {
        if (!this.isGameStarted) { return; }
        this.isGameStarted = false;
        base.GetDep<DifficultyContoroller>().StopDifficultLoop();
        base.GetDep<EnemyController>().StopSpawning();
        base.GetDep<EnemyController>().ClearAllEnemies();
        if(base.GetDep<DelegateCenter>().OnGameOver != null && callOnGameover) { base.GetDep<DelegateCenter>().OnGameOver(); }
        
    }
// End: Game controls

// Mark: Game stats
    public virtual void Score(int score) {
        this.score += score;
        if (base.GetDep<DelegateCenter>().OnScoreChange != null) { base.GetDep<DelegateCenter>().OnScoreChange(this.score); }
    }

    public virtual void DeductLife(int amount) {
        this.life -= amount;
        if (base.GetDep<DelegateCenter>().OnLifeChange != null) { base.GetDep<DelegateCenter>().OnLifeChange(this.life); }
        if (this.life <= 0) {
            GameOver();
        }
    }

    public void ResetScore() {
        this.score = 0;
        if (base.GetDep<DelegateCenter>().OnScoreChange != null) { base.GetDep<DelegateCenter>().OnScoreChange(this.score); }
    }

    public void ResetLife() {
        this.life = this.defaultLife;
        if (base.GetDep<DelegateCenter>().OnLifeChange != null) { base.GetDep<DelegateCenter>().OnLifeChange(this.life); }
        
    }
// End: Game stats

// Mark: Game events
    virtual protected void OnGameStart() {

    }

    virtual protected void OnGameOver() {
        GameResume();
        base.GetDep<DelegateCenter>().SetStatsBarActive(false);
        this.scoreMenuPrefab.ClonePrefabAndShow("Score", this.score, false, ()=> {
            base.GetDep<DelegateCenter>().SetStatsBarActive(true);
        });
    }

    virtual protected void OnGamePause() {
        base.GetDep<SoundController>().BlurBGM();
    }

    virtual protected void OnGameResume() {
        base.GetDep<SoundController>().NormalizeBGM();
    }

    virtual protected void OnFirstUpdateAfterStart() {
        if (base.GetDep<DelegateCenter>().OnScoreChange != null) { base.GetDep<DelegateCenter>().OnScoreChange(this.score); }
        if (base.GetDep<DelegateCenter>().OnLifeChange != null) { base.GetDep<DelegateCenter>().OnLifeChange(this.life); }
    }
// End: Game events

    private void FixedUpdate() {
        if (this.isFirstFixedUpdateAfterStart) {
            this.isFirstFixedUpdateAfterStart = false;
            OnFirstUpdateAfterStart();
        }
    }

// Mark: Singleton initialization
    protected override void InitializeDelegates() {
        base.InitializeDelegates();
        // Setup delegates
        DelegateCenter mc = base.GetDep<DelegateCenter>();
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        mc.OnGameOver += OnGameOver;
        mc.OnGameStart += OnGameStart;
        mc.OnGamePause += OnGamePause;
        mc.OnGameResume += OnGameResume;
        lc.OnceOnDestroy(() => {
            mc.OnGameOver -= OnGameOver;
            mc.OnGameStart -= OnGameStart;
            mc.OnGamePause -= OnGamePause;
            mc.OnGameResume -= OnGameResume;
        });
        
        lc.OnceOnFirstFixedUpdate(() => {
            this.isInitDone = true;
        });
    }

    public void InjectDependencies(
        DifficultyContoroller dc,
        EnemyController ec,
        SceneController sc,
        SoundController soundC,
        DelegateCenter delegateC
    ) {
        base.ClearDependencies();
        base.AddDep<DifficultyContoroller>(dc);
        base.AddDep<EnemyController>(ec);
        base.AddDep<SceneController>(sc);
        base.AddDep<SoundController>(soundC);
        base.AddDep<DelegateCenter>(delegateC);
        base.isInjected = true;
    }
// End: Singleton initialization
}
