using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DelegateCenter {
    public Func<bool> GetEnableTutorial;
    public Action<bool> SetEnableTutorial;
}

public class SettingsController : ControllerBase {
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

// Mark: Singleton initialization
    protected override void Awake() {
        base.Awake();
        if (this.isFirstRun) {
            InitSettings();
            this.isFirstRun = false;
        }
    }

    public override void InitializeDelegates() {
        base.InitializeDelegates();
        DelegateCenter dc = Loader.shared.GetSingleton<DelegateCenter>();
        Func<bool> GetEnableTutorial = () => { return this.isTutorialEnabled; };
        Action<bool> SetEnableTutorial = (v) => { this.isTutorialEnabled = v; };
        dc.GetEnableTutorial += GetEnableTutorial;
        dc.SetEnableTutorial += SetEnableTutorial;
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            dc.GetEnableTutorial -= GetEnableTutorial;
            dc.SetEnableTutorial -= SetEnableTutorial;
        });
    }
    // End: Singleton initialization
}
