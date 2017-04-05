using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletWithDamage : Bullet {
    public int defaultDamage = 10;

    [HideInInspector] public int damage;

    virtual protected void OnEnable() {
        this.damage = this.defaultDamage;
    }
}
