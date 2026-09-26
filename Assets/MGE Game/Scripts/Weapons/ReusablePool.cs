using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ReusablePool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    private ObjectPool<GameObject> pool;
    private List<GameObject> activeObjects = new List<GameObject>(); // TODO: Coroutines don't stop after pool.Get() calls
    public List<GameObject> ActiveObjects { get { return activeObjects; } }

    public int maxActiveCount = 9;
    private void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: CreateObject,
            actionOnGet: OnGetObject,
            actionOnRelease: OnReleaseObject,
            actionOnDestroy: OnDestroyObject,
            collectionCheck: true,
            defaultCapacity: maxActiveCount,
            maxSize: maxActiveCount
        );
    }
    private void OnDestroy()
    {
        pool?.Dispose();
    }

    private GameObject CreateObject()
    {
        GameObject go = GameObject.Instantiate(prefab);
        go.SetActive(false);
        return go;
    }

    private void OnGetObject(GameObject obj)
    {
        obj.SetActive(true);
        activeObjects.Add(obj);
    }

    private void OnReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
        activeObjects.Remove(obj);
    }

    private void OnDestroyObject(GameObject obj)
    {
        activeObjects.Remove(obj);
        Destroy(obj);
    }

    private void ReleaseOldestObject()
    {
        if (activeObjects.Count > 0)
        {
            GameObject oldest = activeObjects[0];
            pool.Release(oldest);
        }
    }

    // ============================== Public Methods
    public GameObject GetObject()
    {
        if (activeObjects.Count >= maxActiveCount)
            ReleaseOldestObject();

        return pool.Get();
    }
    public void Release(GameObject go)
    {
        pool.Release(go);
    }
}
