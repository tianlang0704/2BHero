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









    public void RandomWindowInit(Action<Window> callback) {
        Window rngWindow = this.GetRandomWindow();
        rngWindow.PlayInit();
        Action callbackWithWindow = () => { callback(rngWindow); };
        base.StartCoroutine(this.DelayCoroutine(this.InitAnimationDelay, callbackWithWindow));
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
    } 

    public void OnRPSHit(Window rpsWindow) {
        rpsWindow.ClearRPS();
        base.StartCoroutine(this.DelayCoroutine(this.RPSInterval, ()=>{
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
        base.StartCoroutine(this.DelayCoroutine(this.RPSInterval, () => {
            this.EnableRPS();
        }));
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
