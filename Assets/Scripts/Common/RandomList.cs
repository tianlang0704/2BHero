using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomList<T> : List<T> {

    public RandomList() : base() { }
    public RandomList(List<T> list) : base(list) { }

    public T GetRandom() {
        if (this.Count <= 0) { return default(T); }
        System.Random r = new System.Random();
        return this[r.Next(0, this.Count)];
    }
}