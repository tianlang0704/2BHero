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
    [Inject]
    protected EffectController effectController;










// Mark: Public socring methods
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
    /// Method for adding life
    /// </summary>
    /// <param name="amount">Life amount to add</param>
    public virtual void AddLife(int amount) {
        this.life += amount;
        if (this.delegateCenter.OnLifeChange != null) { this.delegateCenter.OnLifeChange(this.life); }
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
// End: Public socring methods










// Mark: Goaling events
    /// <summary>
    /// Method called when normal goal is reached by enemy
    /// </summary>
    /// <param name="e">The enemy reached the goal</param>
    protected virtual void OnNormalGoal(Enemy e) {
        this.effectController.ShowScoreAt(e.transform.position, -1, Color.red);
        this.DeductLife(1);
        e.Recycle();
    }
    /// <summary>
    /// Method called when a correct enemy reached the rps goal
    /// </summary>
    /// <param name="e">The enemy reached the goal</param>
    protected virtual void OnRPSGoalCorrect(Enemy e) {
        this.effectController.ShowScoreAt(e.transform.position + (new Vector3(-1f, 0)), 500, Color.white);
        this.effectController.ShowScoreAt(e.transform.position + (new Vector3(-1f, 0.7f)), 1, Color.red);
        this.AddLife(1);
        this.Score(500);
        e.Recycle();
    }
    /// <summary>
    /// Method called when a incorrect enemy reached the rps goal
    /// </summary>
    /// <param name="e">The enemy reached the goal</param>
    protected virtual void OnRPSGoalIncorrect(Enemy e) {
        this.effectController.ShowScoreAt(e.transform.position, -1, Color.red);
        this.DeductLife(1);
        e.Recycle();
    }
// End: Goaling events

    protected override void Start() {
        this.delegateCenter.OnNormalGoal += OnNormalGoal;
        this.delegateCenter.OnRPSGoalCorrect += OnRPSGoalCorrect;
        this.delegateCenter.OnRPSGoalIncorrect += OnRPSGoalIncorrect;
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            this.delegateCenter.OnNormalGoal -= OnNormalGoal;
            this.delegateCenter.OnRPSGoalCorrect -= OnRPSGoalCorrect;
            this.delegateCenter.OnRPSGoalIncorrect -= OnRPSGoalIncorrect;
        });
    }
}
