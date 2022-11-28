using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [SerializeField] int MAX_COUNT = 5;
    [SerializeField] int MAX_TTL = 3;
    [SerializeField] GameObject cfolder;    // customer folder
    static int currentCount = 0;
    static int _maxTTL;
    
    static object _lock = new object();

    [SerializeField] GameObject ground;
    private Bounds bounds;

    void Start()
    {
        _maxTTL = MAX_TTL;
        bounds = ground.GetComponent<Renderer>().bounds;
    }

    void Update()
    {
        if (currentCount < MAX_COUNT)
            Spawn();
    }

    public static void KillMe(CustomerController cc)
    {
        lock (_lock)
        {
            currentCount--;
            Destroy(cc.gameObject);
        }
    }

    public static int GetMaxTTL()
    {
        return _maxTTL;
    }

    private void Spawn()
    {
        for (int i = 0; MAX_COUNT > currentCount; i++)
        {
            GameObject customer = Instantiate<GameObject>(Resources.Load<GameObject>("Customer"));
            customer.transform.position = GetRandomPositionInBounds();
            customer.transform.SetParent(cfolder.transform);
            currentCount++;
        }
    }

    public Vector3 GetRandomPositionInBounds()
    {
        return new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    bounds.max.y,
                    Random.Range(bounds.min.z, bounds.max.z)
                    );
    }
}
