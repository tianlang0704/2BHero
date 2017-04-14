using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LifeCycleDelegates))]
public class StatsMenu : MonoBehaviour {
    public OptionMenu optionMenuPrefab;
    public Text scoreText;
    public Text lifeText;
    public Text bulletText;

    public void HandlePause() {
        DelegateCenter.shared.GamePauseResume();
    }

    public void HandleOption() {
        DelegateCenter.shared.GamePause();
        this.optionMenuPrefab.ClonePrefabAndShow(() => {
            DelegateCenter.shared.GameResume();
        });
    }

    public void HandleRestart() {
        DelegateCenter.shared.GameRestart();
    }

    public void HandleStop() {
        DelegateCenter.shared.GameOver();
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

    private void Start() {
        DelegateCenter dc = DelegateCenter.shared;
        dc.OnScoreChange += OnScoreChange;
        dc.OnLifeChange += OnLifeChange;
        dc.OnBulletCountChange += OnBulletCountChange;
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            dc.OnScoreChange -= OnScoreChange;
            dc.OnLifeChange -= OnLifeChange;
            dc.OnBulletCountChange -= OnBulletCountChange;
        });
    }
}
