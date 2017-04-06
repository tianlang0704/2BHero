using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

    public ShootController shootControllerPrefab;
    public ObjectPoolController poolControllerPrefab;
    public PlayerController playerControllerPrefab;
    public EnemyController enemyControllerPrefab;
    public GameController gameControllerPrefab;
    public DifficultyContoroller difficultyControllerPrefab;
    public MessengerController messengerControllerPrefab;
    public InputController inputControllerPrefab;

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
        if (MessengerController.shared == null)
            Instantiate(this.messengerControllerPrefab);
        if (InputController.shared == null)
            Instantiate(this.inputControllerPrefab);
    }
}
