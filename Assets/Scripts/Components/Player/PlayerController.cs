using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public List<string> positionMarkNames = new List<string>() { "L1", "L2", "L3" };
    public int currentPosition = 0;

    private int maxPosition = 0;
    private List<GameObject> positionMarks = new List<GameObject>();
    private Player player;

	private void Update () {
        InputController.shared.PlayerMoveUp(()=> {
            if (this.currentPosition < this.maxPosition) {
                this.currentPosition += 1;
                this.player.Move(this.positionMarks[this.currentPosition]);
            }
        });

        InputController.shared.PlayerMoveDown(() => {
            if (this.currentPosition > 0) {
                this.currentPosition -= 1;
                this.player.Move(this.positionMarks[this.currentPosition]);
            }
        });
    }

// Mark: Singleton initialization
    public static PlayerController shared = null;
    private void Awake() {
        if (PlayerController.shared == null) {
            PlayerController.shared = this;
        }else if (PlayerController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start() {
        this.maxPosition = this.positionMarkNames.Count - 1;
        this.player = GameObject.FindObjectOfType<Player>();
        InitializePositionsMarks();
    }

    private void InitializePositionsMarks() {
        foreach(string s in this.positionMarkNames) {
            GameObject go = GameObject.Find(s);
            if(!go) { Debug.Log("No mark object found for position: " + s); }
            this.positionMarks.Add(go);
        }
    }
// End: Singleton initialization
}
