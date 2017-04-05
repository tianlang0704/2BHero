using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public void Move(GameObject mark) {
        this.transform.position = mark.transform.position;
    }
}
