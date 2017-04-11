using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionMenu : MonoBehaviour {
    public bool ToggleMusic { set { this.isMusicOn = value; Debug.Log(value); } get { return this.isMusicOn; } }
    private bool isMusicOn = true;
    
    public bool ToggleSFX { set { this.isSFXOn = value; } get { return this.isSFXOn; } }
    private bool isSFXOn = true;

    private Action closeAction;

    public void CloseMenu() {
        this.closeAction();
        Destroy(this.gameObject);
    }

    public void ShowMenu(Action closeAction) {
        this.closeAction = closeAction;
        this.gameObject.SetActive(true);
    }
}
