using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : IPoolable, new() {
    /// <summary>
    /// Retrieves an instance out of the pool and automatically calls its OnReset() method.
    /// </summary>
    public T GetInstance(GameObject prefab = null) {
        T instance;
        if (pool.Count > 0) {
            instance = pool.Dequeue();
        } else {
            instance = new T();
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
                pool.Enqueue(new T());
            }
        }
    }

    Queue<T> pool = new Queue<T>();

}

public interface IPoolable {
    void OnReset();
    void OnRelease();
}
