using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class Effect : MonoInjectable {
    [HideInInspector]
    public List<EffectAnimation> animationList;

    public void EffectEnd() {
        this.GetComponent<Poolable>().Recycle();
    }

    private void OnAnimationEnd(EffectAnimation aniEnded) {
        // 1. Deactivate the animation object
        aniEnded.gameObject.SetActive(false);

        // 2. Check if all animation objects are disabled
        bool isAllEnded = true;
        foreach(EffectAnimation animation in this.animationList) {
            if (animation.gameObject.activeSelf) { isAllEnded = false; break; }
        }                                              

        // 3. If yes, end this effect
        if (isAllEnded) { EffectEnd(); }
    }

    protected override void Awake() {
        base.Awake();
        this.animationList = new List<EffectAnimation>(this.gameObject.GetComponentsInChildren<EffectAnimation>());
        this.animationList.ForEach((animation) => {
            animation.OnAnimationEnd += OnAnimationEnd;
        });
        this.GetComponent<LifeCycleDelegates>().OnceOnDestroy(() => {
            this.animationList.ForEach((animation) => {
                animation.OnAnimationEnd -= OnAnimationEnd;
            });
        });
    }

    protected virtual void OnEnable() {
        this.animationList.ForEach((animation) => {
            animation.gameObject.SetActive(true);
        });
    }
}
