using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[Serializable]
public class BGMSettings {
    public string settingName;
    public float volume = 1f;
    [SerializeField] private List<AudioClip> _settingMusics = new RandomList<AudioClip>();
    public RandomList<AudioClip> settingMusics { get { return (RandomList<AudioClip>)_settingMusics; } }
}

[RequireComponent(typeof(AudioSource))]
public class SoundController : BComponentBase {
    public string masterVolumeParam = "MainVolume";
    public string bgmVolumeParam = "MusicVolume";
    public string sfxVolumeParam = "SFXVolume";
    public AudioMixer mainMixer;
    public AudioMixerSnapshot bluredBGM;
    public AudioMixerSnapshot normalBGM;
    public AudioSource bgmSource;
    public AudioSource uiSource;
    public float soundTransitionTime = 1f;
    public bool isMasterMuted { get { return GetMasterVolume() == -80; } set { SetMasterVolume(value ? -80 : 0); } }
    public bool isBGMMuted { get { return GetBGMVolume() == -80; } set { SetBGMVolume(value ? -80 : 0); } }
    public bool isSFXMuted { get { return GetSFXVolume() == -80; } set { SetSFXVolume(value ? -80 : 0); } }
    public float GetMasterVolume() { float f; this.mainMixer.GetFloat(this.masterVolumeParam, out f); return f; }
    public float GetBGMVolume() { float f; this.mainMixer.GetFloat(this.bgmVolumeParam, out f); return f; }
    public float GetSFXVolume() { float f; this.mainMixer.GetFloat(this.sfxVolumeParam, out f); return f; }
    public void SetMasterVolume(float level) { this.mainMixer.SetFloat(this.masterVolumeParam, level); }
    public void SetBGMVolume(float level) { this.mainMixer.SetFloat(this.bgmVolumeParam, level); }
    public void SetSFXVolume(float level) { this.mainMixer.SetFloat(this.sfxVolumeParam, level); }

    public void BlurBGM() {
        this.bluredBGM.TransitionTo(this.soundTransitionTime);
    }

    public void NormalizeBGM() {
        this.normalBGM.TransitionTo(this.soundTransitionTime);
    }

// Mark: BGM functions
    public List<BGMSettings> bgms = new List<BGMSettings>();

    private bool bgmEnabled = true;
    private string currentSettingName = null;

    public void PlayBGMForSetting(string settingName) {
        this.bgmEnabled = true;
        this.bgms.ForEach((setting) => {
            if(setting.settingName == settingName) {
                this.currentSettingName = settingName;
                this.bgmSource.PlayOneShot(setting.settingMusics.GetRandom(), setting.volume);
            }
        });
    }

    public void StopBGM() {
        if (!this.bgmEnabled && !this.bgmSource.isPlaying) { return; }
        this.bgmEnabled = false;
        this.currentSettingName = null;
        this.bgmSource.Stop();
    }

    public void PlayBGMContinuously() {
        if (!this.bgmEnabled || this.currentSettingName == null || this.bgmSource.isPlaying) { return; }
        PlayBGMForSetting(this.currentSettingName);
    }

    public void PlayUIOneShot(AudioClip clip) {
        this.uiSource.PlayOneShot(clip);
    }

// End: BGM functions

    protected virtual void Update() {
        PlayBGMContinuously();
    }

// Mark: Singleton initialization
    override protected void Awake() {
        base.Awake();
        this.mainMixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
    }
// End: Singleton initialization
}
