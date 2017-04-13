﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class DelegateCenter {
    public Action<float> OnShootHoldUpdate;
    public Action OnShootEnd;
    public Action OnShootStart;
}


// TODO: Change this into weapon controller and move shooting control to player controller
public class ShootController : ControllerBase {
    public float maxPressTime = 0.35f;
    public List<Shooter> shooterPrefabs = new List<Shooter>();
    public int defaultPrefabIndex = 0;
    public string shooterMarkName = "ShooterMark";

    private List<Shooter> shooters = new List<Shooter>();
    private float shootTimeFactor;
    private int prefabIndex;

    private void Update() {
        InputController.shared.VariableDurationShoot((timePassed)=> {
            // Shoot press end handler
            if (timePassed > this.maxPressTime) {
                timePassed = this.maxPressTime;
                if (DelegateCenter.shared.OnShootHoldUpdate != null) {
                    DelegateCenter.shared.OnShootHoldUpdate(timePassed * this.shootTimeFactor);
                }
            }
            this.shooters.ForEach((Shooter s) => {
                s.Shoot(timePassed * this.shootTimeFactor);
                s.AimStop();
            });
            if (DelegateCenter.shared.OnShootEnd != null) { DelegateCenter.shared.OnShootEnd(); }
        }, (timePassed)=> {
            // Shoot press in progress handler
            if (timePassed > this.maxPressTime) {
                timePassed = this.maxPressTime;
            }
            if (DelegateCenter.shared.OnShootHoldUpdate != null) {
                DelegateCenter.shared.OnShootHoldUpdate(timePassed * this.shootTimeFactor);
            }
        }, () => {
            // Shoot press start handler
            this.shooters.ForEach((Shooter s) => { s.Aim(); });
            if (DelegateCenter.shared.OnShootStart != null) { DelegateCenter.shared.OnShootStart(); }
        });
    }

    public void ChangeShooter(int idx) {
        // Update shooter index
        this.prefabIndex = idx;
        // Go through the current shooter list and replace them with the newly instantiated prefabs.
        // The old ones will be destroyed.
        for (int i = 0; i < this.shooters.Count; ++i) {
            Shooter shooterToReplace = this.shooters[i];
            Shooter newShooter = Instantiate(
                this.shooterPrefabs[idx],
                shooterToReplace.transform.position,
                shooterToReplace.transform.rotation,
                shooterToReplace.transform.parent);
            this.shooters[i] = newShooter;
            Destroy(shooterToReplace);
        }
    }

// Mark: Scene initialization
    public void SetupShootControllerForScene() {
        GenerateShootersFromMarks();
    }

    public void GenerateShootersFromMarks() {
        // Clear old shooters before parsing the mark
        this.shooters.ForEach((Shooter s) => { Destroy(s); });
        this.shooters.Clear();
        // Go through all GameObjects, find, parse, and add to the shooters list
        List<GameObject> gos = new List<GameObject>(GameObject.FindObjectsOfType<GameObject>());
        gos.ForEach((GameObject go) => {
            if (go.name != this.shooterMarkName) { return; }
            Shooter newShooter = Instantiate(
                this.shooterPrefabs[this.prefabIndex],
                go.transform.position,
                go.transform.rotation,
                go.transform.parent);
            this.shooters.Add(newShooter);
        });
    }
// End: Scene initialization

// Mark: Singleton initialization
    public static ShootController shared = null;
    protected override void Awake() {
        base.Awake();
        if (ShootController.shared == null) {
            ShootController.shared = this;
        } else if (ShootController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        this.shootTimeFactor = 1 / this.maxPressTime;
        this.prefabIndex = this.defaultPrefabIndex;
    }
// End: Singleton initialization
}
