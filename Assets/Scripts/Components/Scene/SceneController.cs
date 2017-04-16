using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class DelegateCenter {
    public Action LoadGameScene;
    public Action LoadMenuScene;
}

[RequireComponent(typeof(AudioSource))]
public class SceneController : ControllerBase {
    public string menuSceneName = "menu";
    public string gameSceneName = "game";
    public bool bgmEnabled = true;
    [SerializeField] private List<AudioClip> _menuBGMs = new RandomList<AudioClip>();
    public RandomList<AudioClip> menuBGMs { get { return (RandomList<AudioClip>)_menuBGMs; } }
    [SerializeField] private List<AudioClip> _gameBGMs = new RandomList<AudioClip>();
    public RandomList<AudioClip> gameBGMs { get { return (RandomList<AudioClip>)_gameBGMs; } }

    private AudioSource audioSource;
    private bool isBGMPlaying = false;

    public void LoadGameScene() {
        SceneManager.LoadScene(this.gameSceneName);
    }

    public void LoadMenuScene() {
        SceneManager.LoadScene(this.menuSceneName);
    }

    virtual protected void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        StopBGM();
        if (scene.name == this.gameSceneName) {
            // Not using delegate because setup needs to be in Awake,
            // which is earlier than delegate setup.
            ObjectPoolController.shared.SetupPoolControllerForScene();
            EnemyController.shared.SetupEnemyControllerForScene();
            PlayerController.shared.SetupPlayerControllerForScene();
            ShootController.shared.SetupShootControllerForScene();
        }
    }

// Mark: BGM functions
    public void EnableBGM() {
        if (this.bgmEnabled) { return; }
        this.bgmEnabled = true;
        PlayBGM();
    }

    public void DisableBGM() {
        if (!this.bgmEnabled) { return; }
        this.bgmEnabled = false;
        StopBGM();
    }

    public void StopBGM() {
        if (!this.audioSource.isPlaying) { return; }
        this.audioSource.Stop();
    }

    public void PlayBGM() {
        if (this.audioSource.isPlaying) { return; }
        if (SceneManager.GetActiveScene().name == this.gameSceneName) {
            this.audioSource.PlayOneShot(this.gameBGMs.GetRandom());
        }else if (SceneManager.GetActiveScene().name == this.menuSceneName) {
            this.audioSource.PlayOneShot(this.menuBGMs.GetRandom());
        }
    }

    public void PlayBGMContinuously() {
        if (!this.bgmEnabled) { return; }
        PlayBGM();
    }
// End: BGM functions

    protected virtual void Update() {
        PlayBGMContinuously();
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
        this.audioSource = this.GetComponent<AudioSource>();
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
