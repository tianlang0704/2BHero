using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAnimation : MonoBehaviour {

    public Action<EffectAnimation> OnAnimationEnd;

    void AnimationEnd() {
        if(this.OnAnimationEnd != null){ this.OnAnimationEnd(this); };
    }

}
