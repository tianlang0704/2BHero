using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class DelegateCenter {
    public Action LoadGameScene;
    public Action LoadMenuScene;
    public Action LoadCreditsScene;
}


public class SceneController : BComponentBase {
    public string menuSceneName = "menu";
    public string gameSceneName = "game";
    public string creditsSceneName = "credits";

    public void LoadGameScene() {
        SceneManager.LoadScene(this.gameSceneName);
    }

    public void LoadMenuScene() {
        SceneManager.LoadScene(this.menuSceneName);
    }

    public void LoadCreditsScene() {
        SceneManager.LoadScene(this.creditsSceneName);
    }

    virtual protected void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        InitSafeBGMForScene(scene.name);
        // Not using delegate because setup needs to be in Awake,
        // which is earlier than delegate setup.
        base.GetDep<ObjectPoolController>().SetupPoolControllerForScene();
        base.GetDep<EnemyController>().SetupEnemyControllerForScene();
        base.GetDep<PlayerController>().SetupPlayerControllerForScene();
        base.GetDep<ShootController>().SetupShootControllerForScene();
    }

    private void InitSafeBGMForScene(string sceneName) {
        if (!this.lifeCycle.isAfterFirstFixedUpdate) {
            this.lifeCycle.OnceOnFirstFixedUpdate(() => {
                base.GetDep<SoundController>().PlayBGMForSetting(sceneName);
            });
        }else {
            base.GetDep<SoundController>().StopBGM();
            base.GetDep<SoundController>().PlayBGMForSetting(sceneName);
        }
    }

// Mark: Singleton initialization
    override protected void Awake() {
        base.Awake();
        // Setting OnSceneLoaded delegate in awake for getting the first call
        SceneManager.sceneLoaded += OnSceneLoaded;
        this.lifeCycle.OnceOnDestroy(() => {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        });
    }

    public void InjectDependencies(
        ObjectPoolController opc,
        EnemyController gc,
        PlayerController pc,
        ShootController sc,
        SoundController soundC

    ) {
        base.ClearDependencies();
        base.AddDep<ObjectPoolController>(opc);
        base.AddDep<EnemyController>(gc);
        base.AddDep<PlayerController>(pc);
        base.AddDep<ShootController>(sc);
        base.AddDep<SoundController>(soundC);
        base.isInjected = true;
    }
// End: Singleton initialization
}
