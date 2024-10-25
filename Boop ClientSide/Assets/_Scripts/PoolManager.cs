using System.Collections.Generic;
using UnityEngine;

class Pool {
    public string id;
    public GameObject prefab;
    public Queue<GameObject> pool;

    public Pool(string id, GameObject prefab) {
        this.id = id;
        this.prefab = prefab;
        pool = new Queue<GameObject>();
    }
}

public class PoolManager : MonoBehaviour {
    private Dictionary<string, Pool> m_pools = new Dictionary<string, Pool>();


    public void CreatePool(string id, GameObject prefab) {
        if (string.IsNullOrEmpty(id) || prefab == null) {
            Debug.LogError("Error on params");
            return;
        }

        if (m_pools.ContainsKey(id))
            return;

        m_pools.Add(id, new Pool(id, prefab));
    }

    public void Enqueue(string id, GameObject go) {
        if (string.IsNullOrEmpty(id) || go == null) {
            Debug.LogError("Error on params");
            return;
        }

        if (!m_pools.ContainsKey(id))
            return;

        m_pools[id].pool.Enqueue(go);
        go.transform.SetParent(transform);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;
        go.SetActive(false);
    }

    public GameObject Dequeue(string id, Transform parent) {
        if (string.IsNullOrEmpty(id)) {
            Debug.LogError("Error on params");
            return null;
        }

        if (!m_pools.ContainsKey(id))
            return null;

        GameObject go = null;

        if (m_pools[id].pool.Count > 0)
            go = m_pools[id].pool.Dequeue();
        else
            go = Instantiate(m_pools[id].prefab);

        go.SetActive(true);
        go.transform.SetParent(parent);
        return go;
    }
}
