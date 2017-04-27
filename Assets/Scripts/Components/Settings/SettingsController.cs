using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoInjectable {
    private int boolTrue = 1;
    private int boolFalse = 8;
    private string isTutorialEnabledKey = "EnableTutorial";
    public bool isTutorialEnabled {
        get { return PlayerPrefs.GetInt(this.isTutorialEnabledKey) != this.boolFalse; }
        set { PlayerPrefs.SetInt(this.isTutorialEnabledKey, value ? this.boolTrue : this.boolFalse); }
    }
    private string isFirstRunKey = "IsFirstRun";
    public bool isFirstRun {
        get { return PlayerPrefs.GetInt(this.isFirstRunKey) != this.boolFalse; }
        set { PlayerPrefs.SetInt(this.isFirstRunKey, value ? this.boolTrue : this.boolFalse); }
    }

    public void Save() {
        
    }

    public void Load() {
        
    }

    public void InitSettings() {
        PlayerPrefs.DeleteAll();
        this.isTutorialEnabled = true;
    }

    protected override void Awake() {
        base.Awake();
        if (this.isFirstRun) {
            InitSettings();
            this.isFirstRun = false;
        }
    }
}
