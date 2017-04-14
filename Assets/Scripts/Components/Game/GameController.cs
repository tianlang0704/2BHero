﻿using System;
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
    public Action GamePauseResume;
    public Action GamePause;
    public Action GameResume;
    public Action GameRestart;
    public Action GameOver;
}

public class GameController : ControllerBase {
    public int defaultScore = 0;
    public int defaultLife = 3;
    public ScoreMenu scoreMenuPrefab;

    private int score;
    private int life;
    private bool isGameStarted = false;
    private bool isInitDone = false;
    private bool isPaused = false;
    private bool isFirstFixedUpdateAfterStart = true;

// Mark: Game controls
    public void GamePauseResume() {
        if (this.isPaused) {
            GameResume();
        }else {
            GamePause();
        }
    }

    public void GamePause() {
        if (this.isPaused) { return; }
        this.isPaused = true;
        Time.timeScale = 0;
        if (DelegateCenter.shared.OnGamePause != null) { DelegateCenter.shared.OnGamePause(); }
    }

    public void GameResume() {
        if (!this.isPaused) { return; }
        this.isPaused = false;
        Time.timeScale = 1;
        if (DelegateCenter.shared.OnGameResume != null) { DelegateCenter.shared.OnGameResume(); }
    }

    public void GameRestart() {
        GameOver(false);
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
        this.scoreMenuPrefab.ClonePrefabAndShow("Score", this.score);
    }

    virtual protected void OnGamePause() {

    }

    virtual protected void OnGameResume() {

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

    protected override void InitializeDelegates() {
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
        });
        
        lc.OnceOnFirstFixedUpdate(() => {
            this.isInitDone = true;
        });
    }
// End: Singleton initialization
}
