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


public class SceneController : ControllerBase {
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
        Loader.shared.GetSingleton<ObjectPoolController>().SetupPoolControllerForScene();
        Loader.shared.GetSingleton<EnemyController>().SetupEnemyControllerForScene();
        Loader.shared.GetSingleton<PlayerController>().SetupPlayerControllerForScene();
        Loader.shared.GetSingleton<ShootController>().SetupShootControllerForScene();
    }

    private void InitSafeBGMForScene(string sceneName) {
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        if (!lc.isAfterFirstUpdate) {
            lc.OnceOnFirstdUpdate(() => {
                Loader.shared.GetSingleton<DelegateCenter>().PlayBGMForSetting(sceneName);
            });
        }else {
            Loader.shared.GetSingleton<DelegateCenter>().StopBGM();
            Loader.shared.GetSingleton<DelegateCenter>().PlayBGMForSetting(sceneName);
        }
    }

// Mark: Singleton initialization
    override protected void Awake() {
        base.Awake();
        // Setting OnSceneLoaded delegate in awake for getting the first call
        SceneManager.sceneLoaded += OnSceneLoaded;
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        });
    }

    public override void InitializeDelegates() {
        base.InitializeDelegates();
        // Setup delegates
        DelegateCenter mc = Loader.shared.GetSingleton<DelegateCenter>();
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        mc.LoadGameScene += LoadGameScene;
        mc.LoadMenuScene += LoadMenuScene;
        mc.LoadCreditsScene += LoadCreditsScene;
        lc.OnceOnDestroy(() => {
            mc.LoadGameScene -= LoadGameScene;
            mc.LoadMenuScene -= LoadMenuScene;
            mc.LoadCreditsScene -= LoadCreditsScene;
        });
    }
// End: Singleton initialization
}
