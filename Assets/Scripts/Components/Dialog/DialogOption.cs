using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogOption : PopupDialogBase {
    public Toggle musicToggle;
    public Toggle sfxToggle;
    public AudioClip buttonSound;

    public void BGMHandler(bool b) {
        base.GetDep<SoundController>().PlayUIOneShot(this.buttonSound);
        base.GetDep<SoundController>().isBGMMuted = !b;
    }

    public void SFXHandler(bool b) {
        base.GetDep<SoundController>().PlayUIOneShot(this.buttonSound);
        base.GetDep<SoundController>().isSFXMuted = !b;
    }

    public void CloseHandler() {
        base.GetDep<SoundController>().PlayUIOneShot(this.buttonSound);
        CloseMenu();
    }

    protected override void Start() {
        base.Start();
        this.musicToggle.isOn = !base.GetDep<SoundController>().isBGMMuted;
        this.sfxToggle.isOn = !base.GetDep<SoundController>().isSFXMuted;
    }

    protected override void Awake() {
        base.Awake();
        GameObject.FindObjectOfType<Loader>().DynamicInjection(this);
    }

    public void InjectDependencies(
        SoundController sc
    ) {
        base.ClearDependencies();
        base.AddDep<SoundController>(sc);
        base.isInjected = true;
    }
}
