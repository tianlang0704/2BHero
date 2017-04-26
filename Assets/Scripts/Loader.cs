using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Loader : MonoBehaviour {
    // TODO: Create a loadable base and put them all into a list
    // and remodel this class to facilitate more functions
    public List<ControllerBase> prefabs = new List<ControllerBase>();
    private List<ControllerBase> singletons = new List<ControllerBase>();

    void Awake() {
        this.prefabs.ForEach((controller) => {
            FieldInfo fi = controller.GetType().GetField("shared");
            if(fi == null) { throw new Exception("Type does not have a 'shared' variable" + controller); }
            if(fi.GetValue(null) == null) {
                this.singletons.Add(Instantiate(controller));
            }
        });

        this.singletons.ForEach((controller)=> {
            if (controller.isDelegatesInitialzed) { return; }
            MethodInfo mi = controller.GetType().GetMethod("InitializeDelegates");
            if(mi == null) { Debug.Log("Type does not have InitializeDelegates method: " + controller); return; }
            mi.Invoke(controller, null);
        });
    }
}
