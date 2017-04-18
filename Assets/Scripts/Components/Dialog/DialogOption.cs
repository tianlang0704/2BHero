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
        DelegateCenter.shared.PlayUIOneShot(this.buttonSound);
        DelegateCenter.shared.SetBGMMuted(!b);
    }

    public void SFXHandler(bool b) {
        DelegateCenter.shared.PlayUIOneShot(this.buttonSound);
        DelegateCenter.shared.SetSFXMuted(!b);
    }

    public void CloseHandler() {
        DelegateCenter.shared.PlayUIOneShot(this.buttonSound);
        CloseMenu();
    }

    protected virtual void Awake() {
        this.musicToggle.isOn = !DelegateCenter.shared.IsBGMMuted();
        this.sfxToggle.isOn = !DelegateCenter.shared.IsSFXMuted();
    }
}
