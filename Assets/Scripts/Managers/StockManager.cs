using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StockManager : MonoBehaviour
{
    int maxLevel = 1;
    public readonly static List<string> prodNames = new List<string> { "Apple", "Shirt", "Phone", "GPU", "Rolex" };
    public Dictionary<string, float> productToProduction = new Dictionary<string, float>
    {
        {prodNames[0], 1 },     // 1 - 20
        {prodNames[1], 10 },    // 10 - 100
        {prodNames[2], 75 },    // 75 - 500
        {prodNames[3], 300 },   // 300 - 2000
        {prodNames[4], 1500 }   // 1500 - 5000
    };
    Dictionary<string, float> productToMaxPrice = new Dictionary<string, float>
    {
        {prodNames[0], 20 },    // 1 - 20
        {prodNames[1], 100 },   // 15 - 100
        {prodNames[2], 500 },   // 90 - 500
        {prodNames[3], 2000 },  // 300 - 2000
        {prodNames[4], 5000 }   // 1500 - 5000
    };
    Dictionary<string, int> productToTax = new Dictionary<string, int>
    {
        {prodNames[0], 1 },
        {prodNames[1], 5 },
        {prodNames[2], 60 },
        {prodNames[3], 250 },
        {prodNames[4], 700 }
    };
    /// <summary>
    /// The minimum store level per product required to be able to sell it.
    /// </summary>
    Dictionary<string, int> productToLevel = new Dictionary<string, int>
    {
        {prodNames[0], 1 },
        {prodNames[1], 1 },
        {prodNames[2], 2 },
        {prodNames[3], 2 },
        {prodNames[4], 3 }
    };
    /// <summary>
    /// The precentage per product it is likely to be generated in the shopping list for each new generated customer.
    /// </summary>
    Dictionary<string, int> productToScarsity = new Dictionary<string, int>
    {
        {prodNames[0], 50 },
        {prodNames[1], 35 },
        {prodNames[2], 25 },
        {prodNames[3], 15 },
        {prodNames[4], 5 }
    };

    public List<float> GetRewardsPerProduct(List<int> sold, List<int> held)
    {
        List<float> rewards = new List<float>();
        var ptp = productToProduction.Values.ToList();
        var pth = productToTax.Values.ToList();
        for (int i = 0; i < sold.Count; i++)
            //rewards.Add((sold[i] * ptp[i]) / 10f);
            rewards.Add((sold[i] * ptp[i] - held[i] * pth[i]) / 10f);
        return rewards;
    }

    public int GetScarsityOfProduct(string prodName)
    {
        return productToScarsity[prodName];
    }

    public List<float> GetAllAvgPrices()
    {
        List<float> avgs = new List<float>();
        foreach (string prodName in prodNames)
            avgs.Add(GetAveragePrice(prodName));
        return avgs;
    }

    public float GetAveragePrice(string prodName)
    {
        float avg = 0;
        int amount = 0;

        foreach (var sc in GameObject.Find("Stores").GetComponentsInChildren<StoreController>())
            if (sc.GetProductPrices().TryGetValue(prodName, out float value))
            {
                avg += value;
                amount++;
            }

        if (amount == 0) // prevent NaN
            return 0;
        return avg / amount;
    }

    public int GetProductTax(string prodName)
    {
        return productToTax[prodName];
    }

    public int GetMaxLevel()
    {
        return maxLevel;
    }

    public float GetProductionPrice(string prodName)
    {
        return productToProduction[prodName];
    }

    public float GetMaxPrice(string prodName)
    {
        return productToMaxPrice[prodName];
    }

    public List<string> GetBuyList(int level)
    {
        if (maxLevel < level)
            maxLevel = level;
        List<string> temp = new List<string>();
        foreach (KeyValuePair<string, int> kvp in productToLevel)
        {
            if (kvp.Value <= level)
                temp.Add(kvp.Key);
        }
        return temp;
    }
}