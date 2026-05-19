using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct KeyPrefabPair
{
    public string key;
    public PooledObject prefab;
}

[Serializable]
public struct Pool
{
    public PooledObject prefab;
    public Queue<PooledObject> queue;
}

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;

    [SerializeField] private KeyPrefabPair[] prefabs;

    private Dictionary<string, Pool> poolsDict = new Dictionary<string, Pool>();

    void Start()
    {
        instance = this;

        foreach(KeyPrefabPair pair in prefabs)
		{
            Pool p = new Pool();
            p.prefab = pair.prefab;
            p.queue = new Queue<PooledObject>();

            poolsDict.Add(pair.key, p);
        }
    }

    public static bool TryGet(string key, out PooledObject result)
	{
		if (!instance.poolsDict.ContainsKey(key))
		{
            Debug.LogWarning($"UniversalObjectPool don't have a key => {key}");
            result = default;
            return false;
        }

		if (instance.poolsDict[key].queue.Count == 0)
		{
            result = Instantiate(instance.poolsDict[key].prefab);
            result.Refresh();
            return true;
        }

        result = instance.poolsDict[key].queue.Dequeue();
        result.Refresh();
        return true;
    }

    public static void ReturnToPool(string key, PooledObject obj)
	{
        if (!instance.poolsDict.ContainsKey(key))
        {
            Debug.LogWarning($"ObjectPool don't have a key => {key}");
            return ;
        }

        instance.poolsDict[key].queue.Enqueue(obj);
    }
}