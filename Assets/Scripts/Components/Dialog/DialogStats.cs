using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class DelegateCenter {
    public Action<bool> SetStatsBarActive; 
}

[RequireComponent(typeof(LifeCycleDelegates))]
public class DialogStats : BComponentBase {
    public DialogOption optionMenuPrefab;
    public DialogPause pauseMenuPrefab;
    public Text scoreText;
    public Text lifeText;
    public Text bulletText;

    private DialogOption optionMenu = null;
    private DialogPause pauseMenu = null;

    public void HandlePause() {
        if(this.pauseMenu != null) { this.pauseMenu.CloseMenu(); return; }
        base.GetDep<GameController>().GamePause();
        this.pauseMenu = Instantiate(this.pauseMenuPrefab);
        this.pauseMenu.Show(() => {
            base.GetDep<GameController>().GameResume();
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
        base.GetDep<GameController>().GameRestart();
    }

    public void HandleStop() {
        base.GetDep<GameController>().GameOver();
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
        DelegateCenter dc = base.GetDep<DelegateCenter>();
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

    protected override void Awake() {
        base.Awake();
        GameObject.FindObjectOfType<Loader>().DynamicInjection(this);
    }

    public void InjectDependencies(
        SoundController sc,
        GameController gc,
        DelegateCenter dc
    ) {
        base.ClearDependencies();
        base.AddDep<SoundController>(sc);
        base.AddDep<GameController>(gc);
        base.AddDep<DelegateCenter>(dc);
        base.isInjected = true;
    }
}
