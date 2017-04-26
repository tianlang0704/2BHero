using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectController: ControllerBase {
    [Header("Score Settings")]
    public GameObject ScorePrefab;
    public Vector2 ScoreOffset = new Vector2(2, 1);

    public void ShowScoreForObj(GameObject go, int score) {

    }

// Mark: Singleton initialization
    public static EffectController shared = null;
    override protected void Awake() {
        base.Awake();
        if (EffectController.shared == null) {
            EffectController.shared = this;
        } else if (EffectController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public override void InitializeDelegates() {
        base.InitializeDelegates();

    }
// End: Singleton initialization
}
