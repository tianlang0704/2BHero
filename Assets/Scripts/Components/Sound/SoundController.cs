using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public partial class DelegateCenter {
    public Action<float> SetMasterVolume;
    public Action<float> SetBGMVolume;
    public Action<float> SetSFXVolume;
}

public class SoundController : ControllerBase {
    public AudioMixer mainMixer;

    public void SetMasterVolume(float level) {
        this.mainMixer.SetFloat("MainVolume", level);
    }

    public void SetBGMVolume(float level) {
        this.mainMixer.SetFloat("MusicVolume", level);
    }

    public void SetSFXVolume(float level) {
        this.mainMixer.SetFloat("SFXVolume", level);
    }

// Mark: Singleton initialization
    public static SoundController shared = null;
    override protected void Awake() {
        base.Awake();
        if (SoundController.shared == null) {
            SoundController.shared = this;
        } else if (SoundController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    protected override void InitializeDelegates() {
        base.InitializeDelegates();
        DelegateCenter dc = DelegateCenter.shared;
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        dc.SetMasterVolume += SetMasterVolume;
        dc.SetBGMVolume += SetBGMVolume;
        dc.SetSFXVolume += SetSFXVolume;
        lc.OnceOnDestroy(() => {
            dc.SetMasterVolume -= SetMasterVolume;
            dc.SetBGMVolume -= SetBGMVolume;
            dc.SetSFXVolume -= SetSFXVolume;
        });
    }
// End: Singleton initialization
}
