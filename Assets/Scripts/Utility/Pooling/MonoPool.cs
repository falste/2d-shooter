using System.Collections.Generic;
using UnityEngine;

public abstract class MonoPool<T> : MonoBehaviour where T : IPoolable {
    public GameObject prefab;
    public int sizeOnAwake;

    /// <summary>
    /// Retrieves an instance out of the pool and automatically calls its OnReset() method.
    /// </summary>
    public T GetInstance(GameObject prefab = null) {
        T instance;
        if (pool.Count > 0) {
            instance = pool.Dequeue();
        } else {
            instance = Instantiate(prefab, transform).GetComponent<T>();
        }

        instance.OnReset();
        return instance;
    }

    /// <summary>
    /// Inserts an instance into the pool and automatically calls its OnRelease() method.
    /// </summary>
    public void ReleaseInstance(T instance) {
        instance.OnRelease();
        pool.Enqueue(instance);
    }
    public int Poolsize {
        get {
            return pool.Count;
        }
        set {
            while (pool.Count < value) {
                T instance = Instantiate(prefab, transform).GetComponent<T>();
                instance.OnRelease();
                pool.Enqueue(instance);
            }
        }
    }

    protected Queue<T> pool = new Queue<T>();

    virtual protected void Awake() {
        Poolsize = sizeOnAwake;
    }
}
