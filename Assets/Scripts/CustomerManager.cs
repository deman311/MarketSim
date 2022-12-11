using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [SerializeField] GameObject customerFolder;
    [SerializeField] GameObject spawnArea;

    readonly CustomerParams cp = new CustomerParams();

    static int currentCount = 0;
    static int _maxTTL;

    static object _lock = new object();

    private Bounds bounds;

    void Start()
    {
        _maxTTL = cp.TTL;
        bounds = spawnArea.GetComponent<Renderer>().bounds;
    }

    void Update()
    {
        if (currentCount < cp.CUSTOMER_COUNT)
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

    public float GetAveragePrice(string prodName)
    {
        float avg = 0;
        int amount = 0;

        foreach (CustomerController cc in GameObject.Find("Customers").GetComponentsInChildren<CustomerController>())
            if (cc.GetProductPrices().TryGetValue(prodName, out float value))
            {
                avg += value;
                amount++;
            }

        if (amount == 0)
            return 0;

        return avg / amount;
    }

    private void Spawn()
    {
        for (int i = 0; cp.CUSTOMER_COUNT > currentCount; i++)
        {
            GameObject customer = Instantiate<GameObject>(Resources.Load<GameObject>("Customer"));
            customer.transform.position = GetRandomPositionInBounds();
            customer.transform.SetParent(customerFolder.transform);
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
