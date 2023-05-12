using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    [SerializeField] GraphsController aiMarketShareGraph;
    [SerializeField] GraphsController appleAvgPriceGraph;
    [SerializeField] GraphsController shirtAvgPriceGraph;
    [SerializeField] GraphsController phoneAvgPriceGraph;
    [SerializeField] GraphsController gpuAvgPriceGraph;
    [SerializeField] GraphsController rolexAvgPriceGraph;
    //[SerializeField] StatisticsController statisticsController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateGraphs(Dictionary<string,float> storeProductAvgPrice, Dictionary<string, float> aiProductAvgPrice, Dictionary<string, float> marketShareData)
    {
        aiMarketShareGraph.UpdateGraph(marketShareData["stores"], marketShareData["ai"]);
        appleAvgPriceGraph.UpdateGraph(storeProductAvgPrice["apple"], aiProductAvgPrice["apple"]);
        shirtAvgPriceGraph.UpdateGraph(storeProductAvgPrice["shirt"], aiProductAvgPrice["shirt"]);
        phoneAvgPriceGraph.UpdateGraph(storeProductAvgPrice["phone"], aiProductAvgPrice["phone"]);
        gpuAvgPriceGraph.UpdateGraph(storeProductAvgPrice["gpu"], aiProductAvgPrice["gpu"]);
        rolexAvgPriceGraph.UpdateGraph(storeProductAvgPrice["rolex"], aiProductAvgPrice["rolex"]);
    }

}
