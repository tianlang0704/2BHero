using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectController: MonoInjectable {
    [Header("Score Settings")]
    public GameObject ScorePrefab;
    public Vector2 ScoreOffset = new Vector2(2, 1);

    public void ShowScoreForObj(GameObject go, int score) {

    }
}
