using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoInjectable {
    public EnemyGoalRPS rpsGoal;
    public GameObject rpsIconContainer;
    public int FloorLevel = 0;

    // Dependencies
    [Inject]
    protected WindowController windowController;
    [Inject]
    protected ObjectPoolController objectPoolController;











    public void PlayInit() {
        this.GetComponent<Animator>().SetTrigger("Init");
    }

    public void PlayRPS() {
        this.GetComponent<Animator>().SetBool("RPS", false);
        this.RandomizeRPS();
        this.GetComponent<Animator>().SetBool("RPS", true);
        this.EnableRPSGoal();
    }

    public void ClearRPS() {
        this.GetComponent<Animator>().SetBool("RPS", false);
        this.DisableRPSGoal();
    }

    private void EnableRPSGoal() {
        this.rpsGoal.gameObject.SetActive(true);
    }

    private void DisableRPSGoal() {
        this.rpsGoal.gameObject.SetActive(false);
    }

    private void RandomizeRPS() {
        this.rpsGoal.RPS();
    }

    private void ShowRPSIcon() {
        this.SetRPSIcon(this.rpsGoal.currentEnemy);
    }

    private void ClearRPSIcon() {
        Enemy[] iconList = this.rpsIconContainer.GetComponentsInChildren<Enemy>();
        foreach(Enemy e in iconList) {
            Destroy(e.gameObject);
        }
    }

    private void SetRPSIcon(Enemy eFab) {
        ClearRPSIcon();
        Enemy e = Instantiate(eFab, this.rpsIconContainer.transform);
        e.gameObject.SetActive(true);
        e.Iconize(true);
    }

    private void OnRPSHit(EnemyGoalRPS goal) {
        this.windowController.OnRPSHit(this);
    }

    protected override void Start() {
        base.Start();
        // Register window and setup unregistration
        this.windowController.RegisterWindow(this);
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            this.windowController.UnregisterWindow(this);
        });

        // Randomize RPS for this window
        this.RandomizeRPS();

        // Setup RPS goal hit event
        this.DisableRPSGoal();
        this.rpsGoal.OnRPSHit += OnRPSHit;
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            this.rpsGoal.OnRPSHit -= OnRPSHit;
        });
    }
}
