using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : BComponentBase {
    public List<string> positionMarkNames = new List<string>() { "L1", "L2", "L3" };
    public string playerMarkName = "Player";
    public Player playerPrefab = null;
    public int defaultPosition = 0;

    private int maxPosition = 0;
    private int currentPosition = 0;
    private List<GameObject> positionMarks = new List<GameObject>();
    private Player player;

	private void Update () {
        InputController.shared.PlayerMoveUp(()=> {
            if (base.GetDep<GameController>().isPaused) { return; }
            if (this.currentPosition < this.maxPosition) {
                this.currentPosition += 1;
                this.player.Move(this.positionMarks[this.currentPosition]);
            }
        });

        InputController.shared.PlayerMoveDown(() => {
            if (base.GetDep<GameController>().isPaused) { return; }
            if (this.currentPosition > 0) {
                this.currentPosition -= 1;
                this.player.Move(this.positionMarks[this.currentPosition]);
            }
        });
    }

// Mark: Scene initialization
    public void SetupPlayerControllerForScene(bool parseMark = false) {
        ParsePositionsMarks();
        if (parseMark) {
            GeneratePlayerFromMark();
        } else {
            GeneratePlayerForPosition(this.defaultPosition);
        }
    }

    private void ParsePositionsMarks() {
        this.positionMarks.Clear();
        foreach (string s in this.positionMarkNames) {
            GameObject go = GameObject.Find(s);
            if (!go) { Debug.Log("No mark object found for position: " + s); continue; }
            this.positionMarks.Add(go);
        }
        this.maxPosition = this.positionMarks.Count - 1;
    }

    private void GeneratePlayerForPosition(int posIndex) {
        if (this.defaultPosition >= this.positionMarks.Count) { return; }
        GameObject go = this.positionMarks[this.defaultPosition];
        this.player = Instantiate(
            this.playerPrefab,
            go.transform.position,
            go.transform.rotation,
            go.transform.parent);
        this.currentPosition = posIndex;
    }

    private void GeneratePlayerFromMark() {
        // Clear old shooters before parsing the mark
        Destroy(this.player);
        this.player = null;
        // Go through all GameObjects, find, parse, and add to the shooters list
        GameObject go = GameObject.Find(this.playerMarkName);
        if (!go) { Debug.Log("No mark found for player"); }
        this.player = Instantiate(
            this.playerPrefab,
            go.transform.position,
            go.transform.rotation,
            go.transform.parent);
        if(this.defaultPosition < this.positionMarks.Count) {
            this.player.Move(this.positionMarks[this.defaultPosition]);
        }
    }
// End: Scene initialization

// Mark: initialization
    override protected void Awake() {
        base.Awake();
        this.currentPosition = this.defaultPosition;
    }

    public void InjectDependencies(
        GameController gc
    ) {
        base.ClearDependencies();
        base.AddDep<GameController>(gc);
        base.isInjected = true;
    }
// End: initialization
}
