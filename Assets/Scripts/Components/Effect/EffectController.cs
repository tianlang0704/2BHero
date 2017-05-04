using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class EffectController: MonoInjectable {
    [Header("Score Settings")]
    public EffectScore scorePrefab;




    [Inject]
    protected ObjectPoolController objectPoolController;




    public void ShowScoreAt(Vector3 position, int score) {
        Poolable p = this.scorePrefab.GetComponent<Poolable>();
        EffectScore scoreInst = 
            this.objectPoolController.InstantiatePoolable(p, position)
            .GetComponent<EffectScore>();
        scoreInst.scoreAnimation.GetComponent<Text>().text = "+" + score.ToString();
    }
    public void ShowStackOfScoreAt(Vector3 position, int score) {

    }
}
