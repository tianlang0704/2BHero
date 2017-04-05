using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MessengerController {
    public Action<Poolable> Recycle;
    public Action<Poolable, float> RecycleWithDelay;
    public Action<Poolable> RecycleAll;
    public Func<Poolable, Vector2?, Quaternion?, Poolable> GetPoolable;
    public Func<string, Vector2?, Quaternion?, Poolable> GetPoolableWithName;
}

[System.Serializable]
public class PoolItem {
    public Poolable prefab;
    public int amountToPool = 5;
    public bool shrinkBack = false;
    [HideInInspector] public GameObject poolableContainer;
    [HideInInspector] public List<Poolable> poolableList = new List<Poolable>();
    [HideInInspector] public int amountInPool { get { return this.poolableList.Count; } }
}

public class ObjectPoolController: ControllerBase {
    public string poolContainerName = "PoolContainer";
    public List<PoolItem> poolItems = new List<PoolItem>();

    private GameObject poolContainer = null;

// Mark: Recycle methods
    // Recycle is used to for Poolable object instead of Destroy
    public void Recycle(Poolable poolable) {
        PoolItem pi = poolable.poolItem;
        if (pi == null) { throw new Exception("PoolItem cannot be null"); }
        // Return if object is already on delayed recycle
        if(poolable.onDelayedRecycle) { return; }

        // Auto shrink back pool size if specified
        if (poolable.recycleDelegate != null) {
            poolable.recycleDelegate();
        }
        if (pi.shrinkBack && pi.amountInPool > pi.amountToPool) {
            pi.poolableList.Remove(poolable);
            Destroy(poolable.gameObject);
        } else {
            poolable.gameObject.SetActive(false);
        }
    }

    public void Recycle(Poolable poolable, float t) {
        // Check if not double recycling, start coroutine to delay recycle
        if(poolable.onDelayedRecycle) { return; }
        poolable.onDelayedRecycle = true;
        StartCoroutine(DelayedRecycle(poolable, t));
    }

    private IEnumerator DelayedRecycle(Poolable poolable, float t) {
        yield return new WaitForSeconds(t);
        poolable.onDelayedRecycle = false;
        Recycle(poolable);
    }

    public void RecycleAll(Poolable prefab) {
        this.poolItems.ForEach((PoolItem p) => {
            if(p.prefab == prefab) {
                p.poolableList.ForEach((Poolable poolable) => {
                    poolable.Recycle();
                });
            }
        });
    }
// End: Recycle methods

// Mark: Creation methods
    // GetPooledObject is used for Poolable object instead of initiate
    public Poolable GetPoolable(
        string prefabName,
        Vector2? position = null,
        Quaternion? rotation = null
    ){
        //If found prefabname create or return
        int found = this.poolItems.FindIndex((PoolItem pi) => { return prefabName == pi.prefab.name; });
        if (found == -1) {
            return null;
        }
        return ReturnOrCreatePoolable(this.poolItems[found], position, rotation);
    }

    public Poolable GetPoolable(
        Poolable poolable,
        Vector2? position = null,
        Quaternion? rotation = null
    ){
        //If found prefab create or return
        int found = this.poolItems.FindIndex((PoolItem pi) => { return poolable == pi.prefab; });
        if(found == -1) {
            return null;
        }
        return ReturnOrCreatePoolable(this.poolItems[found], position, rotation);
    }

    // Internal method for return or create a poolable object
    private Poolable ReturnOrCreatePoolable(
        PoolItem poolItem,
        Vector2? position = null,
        Quaternion? rotation = null
    ){
        Poolable newPoolable = null;
        // If exists an idle object
        int foundIdx = poolItem.poolableList.FindIndex((Poolable p) => { return !p.gameObject.activeSelf; });
        if(foundIdx != -1) {
            newPoolable = poolItem.poolableList[foundIdx];
            // Reset transform
            newPoolable.transform.position = position ?? poolItem.prefab.transform.position;
            newPoolable.transform.rotation = rotation ?? poolItem.prefab.transform.rotation;
            newPoolable.transform.localScale = poolItem.prefab.transform.localScale;
            // Reset rigidbody
            Rigidbody2D rb2d = newPoolable.GetComponent<Rigidbody2D>();
            if (rb2d) {
                rb2d.velocity = new Vector2(0, 0);
                rb2d.angularVelocity = 0;
            }
            // Reset visibility
            SpriteRenderer sr = newPoolable.GetComponent<SpriteRenderer>();
            if (sr) { sr.enabled = true; }
        }
        // If no idle object exists, create new
        if(newPoolable == null) {
            newPoolable = CreateNewPoolable(poolItem, position, rotation);
        }

        newPoolable.gameObject.SetActive(true);
        return newPoolable;
    }

    private Poolable CreateNewPoolable(PoolItem pi) {
        // Create new poolable from prefab
        Poolable p = Instantiate(
            pi.prefab,
            pi.prefab.transform.position,
            pi.prefab.transform.rotation,
            pi.poolableContainer.transform);
        p.poolItem = pi;
        p.gameObject.SetActive(false);
        pi.poolableList.Add(p);
        return p;
    }

    private Poolable CreateNewPoolable(
        PoolItem pi,
        Vector2? position = null,
        Quaternion? rotation = null
    ) {
        // Create new poolable and set position and rotation
        Poolable p = CreateNewPoolable(pi);
        p.transform.position = position ?? pi.prefab.transform.position;
        p.transform.rotation = rotation ?? pi.prefab.transform.rotation;
        // Reset rigidbody
        Rigidbody2D rb2d = p.GetComponent<Rigidbody2D>();
        if (rb2d) {
            rb2d.velocity = new Vector2(0, 0);
            rb2d.angularVelocity = 0;
        }
        return p;
    }
// End: Creation methods


// Mark: Singleton initialization
    public static ObjectPoolController shared = null;
    override protected void Awake() {
        base.Awake();
        if(ObjectPoolController.shared == null) {
            ObjectPoolController.shared = this;
        }else if(ObjectPoolController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        CreatePools();
        PreloadPools();
    }

    private void CreatePools() {
        this.poolContainer = new GameObject();
        this.poolContainer.name = this.poolContainerName;
        foreach (PoolItem pi in this.poolItems) {
            pi.poolableContainer = new GameObject();
            pi.poolableContainer.name = pi.prefab.name + "Pool";
            pi.poolableContainer.transform.parent = this.poolContainer.transform;
        }
    }

    private void PreloadPools() {
        foreach (PoolItem pi in this.poolItems) {
            for(int i = 0; i < pi.amountToPool; ++i) {
                CreateNewPoolable(pi).gameObject.SetActive(false);
            }
        }
    }

    protected override void InitializeDelegates() {
        base.InitializeDelegates();
        //Action<Poolable> Recycle;
        //Action<Poolable, float> RecycleWithDelay;
        //Action<Poolable> RecycleAll;
        //public Func<Poolable, Vector2?, Quaternion?, Poolable> GetPoolable;
        //public Func<string, Vector2?, Quaternion?, Poolable> GetPoolableWithName;
        MessengerController mc = MessengerController.shared;
        mc.Recycle += Recycle;
        this.lifeCycle.OnceOnDestroy(() => { mc.Recycle -= Recycle; });
        mc.RecycleWithDelay += Recycle;
        this.lifeCycle.OnceOnDestroy(() => { mc.RecycleWithDelay -= Recycle; });
        mc.RecycleAll += RecycleAll;
        this.lifeCycle.OnceOnDestroy(() => { mc.RecycleAll -= RecycleAll; });
        mc.GetPoolable += GetPoolable;
        this.lifeCycle.OnceOnDestroy(() => { mc.GetPoolable -= GetPoolable; });
        mc.GetPoolableWithName += GetPoolable;
        this.lifeCycle.OnceOnDestroy(() => { mc.GetPoolableWithName -= GetPoolable; });
    }
    // End: Singleton initialization
}

