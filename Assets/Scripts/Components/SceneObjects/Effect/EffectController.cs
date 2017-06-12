using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class EffectController: MonoInjectable {
    /// <summary>
    /// Score effect prefab
    /// </summary>
    [Header("Score Settings")]
    public EffectScore scorePrefab;
    
    //Dependencies
    [Inject]
    protected ObjectPoolController objectPoolController;

    /// <summary>
    /// Method to show a score in scene
    /// </summary>
    /// <param name="position">Position to show score</param>
    /// <param name="score">Score value</param>
    public void ShowScoreAt(Vector3 position, int score, Color textColor) {
        // Instantiate a score template
        Poolable p = this.scorePrefab.GetComponent<Poolable>();
        EffectScore scoreInst =
            this.objectPoolController.InstantiatePoolable(p, position)
            .GetComponent<EffectScore>();

        // Set its sign
        string sign = "";
        if (score > 0) {
            sign = "+";
        }
        scoreInst.scoreAnimation.GetComponent<Text>().text = sign + score.ToString();
        scoreInst.scoreAnimation.GetComponent<Text>().color = textColor;
    }

    public void ShowStackOfScoreAt(Vector3 position, int score) {

    }
}
