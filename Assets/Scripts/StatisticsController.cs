using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StatisticsController : MonoBehaviour
{
    [SerializeField] GraphManager graphManager;
    TextMeshProUGUI mainText;
    private float timer = 0f;
    public static int daysPassed;
    public static bool updateGraphs = false;
    Dictionary<string, float> storeProductAvgPrice = new Dictionary<string, float>() {
        {"apple", 0 },
        {"shirt", 0 },
        {"phone", 0 },
        {"gpu", 0 },
        {"rolex", 0 }
    };
    Dictionary<string, float> aiProductAvgPrice = new Dictionary<string, float>() {
        {"apple", 0 },
        {"shirt", 0 },
        {"phone", 0 },
        {"gpu", 0 },
        {"rolex", 0 }
    };
    Dictionary<string, float> marketShareData = new Dictionary<string, float>()
    {
        {"stores", 0 },
        {"ai", 0 }
    };
    void Start()
    {
        daysPassed = 0;
        //graphManager = GetComponent<GraphManager>();
        mainText = GetComponent<TextMeshProUGUI>();
        mainText.text = GetAveragePrices();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.G))
        {
            Canvas canvas = GameObject.Find("GraphsCanvas").GetComponent<Canvas>();

            if (canvas != null)
            {
                canvas.enabled = !canvas.enabled;
            }
        }

        // update the statistics once every 1 second to not overwhelm cpu.
        if (timer >= 1)
        {
            timer = 0;
            mainText.text = GetAveragePrices();
        }
        if (updateGraphs && graphManager != null)
        {
            updateGraphs= false;
            GetTotalMarketShareData();
            GetAveragePrices();
            graphManager.UpdateGraphs(storeProductAvgPrice, aiProductAvgPrice, marketShareData);    
        }
    }

    /// <summary>
    /// return the relative store balance in precentage from the total balances in the market.
    /// if the balance is negative or zero - return zero.
    /// </summary>
    public static float GetMarketShare(StoreController store)
    {
        if (store.GetBalance() <= 0)
            return 0;
        var allStores = GameObject.Find("Stores").GetComponentsInChildren<StoreController>();
        float totalShares = allStores.Sum(x => x.GetBalance());
        if (totalShares <= 0) totalShares = 1; // bad market
        return store.GetBalance() / totalShares * 100;
    }

    public void GetTotalMarketShareData()
    {
        marketShareData["stores"] = 0;
        marketShareData["ai"] = 0;

        PathfindingManager pm =
            GameObject.Find("SimulationController").GetComponent<PathfindingManager>();
        List<GameObject> stores = pm.GetAllStores();
        float storeBalance = 0;
        float aiBalance = 0;
        float balance = 0;
        foreach (GameObject store in stores)
        {
            StoreController sc = store.GetComponent<StoreController>();
            balance = sc.GetBalance();
            if (!sc.isAI) {
                if (balance > 0)
                {
                    storeBalance += balance;
                }
            }
            else{
                if(balance > 0)
                {
                    aiBalance = balance;
                }
            }
        }
        marketShareData["stores"] = storeBalance; // stores.Count-1;// / storeBalance + aiBalance;
        marketShareData["ai"] = aiBalance;// / storeBalance + aiBalance;
    }

    public string GetAveragePrices()
    {
        PathfindingManager pm =
            GameObject.Find("SimulationController").GetComponent<PathfindingManager>();
        List<GameObject> stores = pm.GetAllStores();

        StringBuilder sb = new StringBuilder();
        Dictionary<string, float> productToSum = new Dictionary<string, float>();

        foreach (GameObject store in stores)
        {
            StoreController sc = store.GetComponent<StoreController>();
            foreach (KeyValuePair<string, float> kvp in sc.GetProductPrices())
                if (sc.isAI)
                {
                    aiProductAvgPrice[kvp.Key.ToLower()] = kvp.Value;
                }
                else
                {
                    if (!productToSum.TryAdd(kvp.Key, kvp.Value))
                    {
                        productToSum[kvp.Key] += kvp.Value;
                    }
                }
                
        }

        foreach (KeyValuePair<string, float> kvp in productToSum)
        {
            storeProductAvgPrice[kvp.Key.ToLower()] = kvp.Value/stores.Count - 1; //without the ai store
        }

        sb.Append("\nDays passed: " + daysPassed);
        return sb.ToString();
    }
}

