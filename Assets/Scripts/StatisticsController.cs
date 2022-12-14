using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class StatisticsController : MonoBehaviour
{
    TextMeshProUGUI mainText;
    private float timer = 0f;

    void Start()
    {
        mainText = GetComponent<TextMeshProUGUI>();
        mainText.text = GetAveragePrices();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // update the statistics once every 1 second to not overwhelm cpu.
        if (timer >= 1)
        {
            timer = 0;
            mainText.text = GetAveragePrices();
        }
    }

    private string GetAveragePrices()
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

        var cm = GameObject.Find("SimulationController").GetComponent<CustomerManager>();
        sb.Append("Customer Count: " + cm.GetCustomerCount());
        return sb.ToString();
    }
}
