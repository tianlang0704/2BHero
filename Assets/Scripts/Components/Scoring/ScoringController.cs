using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateCenter {
    public Action<int> OnScoreChange;
    public Action<int> OnLifeChange;
}


public class ScoringController : MonoInjectable {
    // Inspector public variables
    public int defaultScore = 0;
    public int defaultLife = 3;
    public DialogScore scoreMenuPrefab;

    // Non-Inpsector public variables
    [HideInInspector]
    public int score;
    [HideInInspector]
    public int life;

    // Dependencies
    [Inject]
    protected DelegateCenter delegateCenter;
    [Inject]
    protected GameController gameController;








    /// <summary>
    /// Method for stacking score
    /// </summary>
    /// <param name="score">score to increase</param>
    public virtual void Score(int score) {
        this.score += score;
        if (this.delegateCenter.OnScoreChange != null) { this.delegateCenter.OnScoreChange(this.score); }
    }
    /// <summary>
    /// Method for deducting life
    /// </summary>
    /// <param name="amount">Life amount to deduct</param>
    public virtual void DeductLife(int amount) {
        this.life -= amount;
        if (this.delegateCenter.OnLifeChange != null) { this.delegateCenter.OnLifeChange(this.life); }
        if (this.life <= 0) {
            this.gameController.GameOver();
        }
    }
    /// <summary>
    /// Method for reset score
    /// </summary>
    public void ResetScore() {
        this.score = 0;
        if (this.delegateCenter.OnScoreChange != null) { this.delegateCenter.OnScoreChange(this.score); }
    }
    /// <summary>
    /// Method for reset life
    /// </summary>
    public void ResetLife() {
        this.life = this.defaultLife;
        if (this.delegateCenter.OnLifeChange != null) { this.delegateCenter.OnLifeChange(this.life); }
    }
    /// <summary>
    /// Method for showing the score dialog
    /// </summary>
    public void ShowScore() {
        this.delegateCenter.SetStatsBarActive(false);
        this.scoreMenuPrefab.ClonePrefabAndShow("Score", this.score, false, () => {
            this.delegateCenter.SetStatsBarActive(true);
        });
    }
}
