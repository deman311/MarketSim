using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [SerializeField] GameObject customerFolder;
    [SerializeField] GameObject spawnArea;

    readonly CustomerParams cp = new CustomerParams();

    static int currentCount = 0;
    static int maxTTL;

    static readonly object _lock = new object();

    private Bounds bounds;

    void Start()
    {
        maxTTL = cp.TTL;
        bounds = spawnArea.GetComponent<Renderer>().bounds;
        Spawn(firstDay: true);
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
        return maxTTL;
    }

    public int GetCustomerCount()
    {
        return currentCount;
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

    public void Spawn(bool firstDay = false)
    {
        for (int i = currentCount; i < cp.CUSTOMER_COUNT; i++)
        {
            if (firstDay || Random.Range(0, 2) == 0)    // 50% to spawn a new customer
            {
                GameObject customer = Instantiate<GameObject>(Resources.Load<GameObject>("Customer"), GetRandomPositionInBounds(), Quaternion.identity, customerFolder.transform);
                currentCount++;
            }
        }
    }

    public Vector3 GetRandomPositionInBounds()
    {
        return new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    0,
                    Random.Range(bounds.min.z, bounds.max.z)
                    );
    }
}
