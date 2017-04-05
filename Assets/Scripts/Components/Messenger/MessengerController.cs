using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MessengerController : MonoBehaviour {

// Mark: Singleton initialization
    public static MessengerController shared = null;
    private void Awake() {
        if (MessengerController.shared == null) {
            MessengerController.shared = this;
        } else if (MessengerController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }
// End: Singleton initialization
}
