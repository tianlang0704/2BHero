using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class DelegateCenter {
    public Action LoadGameScene;
    public Action LoadMenuScene;
}

public class SceneController : ControllerBase {
    public string gameSceneName = "game";
    public string menuSceneName = "menu";

    public void LoadGameScene() {
        SceneManager.LoadScene(this.gameSceneName);
    }

    public void LoadMenuScene() {
        SceneManager.LoadScene(this.menuSceneName);
    }

    virtual protected void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if(scene.name == "game") {
            // Not using delegate because setup needs to be in Awake,
            // which is earlier than delegate setup.
            ObjectPoolController.shared.SetupPoolControllerForScene();
            EnemyController.shared.SetupEnemyControllerForScene();
            PlayerController.shared.SetupPlayerControllerForScene();
            ShootController.shared.SetupShootControllerForScene();
        }
    }

// Mark: Singleton initialization
    public static SceneController shared = null;
    override protected void Awake() {
        base.Awake();
        if (SceneController.shared == null) {
            SceneController.shared = this;
        } else if (SceneController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        this.lifeCycle.OnceOnDestroy(() => {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        });
    }

    protected override void InitializeDelegates() {
        base.InitializeDelegates();
        // Setup delegates
        DelegateCenter mc = DelegateCenter.shared;
        LifeCycleDelegates lc = this.GetComponent<LifeCycleDelegates>();
        mc.LoadGameScene += LoadGameScene;
        mc.LoadMenuScene += LoadMenuScene;
        lc.OnceOnDestroy(() => {
            mc.LoadGameScene -= LoadGameScene;
            mc.LoadMenuScene -= LoadMenuScene;
        });
    }
    // End: Singleton initialization
}
