using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomList<T> : List<T> {
    public RandomList() : base() { }
    public RandomList(List<T> list) : base(list) { }

    private System.Random r = new System.Random();

    public T GetRandom() {
        if (this.Count <= 0) { return default(T); }
        return this[this.r.Next(0, this.Count)];
    }
}