using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public static GameObject endPos;
    List<GameObject> stores = new List<GameObject>();

    public static object _StoreLock = new object();

    void Start()
    {
        endPos = GameObject.Find("EndPosition");
        //foreach (GameObject store in GameObject.FindGameObjectsWithTag("Store"))
          //  SafeAdd(store);
    }

    public void SetStores(List<GameObject> stores)
    {
        this.stores = stores;
    }

    public void SafeAdd(GameObject store)
    {
        lock (_StoreLock)
        {
            stores.Add(store);
        }
    }

    public void SafeRemove(GameObject store)
    {
        lock (_StoreLock)
        {
            stores.Remove(store);
            //store.SetActive(false);
            store.GetComponent<StoreController>().SetLevel(0);
        }
    }

    public List<GameObject> GetPathList(Vector3 pos)
    {
        List<(GameObject, float)> storeToDist = new();
        foreach (GameObject store in stores)
            storeToDist.Add((store, Vector3.Distance(pos, store.transform.position)));
        return Sort(storeToDist).ConvertAll<GameObject>(a => a.Item1);
    }

    public List<GameObject> GetAllStores()
    {
        return new List<GameObject>(stores.ToArray());
    }

    private List<(GameObject, float)> Sort(List<(GameObject, float)> unsorted)
    {
        if (unsorted.Count <= 1)
            return unsorted;

        var left = new List<(GameObject, float)>();
        var right = new List<(GameObject, float)>();
        int middle = unsorted.Count / 2;

        for (int i = 0; i < unsorted.Count; i++)
            if (i < middle)
                left.Add(unsorted[i]);
            else
                right.Add(unsorted[i]);

        left = Sort(left);
        right = Sort(right);

        return MergedList(left, right);
    }

    private List<(GameObject, float)> MergedList(List<(GameObject, float)> left, List<(GameObject, float)> right)
    {
        var result = new List<(GameObject, float)>();
        while (left.Count > 0 || right.Count > 0)
        {
            if (left.Count > 0 && right.Count > 0)
            {
                if (left[0].Item2 < right[0].Item2)
                {
                    result.Add(left[0]);
                    left.Remove(left[0]);
                }
                else
                {
                    result.Add(right[0]);
                    right.Remove(right[0]);
                }
            }
            else if (left.Count > 0)
            {
                result.Add(left[0]);
                left.Remove(left[0]);
            }
            else if (right.Count > 0)
            {
                result.Add(right[0]);
                right.Remove(right[0]);
            }
        }

        return result;
    }
}
