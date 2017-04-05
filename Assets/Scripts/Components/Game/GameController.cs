using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MessengerController {
    public Action<int> OnScore;
    public Action<int> OnDeductLife;
    public Action<int> OnDifficultyIncrement;
    public Action OnGameOver;
    public Action OnGameStart;
}

public class GameController : ControllerBase {
    public int defaultScore = 0;
    public int defaultLife = 3;

    private int score;
    private int life;
    private bool isGameStarted = false;

    public void GameStart() {
        if (this.isGameStarted) { return; }
        ResetLife();
        ResetScore();
        MessengerController.shared.StartDifficultLoop(() => {
            this.isGameStarted = true;
            MessengerController.shared.StartSpawningEnemy();
            MessengerController.shared.OnGameStart();
        });
    }

    public void GameOver() {
        if (!this.isGameStarted) { return; }
        this.isGameStarted = false;
        MessengerController.shared.StopDifficultLoop();
        MessengerController.shared.StopSpawningEnemy();
        MessengerController.shared.ClearAllEnemies();
        MessengerController.shared.OnGameOver();
    }

    public void ResetLife() {
        this.life = this.defaultLife;
    }

    public void ResetScore() {
        this.score = 0;
    }

// Mark: Game events
    virtual protected void OnGameStart() {

    }

    virtual protected void OnGameOver() {
        GameStart();
    }

    virtual protected void OnScore(int score) {
        this.score += score;
        Debug.Log("Score: " + this.score);
    }

    virtual protected void OnDeductLife(int amount) {
        this.life -= amount;
        if(this.life <= 0) {
            GameOver();
        }
        Debug.Log("Life: " + this.life);
    }
// End: Game events

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

    override protected void Start() {
        base.Start();
    }

    protected override void InitializeDelegates() {
        base.InitializeDelegates();
        // Setup listeners
        MessengerController mc = MessengerController.shared;
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        mc.OnScore += OnScore;
        lc.OnceOnDestroy(() => { mc.OnScore -= OnScore; });
        mc.OnDeductLife += OnDeductLife;
        lc.OnceOnDestroy(() => { mc.OnDeductLife -= OnDeductLife; });
        mc.OnGameOver += OnGameOver;
        lc.OnceOnDestroy(() => { mc.OnGameOver -= OnGameOver; });
        mc.OnGameStart += OnGameStart;
        lc.OnceOnDestroy(() => { mc.OnGameStart -= OnGameStart; });
        lc.OnceOnFirstFixedUpdate(() => {
            // TODO: Remove when menu is made
            GameStart();
        });
    }
// End: Singleton initialization
}
