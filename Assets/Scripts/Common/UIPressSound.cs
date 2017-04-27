using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIPressSound : MonoInjectable {  
    public AudioClip pressSound;

    // Dependencies
    [Inject]
    protected SoundController soundController;

    protected override void Start () {
        this.GetComponent<Button>().onClick.AddListener(() => {
            this.soundController.PlayUIOneShot(this.pressSound);
        }); ;
	}
}
