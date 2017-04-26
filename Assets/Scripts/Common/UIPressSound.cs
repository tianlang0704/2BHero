using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIPressSound : BComponentBase {  
    public AudioClip pressSound;

    protected override void Start () {
        base.Start();
        this.GetComponent<Button>().onClick.AddListener(() => {
            base.GetDep<SoundController>().PlayUIOneShot(this.pressSound);
        }); ;
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
