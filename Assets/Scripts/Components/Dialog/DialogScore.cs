﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class DialogScore : PopupDialogBase {
    [Header("Sound Settings")]
    public AudioClip normalSound;
    public AudioClip highScoreSound;
    public float soundVolume = 0.4f;
    [Header("UI Settings")]
    public Text titleText;
    public Text scoreText;
    public Button againBtn;
    public Button menuBtn;
    [Header("Animation Settings")]
    public float scoreGrowthInterval = 0.03f;
    public float scoreGrowthTime = 0.5f;

    private int score;

    public void ClonePrefabAndShow(string title, int score, bool isHighScore = false, Action closeAction = null) {
        DialogScore s = Instantiate(this);
        s.score = score;
        s.titleText.text = title;
        s.Show(isHighScore, closeAction);
    }

    public virtual void Show(bool isHighScore = false, Action closeAction = null) {
        base.GetDep<SoundController>().BlurBGM();
        if (isHighScore) {
            this.GetComponent<AudioSource>().PlayOneShot(this.highScoreSound, this.soundVolume);
        }else {
            this.GetComponent<AudioSource>().PlayOneShot(this.normalSound, this.soundVolume);
        }
        this.Show(closeAction);
        StartCoroutine(ScoreGrowthRoutine());
    }

    public override void CloseMenu() {
        base.CloseMenu();
        base.GetDep<SoundController>().NormalizeBGM();
    }

    public void HandleMenu() {
        CloseMenu();
        base.GetDep<SceneController>().LoadMenuScene();
    }

    public void HandlePlay() {
        CloseMenu();
        base.GetDep<GameController>().GameRestart();
    }


    private IEnumerator ScoreGrowthRoutine() {
        float s = 0;
        float scoreGrowthRate = this.score / (this.scoreGrowthTime / this.scoreGrowthInterval);

        this.scoreText.text = Math.Ceiling(s).ToString();
        while (s < this.score) {
            yield return new WaitForSecondsRealtime(this.scoreGrowthInterval);
            s = s + scoreGrowthRate > this.score ? this.score : s + scoreGrowthRate;
            this.scoreText.text = Math.Ceiling(s).ToString();
        }
    }

// Mark: Initializeation
    protected override void Awake() {
        base.Awake();
        GameObject.FindObjectOfType<Loader>().DynamicInjection(this);
    }

    public void InjectDependencies(
        GameController gc,
        SoundController sc
    ) {
        base.ClearDependencies();
        base.AddDep<GameController>(gc);
        base.AddDep<SoundController>(sc);
        base.isInjected = true;
    }
// End: Initializeation
}
