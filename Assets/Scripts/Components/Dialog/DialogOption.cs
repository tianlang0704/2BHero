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
        Loader.shared.GetSingleton<DelegateCenter>().PlayUIOneShot(this.buttonSound);
        Loader.shared.GetSingleton<DelegateCenter>().SetBGMMuted(!b);
    }

    public void SFXHandler(bool b) {
        Loader.shared.GetSingleton<DelegateCenter>().PlayUIOneShot(this.buttonSound);
        Loader.shared.GetSingleton<DelegateCenter>().SetSFXMuted(!b);
    }

    public void CloseHandler() {
        Loader.shared.GetSingleton<DelegateCenter>().PlayUIOneShot(this.buttonSound);
        CloseMenu();
    }

    protected virtual void Awake() {
        this.musicToggle.isOn = !Loader.shared.GetSingleton<DelegateCenter>().IsBGMMuted();
        this.sfxToggle.isOn = !Loader.shared.GetSingleton<DelegateCenter>().IsSFXMuted();
    }
}
