using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

    // TODO: Create a loadable base and put them all into a list
    // and remodel this class to facilitate more functions
    public ShootController shootControllerPrefab;
    public ObjectPoolController poolControllerPrefab;
    public PlayerController playerControllerPrefab;
    public EnemyController enemyControllerPrefab;
    public GameController gameControllerPrefab;
    public DifficultyContoroller difficultyControllerPrefab;
    public DelegateCenter delegateCenterPrefab;
    public InputController inputControllerPrefab;
    public SceneController sceneControllerPrefab;
    public EffectController effectControllerPrefab;
    public SoundController soundControllerPrefab;

    void Awake() {
		if (ShootController.shared == null)
            Instantiate(this.shootControllerPrefab);
        if (ObjectPoolController.shared == null)
            Instantiate(this.poolControllerPrefab);
        if (PlayerController.shared == null)
            Instantiate(this.playerControllerPrefab);
        if (EnemyController.shared == null)
            Instantiate(this.enemyControllerPrefab);
        if (GameController.shared == null)
            Instantiate(this.gameControllerPrefab);
        if (DifficultyContoroller.shared == null)
            Instantiate(this.difficultyControllerPrefab);
        if (DelegateCenter.shared == null)
            Instantiate(this.delegateCenterPrefab);
        if (InputController.shared == null)
            Instantiate(this.inputControllerPrefab);
        if (SceneController.shared == null)
            Instantiate(this.sceneControllerPrefab);
        if (EffectController.shared == null)
            Instantiate(this.effectControllerPrefab);
        if (SoundController.shared == null)
            Instantiate(this.soundControllerPrefab);
    }
}
