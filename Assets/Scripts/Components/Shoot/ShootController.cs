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
public class ShootController : MonoInjectable {
    public float maxPressTime = 0.35f;
    public List<Shooter> shooterPrefabs = new List<Shooter>();
    public int defaultPrefabIndex = 0;
    public string shooterMarkName = "ShooterMark";

    private List<Shooter> shooters = new List<Shooter>();
    private float shootTimeFactor;
    private int prefabIndex;

    [Inject]
    protected DelegateCenter delegateCenter;
    [Inject]
    protected InputController inputController;

    private void Update() {
        this.inputController.VariableDurationShoot((timePassed, isShoot)=> {
            // =======================================
            // ========Shoot Press End Handler========
            // =======================================
            if (timePassed > this.maxPressTime) { timePassed = this.maxPressTime; }
            // Do last press in progress update to set progress to 0
            if (this.delegateCenter.OnShootHoldUpdate != null) {
                this.delegateCenter.OnShootHoldUpdate(timePassed * this.shootTimeFactor);
            }
            // Stop aiming no matter what (is move or shoot)
            this.shooters.ForEach((Shooter s) => { s.AimStop(); });
            // Shoot if is shoot (not swiping or move)
            if (isShoot) {
                this.shooters.ForEach((Shooter s) => { s.Shoot(timePassed * this.shootTimeFactor); });
            }
            if (this.delegateCenter.OnShootEnd != null) { this.delegateCenter.OnShootEnd(); }
        }, (timePassed)=> {
            // ===============================================
            // ========Shoot Press In Progress Handler========
            // ===============================================
            // Play aim animation when overlapped animation ends and key is still pressed
            this.shooters.ForEach((Shooter s) => { s.Aim(); });
            // Call shoot press on hold event
            if (this.delegateCenter.OnShootHoldUpdate != null) {
                // Use shoot time factor to make the parameter range from 0 to 1
                if (timePassed > this.maxPressTime) { timePassed = this.maxPressTime; }
                this.delegateCenter.OnShootHoldUpdate(timePassed * this.shootTimeFactor);
            }
        }, () => {
            // =========================================
            // ========Shoot Press Start Handler========
            // =========================================
            this.shooters.ForEach((Shooter s) => { s.Aim(); });
            // Call shoot press begin
            if (this.delegateCenter.OnShootStart != null) { this.delegateCenter.OnShootStart(); }
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

    protected override void Awake() {
        base.Awake();
        this.shootTimeFactor = 1 / this.maxPressTime;
        this.prefabIndex = this.defaultPrefabIndex;
        this.GetComponent<LifeCycleDelegates>().SubOnSceneLoaded((scene, mode) => {
            SetupShootControllerForScene();
        });
    }
}
