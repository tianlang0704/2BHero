using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupDialogBase : MonoInjectable {
    private Action closeAction = null;

    public virtual void CloseMenu() {
        if (this.closeAction != null) { this.closeAction(); }
        Destroy(this.gameObject);
    }

    public virtual void ClonePrefabAndShow(Action closeAction = null) {
        Instantiate(this).Show(closeAction);
    }

    public virtual void Show(Action closeAction = null) {
        this.closeAction = closeAction;
        this.gameObject.SetActive(true);
    }
}
