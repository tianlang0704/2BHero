using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogOption : PopupDialogBase {
    public Toggle musicToggle;
    public Toggle sfxToggle;
    public AudioClip buttonSound;

    [Inject]
    protected SoundController soundController;

    /// <summary>
    /// Handle BGM toggle press
    /// </summary>
    /// <param name="b">true for enable, false for disable</param>
    public void BGMHandler(bool b) {
        this.soundController.PlayUIOneShot(this.buttonSound);
        this.soundController.isBGMMuted = !b;
    }

    /// <summary>
    /// Handle SFX toggle press
    /// </summary>
    /// <param name="b">true for enable, false for disable</param>
    public void SFXHandler(bool b) {
        this.soundController.PlayUIOneShot(this.buttonSound);
        this.soundController.isSFXMuted = !b;
    }

    /// <summary>
    /// Handle close button
    /// </summary>
    public void CloseHandler() {
        this.soundController.PlayUIOneShot(this.buttonSound);
        CloseMenu();
    }

    /// <summary>
    /// Read sound setting when Awake;
    /// </summary>
    protected override void Awake() {
        base.Awake();
        this.musicToggle.isOn = !this.soundController.isBGMMuted;
        this.sfxToggle.isOn = !this.soundController.isSFXMuted;
    }
}
