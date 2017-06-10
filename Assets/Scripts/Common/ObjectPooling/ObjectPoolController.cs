using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



/// <summary>
/// Pool tracker for each poolable prefab
/// </summary>
[System.Serializable]
public class PoolItem {
    public PoolItem() { }
    public PoolItem(Poolable prefab, int cacheAmount = 5, bool shrinkBack = true) {
        this.prefab = prefab;
        this.amountToPool = cacheAmount;
        this.shrinkBack = shrinkBack;
    }
    public Poolable prefab;
    public int amountToPool = 5;
    public bool shrinkBack = false;
    [HideInInspector] public GameObject poolableContainer = null;
    [HideInInspector] public List<Poolable> poolableList = new List<Poolable>();
    [HideInInspector] public int amountInPool { get { return this.poolableList.Count; } }
}










/// <summary>
/// Controller class for poolable objects
/// </summary>
public class ObjectPoolController: MonoInjectable {
    // Inspector settings for the pools
    public string poolContainerName = "PoolContainer";
    public string poolableContainerSuffix = "Pool";

    /// <summary>
    /// Master tracker of the pool settings
    /// </summary>
    [HideInInspector] public List<PoolItem> poolItems = new List<PoolItem>();

    /// <summary>
    /// Master pool container
    /// </summary>
    private GameObject poolContainer = null;

    // Dependencies
    [Inject]
    protected SceneController sceneController;










    // Mark: Recycle methods
    /// <summary>
    /// Recycle is used for Poolable object instead of Destroy
    /// </summary>
    /// <param name="poolable">Poolable object to recycle</param>
    public void Recycle(Poolable poolable) {
        PoolItem pi = poolable.poolItem;
        if (pi == null) { throw new Exception("PoolItem cannot be null"); }
        // 1. Do nothing if object is already on delayed recycle or
        // is already destroyed in events such as scene change
        if(poolable.onDelayedRecycle || poolable == null) { return; }

        // 2. Call recycle event
        if (poolable.recycleDelegate != null) {
            poolable.recycleDelegate();
        }
        // 3. Auto shrink back pool size if specified
        if (pi.shrinkBack && pi.amountInPool > pi.amountToPool) {
            // 3a. If auto shrink specified and is exceeded set number, destroy it
            pi.poolableList.Remove(poolable);
            Destroy(poolable.gameObject);
        } else {
            // 3b. If not auto shrink or not exceeding the set number, deactivate it
            poolable.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Recycle after seconds
    /// </summary>
    /// <param name="poolable">Poolable object to recycle</param>
    /// <param name="t">Delay seconds</param>
    public void Recycle(Poolable poolable, float t) {
        // Check if not double recycling, start coroutine to delay recycle
        if(poolable.onDelayedRecycle) { return; }
        poolable.onDelayedRecycle = true;
        StartCoroutine(DelayedRecycle(poolable, t));
    }
    /// <summary>
    /// Delay routine for delayed recycle
    /// </summary>
    /// <param name="poolable">Poolable object to recycle</param>
    /// <param name="t">Delay seconds</param>
    /// <returns></returns>
    private IEnumerator DelayedRecycle(Poolable poolable, float t) {
        yield return new WaitForSeconds(t);
        poolable.onDelayedRecycle = false;
        Recycle(poolable);
    }
    /// <summary>
    /// Recycle all type of poolable object
    /// </summary>
    /// <param name="prefab">Prefab for a type of poolable object</param>
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
    /// <summary>
    /// Instantiate a poolable, it's either going to activate an idle one or return a new one
    /// and if the pool is set to auto shrink, exceeding ones are going to be destroyed after
    /// recycling
    /// </summary>
    /// <param name="poolable">Poolable prefab to instantiate</param>
    /// <param name="position">Set position</param>
    /// <param name="rotation">Set rotation</param>
    /// <returns></returns>
    public Poolable InstantiatePoolable(
        Poolable poolable,
        Vector3? position = null,
        Quaternion? rotation = null
    ) {
        //If found prefab create or return
        int found = this.poolItems.FindIndex((PoolItem pi) => { return poolable == pi.prefab; });
        if(found == -1) {
            found = CreatePool(poolable);
        }
        return ReturnOrCreatePoolable(this.poolItems[found], position, rotation);
    }
    /// <summary>
    /// Internal method for return or create a poolable object
    /// </summary>
    /// <param name="poolItem">Poolable prefab to instantiate</param>
    /// <param name="position">Set position</param>
    /// <param name="rotation">Set rotation</param>
    /// <returns></returns>
    private Poolable ReturnOrCreatePoolable(
        PoolItem poolItem,
        Vector3? position = null,
        Quaternion? rotation = null
    ){
        Poolable newPoolable = null;
        // 1.If exists an idle object, reset it and return it
        int foundIdx = poolItem.poolableList.FindIndex((Poolable p) => { return !p.gameObject.activeSelf; });
        if(foundIdx != -1) {
            newPoolable = poolItem.poolableList[foundIdx];
            // 1a. Reset transform
            newPoolable.transform.position = position ?? poolItem.prefab.transform.position;
            newPoolable.transform.rotation = rotation ?? poolItem.prefab.transform.rotation;
            newPoolable.transform.localScale = poolItem.prefab.transform.localScale;
            // 1b. Reset rigidbody
            Rigidbody2D rb2d = newPoolable.GetComponent<Rigidbody2D>();
            if (rb2d) {
                rb2d.velocity = new Vector3(0, 0);
                rb2d.angularVelocity = 0;
            }
            // 1c. Reset visibility
            SpriteRenderer sr = newPoolable.GetComponent<SpriteRenderer>();
            if (sr) { sr.enabled = true; }
        }
        // 2. If no idle object exists, create new
        if(newPoolable == null) {
            newPoolable = CreateNewPoolable(poolItem, position, rotation);
        }

        // 3. Set active and return
        newPoolable.gameObject.SetActive(true);
        return newPoolable;
    }
    /// <summary>
    /// Method for creating a new poolable
    /// </summary>
    /// <param name="pi">Pool to create new poolable in</param>
    /// <returns></returns>
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
    /// <summary>
    /// A variant of the method to create new poolable
    /// </summary>
    /// <param name="pi">Pool to create new poolable in</param>
    /// <param name="position">Set position</param>
    /// <param name="rotation">Set rotation</param>
    /// <returns></returns>
    private Poolable CreateNewPoolable(
        PoolItem pi,
        Vector3? position = null,
        Quaternion? rotation = null
    ) {
        // Create new poolable and set position and rotation
        Poolable p = CreateNewPoolable(pi);
        p.transform.position = position ?? pi.prefab.transform.position;
        p.transform.rotation = rotation ?? pi.prefab.transform.rotation;
        // Reset rigidbody
        Rigidbody2D rb2d = p.GetComponent<Rigidbody2D>();
        if (rb2d) {
            rb2d.velocity = new Vector3(0, 0);
            rb2d.angularVelocity = 0;
        }
        return p;
    }
// End: Creation methods










// Mark: Pool methods
    /// <summary>
    /// Create new pool for a poolable object
    /// </summary>
    /// <param name="poolable">Poolable for creating the new pool for</param>
    /// <returns>the index in the poolableItems list</returns>
    private int CreatePool(Poolable poolable) {
        // 1.Find master container, if not exist create one
        if (this.poolContainer == null) {
            this.poolContainer = new GameObject();
            this.poolContainer.name = this.poolContainerName;
        }
        // 2. Find pool contaner, if not exist create one
        int pContainerIdx = this.poolItems.FindIndex((PoolItem poolItem) => {
            return poolItem.poolableContainer.name == poolable.name + this.poolableContainerSuffix;
        });
        if(pContainerIdx != -1) { Debug.Log("Pool already exist"); return pContainerIdx; }
        // 3. Create a new pool container
        PoolItem pi = new PoolItem(poolable, poolable.amountToPool, poolable.isShrinkBack);
        pi.poolableContainer = new GameObject();
        pi.poolableContainer.name = pi.prefab.name + "Pool";
        pi.poolableContainer.transform.parent = this.poolContainer.transform;
        // 4. Fill it according to cache settings
        for (int i = 0; i < pi.amountToPool; ++i) {
            CreateNewPoolable(pi).gameObject.SetActive(false);
        }
        this.poolItems.Add(pi);
        return this.poolItems.Count - 1;
    }
    /// <summary>
    /// Clear all pools
    /// </summary>
    private void ClearPools() {
        foreach (PoolItem pi in this.poolItems) {
            // 1. Clear poolable list
            foreach(Poolable poolable in pi.poolableList) {
                if(poolable == null) { continue; }
                Destroy(poolable);
            }
            pi.poolableList.Clear();

            // 2. Destroy container if still exist
            if (pi.poolableContainer != null) { Destroy(pi.poolableContainer); }
        }
        // 3. Clear all pool records
        this.poolItems.Clear();
    }
// End: Pool methods










// Mark: Scene initialization
    protected override void Awake() {
        base.Awake();
        this.GetComponent<LifeCycleDelegates>().SubOnSceneLoaded((scene, mode) => {
            ClearPools();
        });
    }
// End: Scene initialization
}

