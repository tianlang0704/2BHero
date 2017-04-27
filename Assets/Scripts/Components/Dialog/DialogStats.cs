using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class DelegateCenter {
    public Action<bool> SetStatsBarActive; 
}

[RequireComponent(typeof(LifeCycleDelegates))]
public class DialogStats : MonoInjectable {
    public DialogOption optionMenuPrefab;
    public DialogPause pauseMenuPrefab;
    public Text scoreText;
    public Text lifeText;
    public Text bulletText;

    private DialogOption optionMenu = null;
    private DialogPause pauseMenu = null;

    [Inject]
    protected DelegateCenter delegateCenter;
    [Inject]
    protected GameController gameController;

    public void HandlePause() {
        if(this.pauseMenu != null) { this.pauseMenu.CloseMenu(); return; }
        this.gameController.GamePause();
        this.pauseMenu = Instantiate(this.pauseMenuPrefab);
        this.pauseMenu.Show(() => {
            this.gameController.GameResume();
            this.pauseMenu = null;
        });
    }

    public void HandleOption() {
        if (this.optionMenu != null) { return; }
        if (this.pauseMenu == null) { HandlePause(); } else { this.pauseMenu.CancelCountdown(); }
        this.optionMenu = Instantiate(this.optionMenuPrefab);
        this.optionMenu.Show(() => {
            HandlePause();
            this.optionMenu = null;
        });
    }

    public void HandleRestart() {
        this.gameController.GameRestart();
    }

    public void HandleStop() {
        this.gameController.GameOver();
    }

    private void OnScoreChange(int score) {
        this.scoreText.text = score.ToString();
    }

    private void OnLifeChange(int life) {
        this.lifeText.text = life.ToString();
    }

    private void OnBulletCountChange(Shooter s) {
        this.bulletText.text = s.bulletCount.ToString();
    }

    protected override void Start() {
        base.Start();
        DelegateCenter dc = this.delegateCenter;
        dc.OnScoreChange += OnScoreChange;
        dc.OnLifeChange += OnLifeChange;
        dc.OnBulletCountChange += OnBulletCountChange;
        Action<bool> SetStatsBarActive = (active) => { this.gameObject.SetActive(active); };
        dc.SetStatsBarActive += SetStatsBarActive;
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            dc.OnScoreChange -= OnScoreChange;
            dc.OnLifeChange -= OnLifeChange;
            dc.OnBulletCountChange -= OnBulletCountChange;
            dc.SetStatsBarActive -= SetStatsBarActive;
        });
    }
}
