using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowController : MonoInjectable {
    public float InitAnimationDelay = 2f;
    public float RPSInterval = 5f;

    [Inject]
    EffectController effectController;
    [Inject]
    DelegateCenter delegateCenter;

    private RandomList<Window> windowList = new RandomList<Window>();
    private bool isRPSEnabled = true;
    private Coroutine initDelayCoroutine = null;
    private Coroutine rpsDelayCoroutine = null;









    public void RandomWindowInit(Action<Window> callback) {
        Window rngWindow = this.GetRandomWindow();
        rngWindow.PlayInit();
        Action callbackWithWindow = () => { callback(rngWindow); };
        this.initDelayCoroutine = base.StartCoroutine(this.DelayCoroutine(this.InitAnimationDelay, callbackWithWindow));
    }

    public void RandomWindowRPS() {
        this.GetRandomWindow().PlayRPS();
    }

    public void ClearAllWindowRPS() {
        this.windowList.ForEach((window) => {
            window.ClearRPS();
        });
    }

//Mark: RPS methods
    public void EnableRPS() {
        this.isRPSEnabled = true;
        this.RandomWindowRPS();
    }

    public void DisableRPS() {
        this.isRPSEnabled = false;
        this.ClearAllWindowRPS();
        if (this.initDelayCoroutine != null) {
            base.StopCoroutine(this.initDelayCoroutine);
            this.initDelayCoroutine = null;
        }
        if(this.rpsDelayCoroutine != null) {
            base.StopCoroutine(this.rpsDelayCoroutine);
            this.rpsDelayCoroutine = null;
        }
    } 

    public void OnRPSHit(Window rpsWindow = null) {
        if (rpsWindow != null) { rpsWindow.ClearRPS(); }
        if (this.isRPSEnabled) {
            this.DelayedRPS(this.RPSInterval);
        }
    }

    private void DelayedRPS(float seconds) {
        this.isRPSEnabled = true;
        if (this.rpsDelayCoroutine != null) { base.StopCoroutine(this.initDelayCoroutine); }
        this.rpsDelayCoroutine = base.StartCoroutine(this.DelayCoroutine(seconds, () => {
            this.rpsDelayCoroutine = null;
            if (!this.isRPSEnabled) { return; }
            this.RandomWindowRPS();
        }));
    }
//End: RPS methods

// Mark: Window accessing methods
    public Window GetRandomWindow() {
        return this.windowList.GetRandom();
    }

    public void RegisterWindow(Window window) {
        this.windowList.Add(window);
    }

    public void UnregisterWindow(Window window) {
        this.windowList.Remove(window);
    }

    public void UnregisterAllWindows() {
        this.windowList.Clear();
    }
// End: Window accessing methods

// Mark: General internal functions
    private void OnGameStart() {
        this.DelayedRPS(this.RPSInterval);
    }

    private void OnGameOver() {
        this.DisableRPS();
    }

    protected override void Start() {
        base.Start();
        this.delegateCenter.OnGameStart += OnGameStart;
        this.delegateCenter.OnGameOver += OnGameOver;
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            this.UnregisterAllWindows();
            this.delegateCenter.OnGameStart -= OnGameStart;
            this.delegateCenter.OnGameOver -= OnGameOver;
        });
    }

    private IEnumerator DelayCoroutine(float delay, Action callback) {
        yield return new WaitForSeconds(delay);
        callback();
    }
// End: General internal functions
}
