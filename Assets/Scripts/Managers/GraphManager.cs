using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    [SerializeField] PieChartController aiMarketShareGraph;
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
        aiMarketShareGraph.ShowPieGraph(marketShareData["stores"], marketShareData["ai"]);
        appleAvgPriceGraph.UpdateLineGraph(storeProductAvgPrice["apple"], aiProductAvgPrice["apple"]);
        shirtAvgPriceGraph.UpdateLineGraph(storeProductAvgPrice["shirt"], aiProductAvgPrice["shirt"]);
        phoneAvgPriceGraph.UpdateLineGraph(storeProductAvgPrice["phone"], aiProductAvgPrice["phone"]);
        gpuAvgPriceGraph.UpdateLineGraph(storeProductAvgPrice["gpu"], aiProductAvgPrice["gpu"]);
        rolexAvgPriceGraph.UpdateLineGraph(storeProductAvgPrice["rolex"], aiProductAvgPrice["rolex"]);
    }

}
