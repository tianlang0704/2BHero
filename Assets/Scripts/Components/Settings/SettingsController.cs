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
    public static SettingsController shared = null;
    protected override void Awake() {
        base.Awake();
        if (SettingsController.shared == null) {
            SettingsController.shared = this;
        } else if (SettingsController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        if (this.isFirstRun) {
            InitSettings();
            this.isFirstRun = false;
        }
    }

    public override void InitializeDelegates() {
        base.InitializeDelegates();
        DelegateCenter dc = DelegateCenter.shared;
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
