using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class StatisticsController : MonoBehaviour
{
    TextMeshProUGUI mainText;

    void Start()
    {
        mainText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        mainText.text = GetAveragePrices();
    }

    public string GetAveragePrices()
    {
        PathfindingManager pm =
            GameObject.Find("SimulationController").GetComponent<PathfindingManager>();
        List<GameObject> stores = pm.GetAllStores();

        StringBuilder sb = new StringBuilder();
        sb.Append("Average Prices:\n");
        Dictionary<string, float> productToSum = new Dictionary<string, float>();

        foreach (GameObject store in stores)
        {
            StoreController sc = store.GetComponent<StoreController>();
            foreach (KeyValuePair<string, float> kvp in sc.GetProductPrices())
                if (!productToSum.TryAdd(kvp.Key, kvp.Value))
                {
                    productToSum[kvp.Key] += kvp.Value;
                }
        }

        foreach (KeyValuePair<string, float> kvp in productToSum)
        {
            sb.Append(kvp.Key + ": " + (kvp.Value / stores.Count) + "\n");
        }
        return sb.ToString();
    }
}
