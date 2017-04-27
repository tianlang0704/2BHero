using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoInjectable {
    public string menuSceneName = "menu";
    public string gameSceneName = "game";
    public string creditsSceneName = "credits";

    [Inject]
    protected SoundController soundController;

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
                this.soundController.PlayBGMForSetting(sceneName);
            });
        }else {
            this.soundController.StopBGM();
            this.soundController.PlayBGMForSetting(sceneName);
        }
    }

    override protected void Awake() {
        base.Awake();
        // Setting OnSceneLoaded delegate in awake for getting the first call
        SceneManager.sceneLoaded += OnSceneLoaded;
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        });
    }
}
