using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public GameObject branchPrefab;
    public GameObject leafPrefab;
    public int poolSize = 20;

    private List<GameObject> branchPool;
    private List<GameObject> leafPool;

    private void Awake()
    {
        branchPool = new List<GameObject>();
        leafPool = new List<GameObject>();

        InitializePool(branchPool, branchPrefab);
        InitializePool(leafPool, leafPrefab);
    }

    private void InitializePool(List<GameObject> pool, GameObject prefab)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            // Parent the object to the ObjectPooler
            obj.transform.SetParent(transform);
            pool.Add(obj);
        }
    }

    public GameObject GetBranch()
    {
        return GetPooledObject(branchPool);
    }

    public GameObject GetLeaf()
    {
        return GetPooledObject(leafPool);
    }

    private GameObject GetPooledObject(List<GameObject> pool)
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        // Parent the object to the ObjectPooler
        obj.transform.SetParent(transform);
    }
}