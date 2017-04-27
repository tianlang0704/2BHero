using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class Inject : Attribute {
    public bool isSingleton = true;

    public Inject(bool isSingleton = true) {
        this.isSingleton = isSingleton;
    }
}
