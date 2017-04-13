using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionMenu : PopupMenuBase {
    public bool ToggleMusic { set { this.isMusicOn = value; Debug.Log(value); } get { return this.isMusicOn; } }
    private bool isMusicOn = true;
    
    public bool ToggleSFX { set { this.isSFXOn = value; } get { return this.isSFXOn; } }
    private bool isSFXOn = true;
}
