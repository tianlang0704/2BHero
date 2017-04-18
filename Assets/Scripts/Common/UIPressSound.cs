using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIPressSound : MonoBehaviour {  
    public AudioClip pressSound;

    void Start () {
        this.GetComponent<Button>().onClick.AddListener(() => {
            DelegateCenter.shared.PlayUIOneShot(this.pressSound);
        }); ;
	}
}
